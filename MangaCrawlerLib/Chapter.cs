using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Threading;
using System.Diagnostics;
using TomanuExtensions;
using TomanuExtensions.Utils;
using System.Collections.ObjectModel;
using Ionic.Zip;
using System.Threading.Tasks;

namespace MangaCrawlerLib
{
    public class Chapter : Entity 
    {
        #region PagesCachedList
        private class PagesCachedList : CachedList<Page>
        {
            private Chapter m_chapter;

            public PagesCachedList(Chapter a_chapter)
            {
                m_chapter = a_chapter;
            }

            protected override void EnsureLoaded()
            {
                lock (m_lock)
                {
                    if (m_list != null)
                        return;

                    m_list = Catalog.LoadChapterPages(m_chapter);
                }
            }
        }
        #endregion

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CancellationTokenSource m_cancellation_token_source;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ChapterState m_state;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private PagesCachedList m_pages;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Object m_state_lock = new Object();

        public Serie Serie { get; private set; }
        public string Title { get; internal set; }
        public bool Visited { get; internal set; }

        internal Chapter(Serie a_serie, string a_url, string a_title)
            : this(a_serie, a_url, a_title, Catalog.NextID(), ChapterState.Initial, 0, false)
        {
        }

        internal Chapter(Serie a_serie, string a_url, string a_title, ulong a_id, ChapterState a_state,
            ulong a_limiter_order, bool a_visited)
            : base(a_id)
        {
            Visited = a_visited;
            m_pages = new PagesCachedList(this);
            LimiterOrder = a_limiter_order;
            Serie = a_serie;
            URL = HtmlDecode(a_url);
            m_state = a_state;

            if (m_state == ChapterState.Cancelling)
                m_state = ChapterState.Initial;
            if (m_state == ChapterState.DownloadingPages)
                m_state = ChapterState.Initial;
            if (m_state == ChapterState.DownloadingPagesList)
                m_state = ChapterState.Initial;
            if (m_state == ChapterState.Waiting)
                m_state = ChapterState.Initial;
            if (m_state == ChapterState.Zipping)
                m_state = ChapterState.Initial;

            a_title = a_title.Trim();
            a_title = a_title.Replace("\t", " ");
            while (a_title.IndexOf("  ") != -1)
                a_title = a_title.Replace("  ", " ");
            Title = HtmlDecode(a_title);
        }

        /// <summary>
        /// Thread safe.
        /// </summary>
        public IList<Page> Pages
        {
            get
            {
                return m_pages;
            }
        }

        public int PagesDownloaded
        {
            get
            {
                return Pages.Count(p => p.State == PageState.Downloaded);
            }
        }

        public Server Server
        {
            get
            {
                return Serie.Server;
            }
        }

        internal override Crawler Crawler
        {
            get
            {
                return Serie.Crawler;
            }
        }

        public override bool IsDownloading
        {
            get
            {
                return (State == ChapterState.Waiting) ||
                       (State == ChapterState.DownloadingPages) ||
                       (State == ChapterState.DownloadingPagesList) ||
                       (State == ChapterState.Cancelling) ||
                       (State == ChapterState.Zipping);
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Serie, Title);
        }

        public void CancelDownloading()
        {
            if (State == ChapterState.Cancelling)
                return;

            lock (m_state_lock)
            {
                if (IsDownloading)
                    State = ChapterState.Cancelling;
            }
        }

        public override string GetDirectory()
        {
            string manga_root_dir = DownloadManager.Instance.MangaSettings.GetMangaRootDir(true);

            return manga_root_dir +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileCharacters(Serie.Server.Name) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileCharacters(Serie.Title) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileCharacters(Title) +
                   Path.DirectorySeparatorChar;
        }

        internal void DownloadPagesList()
        {
            var pages = Crawler.DownloadPages(this).ToList();

            m_pages.ReplaceInnerCollection(pages);

            State = ChapterState.DownloadingPages;

            foreach (var page in Pages)
            {
                page.State = PageState.Waiting;

                Debug.Assert(page.Index == Pages.IndexOf(page) + 1);
            }

            Catalog.Save(this);
        }

