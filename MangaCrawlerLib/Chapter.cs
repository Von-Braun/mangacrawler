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
        public virtual DateTime LastChange { get; protected set; }

        protected Chapter()
        {
        }

        internal Chapter(Serie a_serie, string a_url, string a_title)
        {
            Serie = a_serie;
            URL = HttpUtility.HtmlDecode(a_url);
            ID = IDGenerator.Next();
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
                m.Id(c => c.ID, mapper => mapper.Generator(Generators.Native));
                m.Property(c => c.URL, mapping => mapping.NotNullable(true) );
                m.Property(c => c.Title, mapping => mapping.NotNullable(true));
                m.Version(c => c.LastChange, mapper => { });
                m.Property(c => c.State, mapping => { });
                m.Property(c => c.ChapterDir, mapping => mapping.NotNullable(true));
                m.Property(c => c.CBZ, mapping => { });
                m.ManyToOne(c => c.Serie, mapping => mapping.NotNullable(true));
                m.List<Page>("Pages", list_mapping => list_mapping.Inverse(true), mapping => mapping.OneToMany());
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
            var s = State;

            Loggers.MangaCrawler.InfoFormat("Work: {0}, state: {1}", this, s);

            if ((s == ChapterState.Downloading) ||
                (s == ChapterState.Waiting) ||
                (s == ChapterState.Zipping))
            {
                Loggers.MangaCrawler.Info("Cancelling work");
                m_cancellation_token_source.Cancel();

                State = ChapterState.Deleting;
            }
        }

        private void FinishDownload(bool a_error)
        {
            var s = State;

            Loggers.MangaCrawler.InfoFormat("Work: {0}, state: {1}, error: {2}",
                this, s, a_error);

            if ((s == ChapterState.Waiting) || (s == ChapterState.Downloading))
            {
                if (a_error)
                    State = ChapterState.Error;
                else
                {
                    if (DownloadedPages == Pages.Count())
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
            State = ChapterState.Waiting;

            try
            {
                ConnectionsLimiter.BeginDownloadPages(this);
            }
            catch (OperationCanceledException)
            {
                Loggers.Cancellation.InfoFormat(
                    "#1 operation cancelled, work: {0} state: {1}",
                    this, State);

                FinishDownload(true);
                return;
            }

            try
            {
                if (Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "#1 cancellation requested, work: {0} state: {1}",
                        this, State);

                    Token.ThrowIfCancellationRequested();
                }

                Pages = Crawler.DownloadPages(this).ToList();

                if (Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "#2 cancellation requested, work: {0} state: {1}",
                        this, State);

                    Token.ThrowIfCancellationRequested();
                }
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.InfoFormat(
                    "Exception, work: {0} state: {1}, {2}",
                    this, State, ex);

                FinishDownload(a_error: true);
            }
            finally
            {
                ConnectionsLimiter.EndDownloadPages(this);
            }
        }

        protected internal virtual void DownloadPages(string a_manga_root_dir, bool a_cbz)
        {
            ChapterDir = GetChapterDirectory(a_manga_root_dir);
            CBZ = a_cbz;

            try
            {
                ConnectionsLimiter.BeginDownloadPages(this);
            }
            catch (OperationCanceledException)
            {
                Loggers.Cancellation.InfoFormat(
                    "#1 operation cancelled, work: {0} state: {1}",
                    this, State);

                FinishDownload(true);
                return;
            }

            try
            {
                if (Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "#1 cancellation requested, work: {0} state: {1}",
                        this, State);

                    Token.ThrowIfCancellationRequested();
                }

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
                            page.DownloadAndSavePageImage();
                        }
                        catch (OperationCanceledException ex1)
                        {
                            Loggers.Cancellation.InfoFormat(
                                "OperationCanceledException, work: {0} state: {1}, {2}",
                                this, State, ex1);

                            state.Break();
                        }
                        catch (Exception ex2)
                        {
                            Loggers.MangaCrawler.InfoFormat(
                                "1 Exception, work: {0} state: {1}, {2}",
                                this, State, ex2);

                            state.Break();
                            throw;
                        }
                    });

                if (Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "#3 cancellation requested, work: {0} state: {1}",
                        this, State);

                    Token.ThrowIfCancellationRequested();
                }

                if (CBZ)
                    CreateCBZ();

                FinishDownload(a_error: false);
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.InfoFormat(
                    "#2 Exception, work: {0} state: {1}, {2}",
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
                "Work: {0} state: {1}",
                this, State);

            State = ChapterState.Zipping;

            var dir = new DirectoryInfo(Pages.First().ImageFilePath).Parent;

            var zip_file = dir.FullName + ".cbz";

            int counter = 1;
            while (new FileInfo(zip_file).Exists)
            {
                zip_file = String.Format("{0} ({1}).cbz", dir.FullName, counter);
                counter++;
            }

            using (ZipFile zip = new ZipFile())
            {
                zip.UseUnicodeAsNecessary = true;

                foreach (var page in Pages)
                {
                    zip.AddFile(page.ImageFilePath, "");

                    if (Token.IsCancellationRequested)
                    {
                        Loggers.Cancellation.InfoFormat(
                            "cancellation requested, work: {0} state: {1}",
                            this, State);

                        Token.ThrowIfCancellationRequested();
                    }
                }

                zip.Save(zip_file);
            }

            try
            {
                foreach (var page in Pages)
                    new FileInfo(page.ImageFilePath).Delete();

                if ((dir.GetFiles().Count() == 0) && (dir.GetDirectories().Count() == 0))
                    dir.Delete();
            }
            catch
            {
            }
        }

        protected internal virtual CancellationToken Token
        {
            get
            {
                return m_cancellation_token_source.Token;
            }
        }

        public virtual int DownloadedPages
        {
            get
            {
                return Pages.Count(p => p.Downloaded);
            }
        }

        protected internal virtual bool BeginDownloading()
        {
            if (IsWorking)
                return false;
            State = ChapterState.Waiting;
            return false;
        }
    }
}
