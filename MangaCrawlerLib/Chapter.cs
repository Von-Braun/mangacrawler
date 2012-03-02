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
using NHibernate.Mapping.ByCode;
using System.Collections.ObjectModel;
using Ionic.Zip;
using System.Threading.Tasks;
using NHibernate.Type;

namespace MangaCrawlerLib
{
    public class Chapter : IClassMapping
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CancellationTokenSource m_cancellation_token_source = new CancellationTokenSource();

        public virtual int ID { get; protected set; }
        public virtual ChapterState State { get; protected set; }
        public virtual bool CBZ { get; protected set; }
        public virtual string URL { get; protected set; }
        public virtual string ChapterDir { get; protected set; }
        public virtual Serie Serie { get; protected set; }
        protected virtual IList<Page> Pages { get; set; }
        public virtual string Title { get; protected set; }
        protected virtual int Version { get; set; }
        public virtual int PagesCount { get; protected set; }
        public virtual int PagesDownloaded { get; protected set; }

        protected Chapter()
        {
        }

        internal Chapter(Serie a_serie, string a_url, string a_title)
        {
            Serie = a_serie;
            URL = HttpUtility.HtmlDecode(a_url);
            Pages = new List<Page>();

            a_title = a_title.Trim();
            a_title = a_title.Replace("\t", " ");
            while (a_title.IndexOf("  ") != -1)
                a_title = a_title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(a_title);
        }

        private void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<Chapter>(m =>
            {
                m.Id(c => c.ID, mapper => { mapper.Generator(Generators.Native); mapper.Type(new Int32Type()); });
                m.Property(c => c.URL, mapping => mapping.NotNullable(true) );
                m.Property(c => c.Title, mapping => mapping.NotNullable(true));
                m.Version("Version", mapper => { });
                m.Property(c => c.State, mapping => { });
                m.Property(c => c.ChapterDir, mapping => mapping.NotNullable(false));
                m.Property(c => c.CBZ, mapping => { });
                m.Property(c => c.PagesCount, mapping => { });
                m.Property(c => c.PagesDownloaded, mapping => { });

                m.ManyToOne(
                    c => c.Serie,
                    mapping =>
                    {
                        mapping.Fetch(FetchKind.Join);
                        mapping.NotNullable(false);
                    }
                );

                m.List<Page>("Pages",
                    list_mapping => 
                    { 
                        list_mapping.Cascade(Cascade.All); 
                    },
                    mapping => mapping.OneToMany()
                );
            });
        }

        public virtual Server Server
        {
            get
            {
                return Serie.Server;
            }
        }

        protected internal virtual CustomTaskScheduler Scheduler
        {
            get
            {
                return Serie.Scheduler;
            }
        }

        protected internal virtual Crawler Crawler
        {
            get
            {
                return Serie.Crawler;
            }
        }

        public virtual bool IsWorking
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

        public virtual IEnumerable<Page> GetPages()
        {
            return Pages;
        }

        public virtual void DeleteWork()
        {
            NH.TransactionLockUpdate(this, () =>
            {
                if (State == ChapterState.Deleting)
                    return;

                if (IsWorking)
                {
                    Loggers.MangaCrawler.InfoFormat("Deleting, chapter: {0}, state: {1}", this, State);
                    m_cancellation_token_source.Cancel();

                    SetState(ChapterState.Deleting);
                }
            });
        }

        private string GetChapterDirectory(string a_images_base_dir)
        {
            if (a_images_base_dir.Last() == Path.DirectorySeparatorChar)
                a_images_base_dir = a_images_base_dir.RemoveFromRight(1);

            return a_images_base_dir +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(Serie.Server.Name) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(Serie.Title) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(Title) +
                   Path.DirectorySeparatorChar;
        }

        protected internal virtual void DownloadPagesList(string a_manga_root_dir, bool a_cbz)
        {
            var pages = Crawler.DownloadPages(this);

            NH.TransactionLockUpdate(this, () =>
            {
                bool added;
                IList<Page> removed;
                DownloadManager.Sync(pages, Pages, page => (page.Name + page.URL), true, out added, out removed);

                PagesCount = Pages.Count;
                PagesDownloaded = 0;
                ChapterDir = GetChapterDirectory(a_manga_root_dir);
                CBZ = a_cbz;
                SetState(ChapterState.DownloadingPages);

                foreach (var page in Pages)
                {
                    if (added || removed.Any())
                        page.SetState(PageState.WaitingForDownloading);
                    else
                        page.SetState(PageState.WaitingForVerifing);
                }
            });
        }

        protected internal virtual void DownloadPages(string a_manga_root_dir, bool a_cbz)
        {
            bool error = false;

            try
            {
                ConnectionsLimiter.BeginDownloadPages(this);

                DownloadPagesList(a_manga_root_dir, a_cbz);

                Parallel.ForEach(Pages,

                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = Crawler.MaxConnectionsPerServer,
                        TaskScheduler = Scheduler[Priority.Pages],
                    },
                    (page, state) =>
                    {
                        try
                        {
                            if (!page.DownloadRequired())
                            {
                                PagesDownloaded++;
                                return;
                            }
 
                            page.DownloadAndSavePageImage();

                            NH.TransactionLockUpdate(this, () =>
                            {
                                if (page.State == PageState.Downloaded)
                                    PagesDownloaded++;
                            });
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
                    });

                if (CBZ)
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
                NH.TransactionLockUpdate(this, () =>
                {
                    Loggers.MangaCrawler.InfoFormat("Chapter: {0}, state: {1}, error: {2}",
                        this, State, error);

                    if (error)
                    {
                        SetState(ChapterState.Error);
                        return;
                    }

                    if (m_cancellation_token_source.IsCancellationRequested)
                        SetState(ChapterState.Aborted);

                    if (PagesDownloaded != Pages.Count)
                    {
                        SetState(ChapterState.Error);
                        return;
                    }

                    if (Pages.Any(p => (p.State != PageState.Downloaded) && (p.State != PageState.Veryfied)))
                    {
                        SetState(ChapterState.Error);
                        return;
                    }

                    SetState(ChapterState.Downloaded);
                });

                ConnectionsLimiter.EndDownloadPages(this);
            }
        }

        private void CreateCBZ()
        {
            Loggers.MangaCrawler.InfoFormat(
                "Chapter: {0} state: {1}",
                this, State);

            if (PagesCount == 0)
            {
                Loggers.MangaCrawler.InfoFormat("PagesCount = 0 - nothing to zip");
                return;
            }

            NH.TransactionLockUpdate(this, () => SetState(ChapterState.Zipping));

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

        protected internal virtual CancellationToken Token
        {
            get
            {
                return m_cancellation_token_source.Token;
            }
        }

        protected internal virtual void SetState(ChapterState a_state)
        {
            switch (a_state)
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
                    PagesDownloaded = 0;
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
                    throw new InvalidOperationException(String.Format("Unknown state: {0}", a_state));
                }
            }

            State = a_state;
        }
    }
}
