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
using System.Threading.Tasks;
using Ionic.Zip;
using NHibernate.Mapping.ByCode;

namespace MangaCrawlerLib
{
    internal class ChapterWork //: IClassMapping
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CancellationTokenSource m_cancellation_token_source = new CancellationTokenSource();

        public virtual Chapter Chapter { get; private set; }
        public virtual string ChapterDir { get; private set; }
        public virtual bool CBZ { get; private set; }
        public int ID { get; private set; }

        internal ChapterWork(Chapter a_chapter, string a_manga_root_dir, bool a_cbz) 
        {
            ID = IDGenerator.Next();
            Chapter = a_chapter;
            ChapterDir = GetChapterDirectory(a_manga_root_dir);
            CBZ = a_cbz;
        }

        public void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<ChapterWork>(m =>
            {
                m.Lazy(true);
                m.Id(c => c.ID);
                m.Property(c => c.Chapter);
                m.Property(c => c.ChapterDir);
                m.Property(c => c.CBZ);
            });
        }

        public bool IsWorking
        {
            get
            {
                return (Chapter.State == ChapterState.Waiting) ||
                       (Chapter.State == ChapterState.Downloading) ||
                       (Chapter.State == ChapterState.Deleting) ||
                       (Chapter.State == ChapterState.Zipping);
            }
        }

        internal void DownloadPages()
        {
            try
            {
                ConnectionsLimiter.BeginDownloadPages(Chapter);
            }
            catch (OperationCanceledException)
            {
                Loggers.Cancellation.InfoFormat(
                    "#1 operation cancelled, work: {0} state: {1}",
                    this, Chapter.State);

                FinishDownload(true);
                return;
            }

            try
            {
                if (Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "#1 cancellation requested, work: {0} state: {1}",
                        this, Chapter.State);

                    Token.ThrowIfCancellationRequested();
                }

                Chapter.State = ChapterState.Downloading;
                Chapter.LastChange = DateTime.Now;

                Chapter.AddPages(Chapter.Serie.Server.Crawler.DownloadPages(Chapter));
                Chapter.LastChange = DateTime.Now;

                Parallel.ForEach(Chapter.Pages, 

                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = Chapter.Serie.Server.Crawler.MaxConnectionsPerServer,
                        TaskScheduler = Chapter.Serie.Server.Scheduler[Priority.Pages],
                    },
                    (page, state) =>
                    {
                        try
                        {
                            page.DownloadAndSavePageImage();
                            Chapter.LastChange = DateTime.Now;
                        }
                        catch (OperationCanceledException ex1)
                        {
                            Loggers.Cancellation.InfoFormat(
                                "OperationCanceledException, work: {0} state: {1}, {2}",
                                this, Chapter.State, ex1);

                            state.Break();
                        }
                        catch (Exception ex2)
                        {
                            Loggers.MangaCrawler.InfoFormat(
                                "1 Exception, work: {0} state: {1}, {2}",
                                this, Chapter.State, ex2);

                            state.Break();
                            throw;
                        }
                    });

                if (Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "#3 cancellation requested, work: {0} state: {1}",
                        this, Chapter.State);

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
                    this, Chapter.State, ex);

                FinishDownload(a_error: true);
            }
            finally
            {
                ConnectionsLimiter.EndDownloadPages(Chapter);
            }
        }

        private void CreateCBZ()
        {
            Loggers.MangaCrawler.InfoFormat(
                "Work: {0} state: {1}",
                this, Chapter.State);

            Chapter.State = ChapterState.Zipping;

            var dir = new DirectoryInfo(Chapter.Pages.First().ImageFilePath).Parent;

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

                foreach (var page in Chapter.Pages)
                {
                    zip.AddFile(page.ImageFilePath, "");

                    if (Token.IsCancellationRequested)
                    {
                        Loggers.Cancellation.InfoFormat(
                            "cancellation requested, work: {0} state: {1}",
                            this, Chapter.State);

                        Token.ThrowIfCancellationRequested();
                    }
                }

                zip.Save(zip_file);
            }

            try
            {
                foreach (var page in Chapter.Pages)
                    new FileInfo(page.ImageFilePath).Delete();

                if ((dir.GetFiles().Count() == 0) && (dir.GetDirectories().Count() == 0))
                    dir.Delete();
            }
            catch
            {
            }

        }

        public override string ToString()
        {
            return String.Format("{0} - {1} - {2}", Chapter.Serie.Server.Name, Chapter.Serie.Title, Chapter.Title);
        }

        internal CancellationToken Token
        {
            get
            {
                return m_cancellation_token_source.Token;
            }
        }

        public void DeleteWork()
        {
            var s = Chapter.State;

            Loggers.MangaCrawler.InfoFormat("Work: {0}, state: {1}", this, s);

            if ((s == ChapterState.Downloading) ||
                (s == ChapterState.Waiting) ||
                (s == ChapterState.Zipping))
            {
                Loggers.MangaCrawler.Info("Cancelling work");
                m_cancellation_token_source.Cancel();

                Chapter.State = ChapterState.Deleting;
            }
        }

        internal void FinishDownload(bool a_error)
        {
            var s = Chapter.State;

            Loggers.MangaCrawler.InfoFormat("Work: {0}, state: {1}, error: {2}",
                this, s, a_error);

            if ((s == ChapterState.Waiting) || (s == ChapterState.Downloading))
            {
                if (a_error)
                    Chapter.State = ChapterState.Error;
                else
                {
                    if (Chapter.DownloadedPages == Chapter.Pages.Count())
                        Chapter.State = ChapterState.Downloaded;
                    else
                        Chapter.State = ChapterState.Error;
                }
            }

            if (s != ChapterState.Downloaded)
            {
                if (m_cancellation_token_source.IsCancellationRequested)
                    Chapter.State = ChapterState.Aborted;
            }
        }

        public string GetChapterDirectory(string a_images_base_dir)
        {
            if (a_images_base_dir.Last() == Path.DirectorySeparatorChar)
                a_images_base_dir = a_images_base_dir.RemoveFromRight(1);

            return a_images_base_dir +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(Chapter.Serie.Server.Name) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(Chapter.Serie.Title) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(Chapter.Title) +
                   Path.DirectorySeparatorChar;
        }
    }
}
