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
                        //mapping.Insert(false);
                        //mapping.Update(false);
                    }
                );

                m.List<Page>("Pages",
                    list_mapping => 
                    { 
                        //list_mapping.Inverse(false);
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

        private bool IsWorking
        {
            get
            {
                return (State == ChapterState.Waiting) ||
                       (State == ChapterState.Downloading) ||
                       (State == ChapterState.Deleting) ||
                       (State == ChapterState.Zipping);
            }
        }

        protected internal virtual void ResetState()
        {
            State = ChapterState.Initial;
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
                var s = State;

                Loggers.MangaCrawler.InfoFormat("Chapter: {0}, state: {1}", this, s);

                if ((s == ChapterState.Downloading) ||
                    (s == ChapterState.Waiting) ||
                    (s == ChapterState.Zipping))
                {
                    Loggers.MangaCrawler.Info("Cancelling chapter");
                    m_cancellation_token_source.Cancel();

                    State = ChapterState.Deleting;
                }
            });
        }

        private void FinishDownload(bool a_error)
        {
            NH.TransactionLockUpdate(this, () =>
            {
                var s = State;

                Loggers.MangaCrawler.InfoFormat("Chapter: {0}, state: {1}, error: {2}",
                    this, s, a_error);

                if ((s == ChapterState.Waiting) || (s == ChapterState.Downloading))
                {
                    if (a_error)
                        State = ChapterState.Error;
                    else
                    {
                        if (PagesDownloaded == PagesCount)
                            State = ChapterState.Downloaded;
                        else
                            State = ChapterState.Error;
                    }
                }

                if (s != ChapterState.Downloaded)
                {
                    if (m_cancellation_token_source.IsCancellationRequested)
                        State = ChapterState.Aborted;
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

        protected internal virtual void DownloadPagesList()
        {
            var pages = Crawler.DownloadPages(this).ToList();

            // TODO: dodac nowe, usunac nieistniejace.
            // TODO: nie da sie tego dla series, chapters, pages zrobic na wspolnej procedurze
            NH.TransactionLockUpdate(this, () =>
            {
                int index = 0;
                foreach (var page in pages)
                {
                    if (Pages.Count <= index)
                        Pages.Insert(index, page);
                    else
                    {
                        var p = Pages[index];

                        if ((p.Index != page.Index) || (p.URL != page.URL) || (p.Name != page.Name))
                            Pages.Insert(index, page);
                    }
                    index++;
                }

                PagesCount = index;
                PagesDownloaded = 0;
                ChapterDir = null;
                CBZ = false;
            });
        }

        // TODO: testy: 
        // sciaganie losowe wielu rzeczy i ch anulowanie, brak wyjatkow, deadlockow, spojnosc danych
        // dodac procedure ktora testuje spojnosc danych - ich stan i powiazania
        // pobranie serii, chapterow, pagey - dodanie nowych, usuniecie istniejacych, jakies zmiany, czy ponowne 
        //   pobranie sobie z tym radzi
        // page - zmiana hashu juz pobranego, usunicie go z dysku
        // page - symulacja webexcption podczas pobierania
        // page - symulacja pobrania 0 lenth
        // page - symulacja pobrania smieci - np 404 not found
        // testy na series, chapter na zwrocenie jakis wyjatkow z czesci web
        // page - wywalenie wyjatku, niemozliwy zapis pliku (na plik dac lock)
        // podotykac wszystkiego, sprawdzic zajetosc pamieci
        // 

        protected internal virtual void DownloadPages(string a_manga_root_dir, bool a_cbz)
        {
            NH.TransactionLockUpdate(this, () => 
            {
                ChapterDir = GetChapterDirectory(a_manga_root_dir);
                CBZ = a_cbz;
            });

            try
            {
                ConnectionsLimiter.BeginDownloadPages(this);
            }
            catch (OperationCanceledException)
            {
                Loggers.Cancellation.InfoFormat(
                    "OperationCanceledException #1, chapter: {0} state: {1}",
                    this, State);

                FinishDownload(true);
                return;
            }

            try
            {
                DownloadPagesList();

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
                            if (page.Downloaded)
                            {
                                if (page.ImageExists())
                                {
                                    PagesDownloaded++;
                                    return;
                                }
                            }
 
                            page.DownloadAndSavePageImage();

                            NH.TransactionLockUpdate(this, () =>
                            {
                                if (page.Downloaded)
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
                            throw;
                        }
                    });

                if (CBZ)
                    CreateCBZ();

                FinishDownload(a_error: false);
            }
            catch (OperationCanceledException ex1)
            {
                Loggers.Cancellation.InfoFormat(
                    "OperationCanceledException #2, chapter: {0} state: {1}, {2}",
                    this, State, ex1);

                FinishDownload(a_error: false);
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.InfoFormat(
                    "Exception #2, chapter: {0} state: {1}, {2}",
                    this, State, ex);

                FinishDownload(a_error: true);
            }
            finally
            {
                ConnectionsLimiter.EndDownloadPages(this);
            }
        }

        private void CreateCBZ()
        {
            Loggers.MangaCrawler.InfoFormat(
                "Chapter: {0} state: {1}",
                this, State);

            NH.TransactionLockUpdate(this, () => State = ChapterState.Zipping);

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

        protected internal virtual bool BeginWaiting()
        {
            if (IsWorking)
                return false;
            State = ChapterState.Waiting;
            return true;
        }

        protected internal virtual void DownloadingStarted()
        {
            if (State == ChapterState.Waiting)
            {
                State = ChapterState.Downloading;
                PagesDownloaded = 0;
                PagesCount = 0;
                ChapterDir = null;
                CBZ = false;
            }
        }
    }
}
