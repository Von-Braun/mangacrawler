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

namespace MangaCrawlerLib
{
    public class Work
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CancellationTokenSource m_cancellation_token_source = new CancellationTokenSource();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private WorkState m_state = WorkState.Waiting;

        public List<Page> Pages { get; private set; }
        public Chapter Chapter { get; private set; }
        public string ChapterDir { get; private set; }
        public bool CBZ { get; private set; }

        internal Work()
        {
            Pages = new List<Page>();
        }

        internal Work(Chapter a_chapter, string a_manga_root_dir, bool a_cbz) : this()
        {
            Chapter = a_chapter;
            ChapterDir = GetChapterDirectory(a_manga_root_dir);
            CBZ = a_cbz;
        }

        internal string URL
        {
            get
            {
                return Chapter.URL;
            }
        }

        public WorkState State
        {
            get
            {

                return m_state;
            }
            set
            {
                Loggers.MangaCrawler.InfoFormat("{0} -> {1}", State, value);

                m_state = value;
            }
        }

        public bool IsWorking
        {
            get
            {
                return (m_state == WorkState.Waiting) ||
                       (m_state == WorkState.Downloading) ||
                       (m_state == WorkState.Deleting) ||
                       (m_state == WorkState.Zipping);
            }
        }

        internal void DownloadPages()
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

                State = WorkState.Downloading;
                Pages.AddRange(Chapter.Serie.Server.Crawler.DownloadPages(this));

                Parallel.ForEach(Pages, 

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

            State = WorkState.Zipping;

            var dir = new DirectoryInfo(Pages.First().GetImageFilePath()).Parent;

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
                    zip.AddFile(page.GetImageFilePath(), "");

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
                    new FileInfo(page.GetImageFilePath()).Delete();

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

        public int DownloadedPages
        {
            get
            {
                return Pages.Count(p => p.Downloaded);
            }
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
            var s = State;

            Loggers.MangaCrawler.InfoFormat("Work: {0}, state: {1}", this, s);

            if ((s == WorkState.Downloading) ||
                (s == WorkState.Waiting) ||
                (s == WorkState.Zipping))
            {
                Loggers.MangaCrawler.Info("Cancelling work");
                m_cancellation_token_source.Cancel();

                State = WorkState.Deleting;
            }
        }

        internal void FinishDownload(bool a_error)
        {
            var s = State;

            Loggers.MangaCrawler.InfoFormat("Work: {0}, state: {1}, error: {2}",
                this, s, a_error);

            if ((s == WorkState.Waiting) || (s == WorkState.Downloading))
            {
                if (a_error)
                    State = WorkState.Error;
                else
                {
                    if (DownloadedPages == Pages.Count())
                        State = WorkState.Downloaded;
                    else
                        State = WorkState.Error;
                }
            }

            if (s != WorkState.Downloaded)
            {
                if (m_cancellation_token_source.IsCancellationRequested)
                    State = WorkState.Aborted;
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