        internal void DownloadPagesAndImages()
        {
            try
            {
                Limiter.BeginChapter(this);

                try
                {
                    DownloadPagesList();

                    var names = Pages.Select(p => p.Name);
                    var sorted_names = Pages.Select(p => p.Name).OrderBy(n => n, new NaturalOrderStringComparer());
                    bool error = false;

                    PageNamingStrategy pns = DownloadManager.Instance.MangaSettings.PageNamingStrategy;
                    if (pns == PageNamingStrategy.IndexToPreserveOrder)
                    {
                        if (!names.SequenceEqual(sorted_names))
                            pns = PageNamingStrategy.AlwaysUseIndex;
                    }
                    else if (pns == PageNamingStrategy.PrefixToPreserverOrder)
                    {
                        if (!names.SequenceEqual(sorted_names))
                            pns = PageNamingStrategy.AlwaysUsePrefix;
                    }

                    for (int i = 0; i < Pages.Count; i++)
                    {
                        Pages[i].LimiterOrder = Catalog.NextID();

                        Debug.Assert(Pages[i].Index == i + 1);
                    }

                    Parallel.ForEach(new SequentialPartitioner<Page>(Pages),

                        new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = Crawler.MaxConnectionsPerServer
                        },
                        (page, state) =>
                        {
                            try
                            {
                                page.DownloadAndSavePageImage(pns);

                                Catalog.Save(this);
                            }
                            catch (OperationCanceledException)
                            {
                                state.Break();
                            }
                            catch (Exception ex2)
                            {
                                Loggers.MangaCrawler.Error(String.Format(
                                    "Exception #1, chapter: {0} state: {1}",
                                    this, State), ex2);

                                error = true;
                            }
                        }
                    );

                    Token.ThrowIfCancellationRequested();
   
                    if (PagesDownloaded != Pages.Count)
                        State = ChapterState.Error;
                    else if (Pages.Any(p => p.State != PageState.Downloaded))
                        State = ChapterState.Error;
                    else if (error)
                        State = ChapterState.Error;

                    Catalog.Save(this);

                    if (DownloadManager.Instance.MangaSettings.UseCBZ)
                        if (State != ChapterState.Error)
                            CreateCBZ();

                    Visited = true;
                }
                finally
                {
                    Limiter.EndChapter(this);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Assert(State == ChapterState.Cancelling);
                Debug.Assert(m_cancellation_token_source.IsCancellationRequested);

                State = ChapterState.Cancelled;
            }
            catch (Exception ex1)
            {
                Loggers.MangaCrawler.Error(String.Format(
                    "Exception #2, chapter: {0} state: {1}", this, State), ex1);

                State = ChapterState.Error;

                try
                {
                    DownloadManager.Instance.DownloadChapters(Serie, true);
                }
                catch (Exception ex2)
                {
                    Loggers.MangaCrawler.Error(String.Format(
                        "Exception #3, chapter: {0} state: {1}", this, State), ex2);
                }
            }
            finally
            {
                lock (m_state_lock)
                {
                    if ((State != ChapterState.Error) && (State != ChapterState.Cancelled))
                    {
                        Debug.Assert(
                            (State == ChapterState.DownloadingPages) || 
                            (State == ChapterState.Zipping));
                        State = ChapterState.Downloaded;
                    }
                }
            }

            Catalog.Save(this);
        }

        private void CreateCBZ()
        {
            Debug.Assert(State == ChapterState.DownloadingPages);

            Loggers.MangaCrawler.InfoFormat(
                "Chapter: {0} state: {1}",
                this, State);

            if (Pages.Count == 0)
            {
                Loggers.MangaCrawler.InfoFormat("Pages.Count = 0 - nothing to zip");
                return;
            }

            State = ChapterState.Zipping;

            var dir = new DirectoryInfo(Pages.First().ImageFilePath).Parent;

            var zip_file = dir.FullName + ".cbz";

            try
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                    zip.AlternateEncoding = Encoding.UTF8;

                    foreach (var page in Pages)
                    {
                        zip.AddFile(page.ImageFilePath, "");

                        Token.ThrowIfCancellationRequested();
                    }

                    zip.Save(zip_file);
                }

