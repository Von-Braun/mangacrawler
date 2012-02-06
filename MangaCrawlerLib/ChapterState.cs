using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MangaCrawlerLib;
using System.IO;
using System.Diagnostics;
using MangaCrawlerLib.Properties;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    [DebuggerDisplay("ChapterState, {ToString()}")]
    public class ChapterState
    {
        private Object m_lock = new Object();
        private CancellationTokenSource m_cancellation_token_source;
        private int m_downloaded_pages;
        private ItemState m_state;
        private ChapterInfo m_chapter_info;

        public ChapterState(ChapterInfo a_chapterInfo)
        {
            m_chapter_info = a_chapterInfo;
            Initialize();
        }

        public int DownloadedPages
        {
            get
            {
                return m_downloaded_pages;
            }
        }

        public void PageDownloaded()
        {
            Interlocked.Increment(ref m_downloaded_pages);
        }

        public CancellationToken Token
        {
            get
            {
                return m_cancellation_token_source.Token;
            }
        }

        public string Chapter
        {
            get
            {
                lock (m_lock)
                {
                    return String.Format(Resources.DownloadingChapterInfo,
                        m_chapter_info.SerieInfo.ServerInfo.Name, m_chapter_info.SerieInfo.Title, m_chapter_info.Title);
                }
            }
        }

        public void Delete()
        {
            lock (m_lock)
            {
                if ((m_state == ItemState.Downloaded) ||
                    (m_state == ItemState.Error))
                {
                    Initialize();
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
                        case ItemState.Downloading: return String.Format("{0}/{1}", DownloadedPages, m_chapter_info.Pages.Count());
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

        public void Finish(bool a_error)
        {
            lock (m_lock)
            {
                if ((m_state == ItemState.Waiting) || (m_state == ItemState.Downloading))
                {
                    if (a_error)
                        m_state = ItemState.Error;
                    else
                    {
                        if (DownloadedPages == m_chapter_info.Pages.Count())
                            m_state = ItemState.Downloaded;
                        else
                            m_state = ItemState.DownloadedMissingPages;
                    }
                }

                if (m_cancellation_token_source.IsCancellationRequested)
                    Initialize();
            }
        }

        public void Initialize()
        {
            lock (m_lock)
            {
                m_state = ItemState.Initial;

                if (m_cancellation_token_source != null)
                    m_cancellation_token_source.Cancel();

                m_cancellation_token_source = new CancellationTokenSource();
                m_downloaded_pages = 0;
            }
        }

        public override string ToString()
        {
            lock (m_lock)
            {
                return String.Format("name: {0}, state: {1}", m_chapter_info.Title, m_state);
            }
        }

        public string GetImageDirectory(string a_directoryBase)
        {
            if (a_directoryBase.Last() == Path.DirectorySeparatorChar)
                a_directoryBase = a_directoryBase.RemoveFromRight(1);

            return a_directoryBase +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(m_chapter_info.SerieInfo.ServerInfo.Name) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(m_chapter_info.SerieInfo.Title) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(m_chapter_info.Title) +
                   Path.DirectorySeparatorChar;
        }
    }
}
