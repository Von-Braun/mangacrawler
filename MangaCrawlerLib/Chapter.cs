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

        private CancellationTokenSource m_cancellation_token_source;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ChapterState m_state;

        private PagesCachedList m_pages;

        public Serie Serie { get; private set; }
        public string Title { get; private set; }

        internal Chapter(Serie a_serie, string a_url, string a_title)
            : this(a_serie, a_url, a_title, Catalog.NextID(), ChapterState.Initial)
        {
        }

        internal Chapter(Serie a_serie, string a_url, string a_title, ulong a_id, ChapterState a_state)
            : base(a_id)
        {
            m_pages = new PagesCachedList(this);
            Serie = a_serie;
            URL = HttpUtility.HtmlDecode(a_url);
            m_state = a_state;

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

        public bool IsWorking
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

            if (IsWorking)
            {
                Loggers.MangaCrawler.InfoFormat("Deleting, chapter: {0}, state: {1}", this, State);
                CancellationTokenSource.Cancel();
                State = ChapterState.Deleting;
            }
        }

        public string GetChapterDirectory()
        {
            string manga_root_dir = DownloadManager.GetMangaRootDir();

            if (manga_root_dir.Last() == Path.DirectorySeparatorChar)
                manga_root_dir = manga_root_dir.RemoveFromRight(1);

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

            Func<Page, string> key_selector =  p => p.Name + p.URL;
            m_pages.ReplaceInnerCollection(
                pages,
                m_pages.ToDictionary(key_selector),
                true,
                key_selector);

            State = ChapterState.DownloadingPages;

            foreach (var page in Pages)
                page.State = PageState.Waiting;

            Catalog.Save(this);
        }

        internal void DownloadPages()
        {
            bool error = false;

            try
            {
                Limiter.BeginChapter(this);

                DownloadPagesList();

                PageNamingStrategy pns = DownloadManager.PageNamingStrategy();
                if (pns == PageNamingStrategy.PrefixWithIndexWhenNotOrdered)
                    if (!Pages.Select(p => p.Name).SequenceEqual(Pages.Select(p => p.Name).OrderBy(n => n)))
                        pns = PageNamingStrategy.PrefixWithIndex;

                for (int i = 0; i < Pages.Count; i++)
                    Debug.Assert(Pages[i].Index == i + 1);

                Parallel.ForEach(Pages,

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
                        catch (OperationCanceledException ex1)
                        {
                            Loggers.Cancellation.InfoFormat(
                                "OperationCanceledException #2, chapter: {0} state: {1}, {2}",
                                this, State, ex1);

                            state.Break();
                        }
                        catch (Exception ex2)
                        {
                            Loggers.MangaCrawler.InfoFormat(
                                "Exception #1, chapter: {0} state: {1}, {2}",
                                this, State, ex2);

                            state.Break();
                            error = true;
                        }
                    }
                );

                Catalog.Save(this);

                if (DownloadManager.UseCBZ())
                    CreateCBZ();
            }
            catch (OperationCanceledException ex1)
            {
                Loggers.Cancellation.InfoFormat(
                    "OperationCanceledException #2, chapter: {0} state: {1}, {2}",
                    this, State, ex1);
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
                Loggers.MangaCrawler.InfoFormat("Chapter: {0}, state: {1}, error: {2}",
                    this, State, error);

                if (error)
                    State = ChapterState.Error;
                else if (CancellationTokenSource.IsCancellationRequested)
                    State = ChapterState.Aborted;
                else if (PagesDownloaded != Pages.Count)
                    State = ChapterState.Error;
                else if (Pages.Any(p => p.State != PageState.Downloaded))
                    State = ChapterState.Error;
                else
                    State = ChapterState.Downloaded;

                Limiter.EndChapter(this);
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

            int counter = 1;
            while (new FileInfo(zip_file).Exists)
            {
                zip_file = String.Format("{0} ({1}).cbz", dir.FullName, counter);
                counter++;
            }

            try
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.UseUnicodeAsNecessary = true;

                    foreach (var page in Pages)
                    {
                        zip.AddFile(page.ImageFilePath, "");

                        if (Token.IsCancellationRequested)
                        {
                            Loggers.Cancellation.InfoFormat(
                                "cancellation requested, chapter: {0} state: {1}",
                                this, State);

                            Token.ThrowIfCancellationRequested();
                        }
                    }

                    zip.Save(zip_file);
                }
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Fatal("Exception #1", ex);
            }

            try
            {
                foreach (var page in Pages)
                    new FileInfo(page.ImageFilePath).Delete();

                if ((dir.GetFiles().Count() == 0) && (dir.GetDirectories().Count() == 0))
                    dir.Delete();
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Fatal("Exception #2", ex);
            }
        }

        private CancellationTokenSource CancellationTokenSource
        {
            get
            {
                if (m_cancellation_token_source == null)
                    m_cancellation_token_source = new CancellationTokenSource();
                return m_cancellation_token_source;
            }
        }

        internal CancellationToken Token
        {
            get
            {
                return CancellationTokenSource.Token;
            }
        }

        public ChapterState State
        {
            get
            {
                return m_state;
            }
            internal set
            {
                switch (value)
                {
                    case ChapterState.Initial:
                    {
                        break;
                    }
                    case ChapterState.Waiting:
                    {
                        Debug.Assert((State == ChapterState.Aborted) ||
                                     (State == ChapterState.Downloaded) ||
                                     (State == ChapterState.Error) ||
                                     (State == ChapterState.Initial));
                        break;
                    }
                    case ChapterState.DownloadingPagesList:
                    {
                        Debug.Assert(State == ChapterState.Waiting);
                        break;
                    }
                    case ChapterState.DownloadingPages:
                    {
                        Debug.Assert(State == ChapterState.DownloadingPagesList);
                        break;
                    }
                    case ChapterState.Zipping:
                    {
                        Debug.Assert(State == ChapterState.DownloadingPages);
                        break;
                    }
                    case ChapterState.Aborted:
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
