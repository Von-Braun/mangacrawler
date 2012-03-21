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

                    if (m_list.Count != 0)
                        m_loaded_from_xml = true;
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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool m_bookmark_ignored;

        public Serie Serie { get; private set; }
        public string Title { get; private set; }

        internal Chapter(Serie a_serie, string a_url, string a_title)
            : this(a_serie, a_url, a_title, Catalog.NextID(), ChapterState.Initial, 0, false)
        {
        }

        internal Chapter(Serie a_serie, string a_url, string a_title, ulong a_id, ChapterState a_state,
            ulong a_limiter_order, bool a_bookmark_ignore)
            : base(a_id)
        {
            m_bookmark_ignored = a_bookmark_ignore;
            m_pages = new PagesCachedList(this);
            LimiterOrder = a_limiter_order;
            Serie = a_serie;
            URL = HttpUtility.HtmlDecode(a_url);
            m_state = a_state;

            if (m_state == ChapterState.Deleting)
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
            Title = HttpUtility.HtmlDecode(a_title);
        }

        public IList<Page> Pages
        {
            get
            {
                return m_pages;
            }
        }

        public bool BookmarkIgnored
        {
            get
            {
                return m_bookmark_ignored;
            }
            set
            {
                m_bookmark_ignored = value;
                Catalog.Save(this);
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
                       (State == ChapterState.Deleting) ||
                       (State == ChapterState.Zipping);
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Serie, Title);
        }

        public void DeleteWork()
        {
            if (State == ChapterState.Deleting)
                return;

            lock (m_state_lock)
            {
                if (IsDownloading)
                    State = ChapterState.Deleting;
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
                page.State = PageState.Waiting;

            Catalog.Save(this);
        }

        internal void DownloadPages()
        {
            bool error = false;
            m_cancellation_token_source = new CancellationTokenSource();

            try
            {
                Limiter.BeginChapter(this);

                try
                {
                    DownloadPagesList();

                    var names = Pages.Select(p => p.Name);
                    var sorted_names = Pages.Select(p => p.Name).OrderBy(n => n, new NaturalOrderStringComparer());

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
                            MaxDegreeOfParallelism = Crawler.MaxConnectionsPerServer,
                            TaskScheduler = Limiter.Scheduler
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
                                Loggers.MangaCrawler.InfoFormat(
                                    "Exception #1, chapter: {0} state: {1}, {2}",
                                    this, State, ex2);

                                error = true;
                            }
                        }
                    );

                    Catalog.Save(this);

                    if (DownloadManager.Instance.MangaSettings.UseCBZ)
                        CreateCBZ();

                    m_bookmark_ignored = true;
                }
                finally
                {
                    Limiter.EndChapter(this);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.InfoFormat(
                    "Exception #2, chapter: {0} state: {1}, {2}",
                    this, State, ex);
                error = true;
            }
            finally
            {
                lock (m_state_lock)
                {
                    Loggers.MangaCrawler.DebugFormat("Chapter: {0}, state: {1}, error: {2}",
                        this, State, error);

                    if (error)
                        State = ChapterState.Error;
                    else if (m_cancellation_token_source.IsCancellationRequested)
                        State = ChapterState.Deleted;
                    else if (PagesDownloaded != Pages.Count)
                        State = ChapterState.Error;
                    else if (Pages.Any(p => p.State != PageState.Downloaded))
                        State = ChapterState.Error;
                    else
                        State = ChapterState.Downloaded;
                }
            }

            Catalog.Save(this);
        }

        private void CreateCBZ()
        {
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
                    zip.UseUnicodeAsNecessary = true;

                    foreach (var page in Pages)
                    {
                        zip.AddFile(page.ImageFilePath, "");

                        Token.ThrowIfCancellationRequested();
                    }

                    zip.Save(zip_file);
                }
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Fatal("Exception", ex);
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
                            Debug.Assert((State == ChapterState.Deleted) ||
                                         (State == ChapterState.Downloaded) ||
                                         (State == ChapterState.Error) ||
                                         (State == ChapterState.Initial));
                            break;
                        }
                        case ChapterState.DownloadingPagesList:
                        {
                            Token.ThrowIfCancellationRequested();
                            Debug.Assert(State == ChapterState.Waiting);
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
                        case ChapterState.Deleted:
                        {
                            Debug.Assert(State == ChapterState.Deleting);
                            break;
                        }
                        case ChapterState.Deleting:
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
    }
}
