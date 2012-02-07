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

namespace MangaCrawlerLib
{
    public class ChapterInfo
    {
        private string m_url;
        private Object m_lock = new Object();
        private CancellationTokenSource m_cancellation_token_source;
        private ItemState m_state;

        public List<PageInfo> Pages { get; private set; }
        internal string URLPart { get; private set; }
        public SerieInfo SerieInfo { get; private set; }
        public string Title { get; private set; }
        
        internal ChapterInfo(SerieInfo a_serieInfo, string a_urlPart, string a_title)
        {
            SerieInfo = a_serieInfo;
            URLPart = a_urlPart;

            Title = a_title.Trim();
            Title = Title.Replace("\t", " ");
            while (Title.IndexOf("  ") != -1)
                Title = Title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(Title);

            Pages = new List<PageInfo>();
        }

        public ItemState State
        {
            get
            {
                return m_state;
            }
            set
            {
                System.Diagnostics.Debug.WriteLine(
                    "ChapterInfo.State - {0} -> {1}", 
                    State, value);

                m_state = value;
            }
        }

        internal void DownloadPages()
        {
            State = ItemState.Downloading;
            Pages.AddRange(SerieInfo.ServerInfo.Crawler.DownloadPages(this));
        }

        public string URL
        {
            get
            {
                if (m_url == null)
                    m_url = HttpUtility.HtmlDecode(SerieInfo.ServerInfo.Crawler.GetChapterURL(this));

                return m_url;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", SerieInfo, Title);
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
            System.Diagnostics.Debug.WriteLine(
                "ChapterInfo.DeleteTask - chapter: {0}, state: {1}", 
                Title, State);

            lock (m_lock)
            {
                if ((State == ItemState.Downloaded) ||
                    (State == ItemState.Error))
                {
                    InitializeDownload();
                }
                else if (State != ItemState.Initial)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "ChapterInfo.DeleteTask - cancelling tasks");
                    m_cancellation_token_source.Cancel();
                    
                    State = ItemState.Deleting;
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
                        case ItemState.Error: 
                            return MangaCrawlerLib.Properties.Resources.TaskProgressError;
                        case ItemState.Downloaded: 
                            return MangaCrawlerLib.Properties.Resources.TaskProgressDownloaded;
                        case ItemState.DownloadedMissingPages: 
                            return MangaCrawlerLib.Properties.Resources.TaskProgressDownloadedMissingPages;
                        case ItemState.Waiting: 
                            return MangaCrawlerLib.Properties.Resources.TaskProgressWaiting;
                        case ItemState.Deleting: 
                            return MangaCrawlerLib.Properties.Resources.TaskProgressDeleting;
                        case ItemState.Zipping: 
                            return MangaCrawlerLib.Properties.Resources.TaskProgressZipping;
                        case ItemState.Downloading: 
                            return String.Format("{0}/{1}", DownloadedPages, Pages.Count());
                        case ItemState.Initial: 
                            return "";
                        default: throw new NotImplementedException();
                    }
                }
            }
        }

        public bool IsTask
        {
            get
            {
                lock (m_lock)
                {
                    return (State == ItemState.Waiting) || (State == ItemState.Error) ||
                        (State == ItemState.Deleting) || (State == ItemState.Downloading) ||
                        (State == ItemState.Zipping);
                }
            }
        }

        public bool Working
        {
            get
            {
                lock (m_lock)
                {
                    return (State == ItemState.Waiting) || (State == ItemState.Downloading) ||
                           (State == ItemState.Zipping) || (State == ItemState.Deleting);
                }
            }
        }

        internal void FinishDownload(bool a_error)
        {
            System.Diagnostics.Debug.WriteLine(
                "ChapterInfo.FinishDownload - chapter: {0}, state: {1}, error: {2}", 
                Title, State, a_error);

            lock (m_lock)
            {
                if ((State == ItemState.Waiting) || (State == ItemState.Downloading))
                {
                    if (a_error)
                        State = ItemState.Error;
                    else
                    {
                        if (DownloadedPages == Pages.Count())
                            State = ItemState.Downloaded;
                        else
                            State = ItemState.DownloadedMissingPages;
                    }
                }

                if (m_cancellation_token_source.IsCancellationRequested)
                    InitializeDownload();
            }
        }

        internal void InitializeDownload()
        {
            System.Diagnostics.Debug.WriteLine(
                "ChapterInfo.InitializeDownload - chapter: {0}, state: {1}", 
                Title, State);

            lock (m_lock)
            {
                State = ItemState.Initial;

                if (m_cancellation_token_source != null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "ChapterInfo.InitializeDownload - cancelling tokens");
                    m_cancellation_token_source.Cancel();
                }

                System.Diagnostics.Debug.WriteLine(
                    "ChapterInfo.InitializeDownload - new token source");
                m_cancellation_token_source = new CancellationTokenSource();
                System.Diagnostics.Debug.WriteLine(
                    "ChapterInfo.InitializeDownload - clearing pages");
                Pages.Clear();
            }
        }

        public string TaskTitle
        {
            get
            {
                return String.Format(MangaCrawlerLib.Properties.Resources.DownloadingChapterInfo,
                    SerieInfo.ServerInfo.Name, SerieInfo.Title, Title);
            }
        }

        public string GetImageDirectory(string a_directoryBase)
        {
            if (a_directoryBase.Last() == Path.DirectorySeparatorChar)
                a_directoryBase = a_directoryBase.RemoveFromRight(1);

            return a_directoryBase +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(SerieInfo.ServerInfo.Name) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(SerieInfo.Title) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(Title) +
                   Path.DirectorySeparatorChar;
        }
    }
}
