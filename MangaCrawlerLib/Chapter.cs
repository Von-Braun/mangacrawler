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
        protected ChapterState m_State = ChapterState.Initial;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CancellationTokenSource m_cancellation_token_source = new CancellationTokenSource();

        public virtual int ID { get; protected internal set; }
        public virtual string URL { get; protected internal set; }
        public virtual Serie Serie { get; protected internal set; }
        public virtual string Title { get; protected internal set; }
        public virtual DateTime LastChange { get; protected internal set; }
        protected internal virtual IList<Page> Pages { get; set; }
        public virtual string ChapterDir { get; protected internal set; }
        public virtual bool CBZ { get; protected internal set; }

        internal protected Chapter()
        {
        }

        internal Chapter(Serie a_serie, string a_url, string a_title)
        {
            Pages = new List<Page>();
            ID = IDGenerator.Next();
            Serie = a_serie;
            URL = HttpUtility.HtmlDecode(a_url);
            LastChange = DateTime.Now;

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
                m.Property(c => c.State, mapping => mapping.Access(Accessor.Field));
                m.Property(c => c.ChapterDir, mapping => mapping.NotNullable(true));
                m.Property(c => c.CBZ);
                m.ManyToOne(c => c.Serie, mapping => mapping.NotNullable(true));
                m.List<Page>("Pages", list_mapping => list_mapping.Inverse(true), mapping => mapping.OneToMany());
            });
        }

        public virtual ChapterState State
        {
            get
            {
                return m_State;
            }
            internal protected set
            {
                m_State = value;
                LastChange = DateTime.Now;
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

        protected internal virtual bool DownloadRequired
        {
            get
            {
                return (State == ChapterState.Error) ||
                       (State == ChapterState.Initial) &&
                       (State == ChapterState.Aborted);
            }
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

        protected internal virtual void CreateWork(string a_manga_root_dir, bool a_cbz)
        {
            ChapterDir = GetChapterDirectory(a_manga_root_dir);
            CBZ = a_cbz;
            State = ChapterState.Waiting;
        }

        public virtual bool IsWorking
        {
            get
            {
                return (State == ChapterState.Waiting) ||
                       (State == ChapterState.Downloading) ||
                       (State == ChapterState.Deleting) ||
                       (State == ChapterState.Zipping);
            }
        }

        internal protected virtual void FinishDownload(bool a_error)
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

        protected internal virtual string GetChapterDirectory(string a_images_base_dir)
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

        protected internal virtual void DownloadPages()
        {
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

                State = ChapterState.Downloading;
                LastChange = DateTime.Now;

                Pages = Serie.Server.Crawler.DownloadPages(this).ToList();
                LastChange = DateTime.Now;

                Parallel.ForEach(Pages,

                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = Serie.Server.Crawler.MaxConnectionsPerServer,
                        TaskScheduler = Serie.Server.Scheduler[Priority.Pages],
                    },
                    (page, state) =>
                    {
                        try
                        {
                            page.DownloadAndSavePageImage();
                            LastChange = DateTime.Now;
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
    }
}
