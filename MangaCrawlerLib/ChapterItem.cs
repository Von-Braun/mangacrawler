﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MangaCrawlerLib;
using System.IO;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    public class ChapterItem
    {
        private Object m_lock = new Object();
        private CancellationTokenSource m_cancellationTokenSource;
        private int m_downloadedPages;
        private ItemState m_state;

        public readonly ChapterInfo ChapterInfo;

        public ChapterItem(ChapterInfo a_chapterInfo)
        {
            ChapterInfo = a_chapterInfo;
            Initialize();
        }

        public int DownloadedPages
        {
            get
            {
                return m_downloadedPages;
            }
        }

        public void PageDownloaded()
        {
            Interlocked.Increment(ref m_downloadedPages);
        }

        public CancellationToken Token
        {
            get
            {
                return m_cancellationTokenSource.Token;
            }
        }

        public string Chapter
        {
            get
            {
                lock (m_lock)
                {
                    return String.Format("server: {0}\nserie: {1}\nchapter: {2}",
                        ChapterInfo.SerieInfo.ServerInfo.Name, ChapterInfo.SerieInfo.Name, ChapterInfo.Name);
                }
            }
        }

        public void Delete()
        {
            lock (m_lock)
            {
                if ((m_state == ItemState.Downloaded) || (m_state == ItemState.Error))
                    Initialize();
                else
                {
                    m_cancellationTokenSource.Cancel();
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
                        case ItemState.Error: return MangaCrawlerLib.Properties.Resources.Task_Progress_Error;
                        case ItemState.Downloaded: return MangaCrawlerLib.Properties.Resources.Task_Progress_Downloaded;
                        case ItemState.Waiting: return MangaCrawlerLib.Properties.Resources.Task_Progress_Waiting;
                        case ItemState.Deleting: return MangaCrawlerLib.Properties.Resources.Task_Progress_Deleting;
                        case ItemState.Zipping: return MangaCrawlerLib.Properties.Resources.Task_Progress_Zipping;
                        case ItemState.Downloading: return String.Format("{0}/{1}", DownloadedPages, ChapterInfo.Pages.Count());
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
                return (State == ItemState.Waiting) || (State == ItemState.Error) || (State == ItemState.Deleting) ||
                      (State == ItemState.Downloading) || (State == ItemState.Zipping);
            }
        }

        public bool Working
        {
            get
            {
                return (State == ItemState.Waiting) || (State == ItemState.Downloading) ||
                       (State == ItemState.Zipping) || (State == ItemState.Deleting);
            }
        }

        public void Finish(bool a_error)
        {
            lock (m_lock)
            {
                if (a_error)
                    m_state = ItemState.Error;
                else
                    m_state = ItemState.Downloaded;

                if (m_cancellationTokenSource.IsCancellationRequested)
                    Initialize();
            }
        }

        public void Initialize()
        {
            lock (m_lock)
            {
                m_state = ItemState.Initial;
                m_cancellationTokenSource = new CancellationTokenSource();
                m_downloadedPages = 0;
            }
        }

        public override string ToString()
        {
            return String.Format("name: {0}, state: {1}", ChapterInfo.Name, m_state);
        }

        public string GetImageDirectory(string a_directoryBase)
        {
            if (a_directoryBase.Last() == Path.DirectorySeparatorChar)
                a_directoryBase = a_directoryBase.RemoveFromRight(1);

            return a_directoryBase +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(ChapterInfo.SerieInfo.ServerInfo.Name) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(ChapterInfo.SerieInfo.Name) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileDirectoryCharacters(ChapterInfo.Name) +
                   Path.DirectorySeparatorChar;
        }

    }
}
