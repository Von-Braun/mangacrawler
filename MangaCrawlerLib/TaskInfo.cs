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
using YAXLib;
using System.Threading.Tasks;
using Ionic.Zip;

namespace MangaCrawlerLib
{
    public class TaskInfo
    {
        private Object m_lock = new Object();
        private CancellationTokenSource m_cancellation_token_source = new CancellationTokenSource();

        [YAXNode("State")]
        private TaskState m_state = TaskState.Waiting;

        private ServerInfo m_server;

        public List<PageInfo> Pages { get; private set; }

        [YAXNode]
        internal string URL { get; private set; }

        [YAXNode]
        internal string URLPart { get; private set; }

        [YAXNode]
        public string Serie { get; private set; }

        [YAXNode]
        public string ServerName { get; private set; }

        [YAXNode]
        public string Chapter { get; private set; }

        [YAXNode]
        public string ImagesBaseDir { get; private set; }

        [YAXNode]
        public bool CBZ { get; private set; }

        internal TaskInfo()
        {
            Pages = new List<PageInfo>();
        }

        internal TaskInfo(ChapterInfo a_chapter_info, string a_images_base_dir, bool a_cbz) : this()
        {
            URL = a_chapter_info.URL;
            URLPart = a_chapter_info.URLPart;
            Serie = a_chapter_info.Serie.Title;
            ServerName = a_chapter_info.Serie.Server.Name;
            Chapter = a_chapter_info.Title;
            ImagesBaseDir = a_images_base_dir;
            CBZ = a_cbz;
        }

        public ServerInfo Server 
        {
            get
            {
                if (m_server == null)
                    m_server = DownloadManager.Servers.FirstOrDefault(s => s.Name == ServerName);
                return m_server;
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
                new DirectoryInfo(ImagesBaseDir).DeleteAll();

                if (Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "#1 cancellation requested, task: {0} state: {1}",
                        this, State);
                }

                Token.ThrowIfCancellationRequested();

                State = TaskState.Downloading;
                Pages.AddRange(Server.Crawler.DownloadPages(this));

                if (Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "#2 cancellation requested, task: {0} state: {1}",
                        this, State);
                }

                Token.ThrowIfCancellationRequested();

                Parallel.ForEach(Pages, 

                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = Server.Crawler.MaxConnectionsPerServer,
                        TaskScheduler = Server.Scheduler[Priority.Pages],
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
                }

                Token.ThrowIfCancellationRequested();

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
                    Token.ThrowIfCancellationRequested();
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
            return String.Format("{0} - {1} - {2}", Server, Serie, Chapter);
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
            Loggers.MangaCrawler.InfoFormat("Task: {0}, state: {1}", this, State);

            lock (m_lock)
            {
                if ((State == TaskState.Downloading) ||
                    (State == TaskState.Waiting) ||
                    (State == TaskState.Zipping))
                {
                    Loggers.MangaCrawler.Info("Cancelling tasks");
                    m_cancellation_token_source.Cancel();

                    State = TaskState.Deleting;
                }
            }
        }

        public string TaskProgress
        {
            get
            {
                lock (m_lock)
                {
                    switch (State)
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
        }

        internal void FinishDownload(bool a_error)
        {
            Loggers.MangaCrawler.InfoFormat("Task: {0}, state: {1}, error: {2}", 
                this, State, a_error);

            lock (m_lock)
            {
                if ((State == TaskState.Waiting) || (State == TaskState.Downloading))
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

                if (State != TaskState.Downloaded)
                {
                    if (m_cancellation_token_source.IsCancellationRequested)
                        State = TaskState.Aborted;
                }
            }
        }

        public string TaskTitle
        {
            get
            {
                return String.Format(MangaCrawlerLib.Properties.Resources.DownloadingChapterInfo,
                    Server, Serie, Chapter);
            }
        }

        public string GetImagesDirectory()
        {
            if (ImagesBaseDir.Last() == Path.DirectorySeparatorChar)
                ImagesBaseDir = ImagesBaseDir.RemoveFromRight(1);

            return ImagesBaseDir +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(ServerName) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(Serie) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(Chapter) +
                   Path.DirectorySeparatorChar;
        }
    }
}
