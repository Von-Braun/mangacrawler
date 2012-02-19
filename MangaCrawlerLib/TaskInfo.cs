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
    public class TaskInfo
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CancellationTokenSource m_cancellation_token_source = new CancellationTokenSource();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TaskState m_state = TaskState.Waiting;

        public List<PageInfo> Pages { get; private set; }
        public ChapterInfo Chapter { get; private set; }
        public string ChapterDir { get; private set; }
        public bool CBZ { get; private set; }

        internal TaskInfo()
        {
            Pages = new List<PageInfo>();
        }

        internal TaskInfo(ChapterInfo a_chapter_info, string a_manga_root_dir, bool a_cbz) : this()
        {
            Chapter = a_chapter_info;
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

        public TaskState State
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
                return (m_state == TaskState.Waiting) ||
                       (m_state == TaskState.Downloading) ||
                       (m_state == TaskState.Deleting) ||
                       (m_state == TaskState.Zipping);
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
                    "#1 operation cancelled, task: {0} state: {1}",
                    this, State);

                FinishDownload(true);
                return;
            }

            try
            {
                if (Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "#1 cancellation requested, task: {0} state: {1}",
                        this, State);

                    Token.ThrowIfCancellationRequested();
                }

                State = TaskState.Downloading;
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
                                "OperationCanceledException, task: {0} state: {1}, {2}",
                                this, State, ex1);

                            state.Break();
                        }
                        catch (Exception ex2)
                        {
                            Loggers.MangaCrawler.InfoFormat(
                                "1 Exception, task: {0} state: {1}, {2}",
                                this, State, ex2);

                            state.Break();
                            throw;
                        }
                    });

                if (Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "#3 cancellation requested, task: {0} state: {1}",
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
                    "#2 Exception, task: {0} state: {1}, {2}",
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
                "Task: {0} state: {1}",
                this, State);

            State = TaskState.Zipping;

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
                            "cancellation requested, task: {0} state: {1}",
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

        public void DeleteTask()
        {
            var s = State;

            Loggers.MangaCrawler.InfoFormat("Task: {0}, state: {1}", this, s);

            if ((s == TaskState.Downloading) ||
                (s == TaskState.Waiting) ||
                (s == TaskState.Zipping))
            {
                Loggers.MangaCrawler.Info("Cancelling tasks");
                m_cancellation_token_source.Cancel();

                State = TaskState.Deleting;
            }
        }

        public string TaskProgress
        {
            get
            {
                var s = State;
                
                switch (s)
                {
                    case TaskState.Error: 
                        return MangaCrawlerLib.Properties.Resources.TaskProgressError;
                    case TaskState.Aborted: 
                        return MangaCrawlerLib.Properties.Resources.TaskProgressAborted;
                    case TaskState.Waiting: 
                        return MangaCrawlerLib.Properties.Resources.TaskProgressWaiting;
                    case TaskState.Deleting: 
                        return MangaCrawlerLib.Properties.Resources.TaskProgressDeleting;
                    case TaskState.Downloaded: 
                        return MangaCrawlerLib.Properties.Resources.TaskProgressDownloaded;
                    case TaskState.Zipping: 
                        return MangaCrawlerLib.Properties.Resources.TaskProgressZipping;
                    case TaskState.Downloading: 
                        return String.Format("{0}/{1}", DownloadedPages, Pages.Count());
                    default: throw new NotImplementedException();
                }
            }
        }

        internal void FinishDownload(bool a_error)
        {
            var s = State;

            Loggers.MangaCrawler.InfoFormat("Task: {0}, state: {1}, error: {2}",
                this, s, a_error);

            if ((s == TaskState.Waiting) || (s == TaskState.Downloading))
            {
                if (a_error)
                    State = TaskState.Error;
                else
                {
                    if (DownloadedPages == Pages.Count())
                        State = TaskState.Downloaded;
                    else
                        State = TaskState.Error;
                }
            }

            if (s != TaskState.Downloaded)
            {
                if (m_cancellation_token_source.IsCancellationRequested)
                    State = TaskState.Aborted;
            }
        }

        public string TaskTitle
        {
            get
            {
                return String.Format(MangaCrawlerLib.Properties.Resources.DownloadingChapterInfo,
                     Chapter.Serie.Server.Name, Chapter.Serie.Title, Chapter.Title);
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