                if (DownloadManager.Instance.MangaSettings.DeleteDirWithImagesWhenCBZ)
                {
                    foreach (var page in Pages)
                    {
                        if (!String.IsNullOrWhiteSpace(page.ImageFilePath))
                            if (File.Exists(page.ImageFilePath))
                                File.Delete(page.ImageFilePath);
                    }

                    if (!Directory.EnumerateFiles(GetDirectory()).Any())
                        if (!Directory.EnumerateDirectories(GetDirectory()).Any())
                            Directory.Delete(GetDirectory());
                }
            }
            catch (Exception ex)
            {
                State = ChapterState.Error;

                Loggers.MangaCrawler.Error(String.Format(
                        "Exception, chapter: {0} state: {1}", this, State), ex);
            }
        }

        internal CancellationToken Token
        {
            get
            {
                return m_cancellation_token_source.Token;
            }
        }

        public ChapterState State
        {
            get
            {
                lock (m_state_lock)
                {
                return m_state;
                }
            }
            internal set
            {
                lock (m_state_lock)
                {
                    switch (value)
                    {
                        case ChapterState.Initial:
                        {
                            break;
                        }
                        case ChapterState.Waiting:
                        {
                            Debug.Assert((State == ChapterState.Cancelled) ||
                                         (State == ChapterState.Downloaded) ||
                                         (State == ChapterState.Error) ||
                                         (State == ChapterState.Initial));
                            m_cancellation_token_source = new CancellationTokenSource();
                            break;
                        }
                        case ChapterState.DownloadingPagesList:
                        {
                            Token.ThrowIfCancellationRequested();
                            Debug.Assert((State == ChapterState.Waiting) ||
                                         (State == ChapterState.DownloadingPagesList));
                            break;
                        }
                        case ChapterState.DownloadingPages:
                        {
                            Token.ThrowIfCancellationRequested();
                            Debug.Assert(State == ChapterState.DownloadingPagesList);
                            break;
                        }
                        case ChapterState.Zipping:
                        {
                            Token.ThrowIfCancellationRequested();
                            Debug.Assert(State == ChapterState.DownloadingPages);
                            break;
                        }
                        case ChapterState.Cancelled:
                        {
                            Debug.Assert(State == ChapterState.Cancelling);
                            break;
                        }
                        case ChapterState.Cancelling:
                        {
                            Debug.Assert((State == ChapterState.DownloadingPages) ||
                                         (State == ChapterState.DownloadingPagesList) ||
                                         (State == ChapterState.Waiting) ||
                                         (State == ChapterState.Zipping));
                            m_cancellation_token_source.Cancel();
                            break;
                        }
                        case ChapterState.Error:
                        {
                            Debug.Assert((State == ChapterState.DownloadingPages) ||
                                         (State == ChapterState.DownloadingPagesList) ||
                                         (State == ChapterState.Waiting) ||
                                         (State == ChapterState.Error) ||
                                         (State == ChapterState.Zipping));
                            break;
                        }
                        case ChapterState.Downloaded:
                        {
                            Debug.Assert((State == ChapterState.DownloadingPages) ||
                                         (State == ChapterState.Zipping));
                            break;
                        }
                        default:
                        {
                            throw new InvalidOperationException(String.Format("Unknown state: {0}", value));
                        }
                    }

                    m_state = value;
                }
            }
        }

        public bool CanReadFirstPage()
        {
            if (Pages.Any())
            {
                if (!String.IsNullOrWhiteSpace(Pages.First().ImageFilePath))
                {
                    try
                    {
                        if (new FileInfo(Pages.First().ImageFilePath).Exists)
                            return true;
                    }
                    catch (Exception ex)
                    {
                        Loggers.MangaCrawler.Error(String.Format(
                            "Exception, chapter: {0} state: {1}", this, State), ex);

                        return false;
                    }
                }
            }

            return false;
        }
    }
}
