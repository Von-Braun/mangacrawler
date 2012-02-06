using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Threading;
using System.Diagnostics;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    public class ChapterInfo
    {
        private string m_title;
        private List<PageInfo> m_pages;
        private string m_url;
        private string m_urlPart;
        private SerieInfo m_serieInfo;

        private Object m_lock = new Object();
        private CancellationTokenSource m_cancellation_token_source;
        private ItemState m_state;

        internal ChapterInfo(SerieInfo a_serieInfo, string a_urlPart, string a_title)
        {
            m_serieInfo = a_serieInfo;
            m_urlPart = a_urlPart;

            m_title = a_title.Trim();
            m_title = m_title.Replace("\t", " ");
            while (m_title.IndexOf("  ") != -1)
                m_title = m_title.Replace("  ", " ");
            m_title = HttpUtility.HtmlDecode(m_title);

            InitializeDownload();
        }

        public SerieInfo SerieInfo
        {
            get
            {
                return m_serieInfo;
            }
        }

        internal string URLPart
        {
            get
            {
                return m_urlPart;
            }
        }

        public string Title
        {
            get
            {
                return m_title;
            }
        }

        public void DownloadPages()
        {
            m_pages = SerieInfo.ServerInfo.Crawler.DownloadPages(this).ToList();
        }

        public IEnumerable<PageInfo> Pages
        {
            [DebuggerStepThrough]
            get
            {
                if (m_pages == null)
                    return new PageInfo[0];

                return from page in m_pages
                       select page;
            }
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

        public CancellationToken Token
        {
            get
            {
                return m_cancellation_token_source.Token;
            }
        }

        public void DeleteTask()
        {
            lock (m_lock)
            {
                if ((m_state == ItemState.Downloaded) ||
                    (m_state == ItemState.Error))
                {
                    InitializeDownload();
                }
                else if (m_state != ItemState.Initial)
                {
                    m_cancellation_token_source.Cancel();
                    m_state = ItemState.Deleting;
                }
            }
        }

        public ItemState State
        {
            get
            {
                return m_state;
            }
            set
            {
                lock (m_lock)
                {
                    m_state = value;
                }
            }
        }

        public string TaskProgress
        {
            get
            {
                lock (m_lock)
                {
                    switch (m_state)
                    {
                        case ItemState.Error: return MangaCrawlerLib.Properties.Resources.TaskProgressError;
                        case ItemState.Downloaded: return MangaCrawlerLib.Properties.Resources.TaskProgressDownloaded;
                        case ItemState.DownloadedMissingPages: return MangaCrawlerLib.Properties.Resources.TaskProgressDownloadedMissingPages;
                        case ItemState.Waiting: return MangaCrawlerLib.Properties.Resources.TaskProgressWaiting;
                        case ItemState.Deleting: return MangaCrawlerLib.Properties.Resources.TaskProgressDeleting;
                        case ItemState.Zipping: return MangaCrawlerLib.Properties.Resources.TaskProgressZipping;
                        case ItemState.Downloading: return String.Format("{0}/{1}", DownloadedPages, Pages.Count());
                        case ItemState.Initial: return "";
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
                    return (m_state == ItemState.Waiting) || (m_state == ItemState.Error) ||
                        (m_state == ItemState.Deleting) || (m_state == ItemState.Downloading) ||
                        (m_state == ItemState.Zipping);
                }
            }
        }

        public bool Working
        {
            get
            {
                lock (m_lock)
                {
                    return (m_state == ItemState.Waiting) || (m_state == ItemState.Downloading) ||
                           (m_state == ItemState.Zipping) || (m_state == ItemState.Deleting);
                }
            }
        }

        public void FinishDownload(bool a_error)
        {
            lock (m_lock)
            {
                if ((m_state == ItemState.Waiting) || (m_state == ItemState.Downloading))
                {
                    if (a_error)
                        m_state = ItemState.Error;
                    else
                    {
                        if (DownloadedPages == Pages.Count())
                            m_state = ItemState.Downloaded;
                        else
                            m_state = ItemState.DownloadedMissingPages;
                    }
                }

                if (m_cancellation_token_source.IsCancellationRequested)
                    InitializeDownload();
            }
        }

        public void InitializeDownload()
        {
            lock (m_lock)
            {
                m_state = ItemState.Initial;

                if (m_cancellation_token_source != null)
                    m_cancellation_token_source.Cancel();

                m_cancellation_token_source = new CancellationTokenSource();
                m_pages.Clear();
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
