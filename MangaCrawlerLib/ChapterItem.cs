using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MangaCrawlerLib;
using System.IO;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    [DebuggerDisplay("{ChapterInfo}")]
    public class ChapterItem
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Object m_lock = new Object();

        private CancellationTokenSource m_cancellationTokenSource;
        private int m_downloadedPages;

        private bool m_error;
        private bool m_waiting;
        private bool m_downloading;
        private bool m_downloaded;

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

        public bool Error
        {
            get
            {
                return m_error;
            }
        }

        public bool Waiting
        {
            get
            {
                return m_waiting;
            }
            set
            {
                lock (m_lock)
                {
                    m_waiting = value;
                }
            }
        }

        public bool Downloading
        {
            get
            {
                return m_downloading;
            }
            set
            {
                lock (m_lock)
                {
                    m_downloading = value;
                }
            }
        }

        public bool Downloaded
        {
            get
            {
                return m_downloaded;
            }
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
                if (Downloaded || Error)
                    Initialize();
                else
                    m_cancellationTokenSource.Cancel();
            }
        }

        public bool Deleting
        {
            get
            {
                return m_cancellationTokenSource.IsCancellationRequested & !(Downloaded || Error);
            }
        }
        
        // TODO: owner draw
        public string TaskProgress
        {
            get
            {
                lock (m_lock)
                {
                    if (Deleting)
                        return "Deleting";
                    else if (Error)
                        return "Error";
                    else if (Downloaded)
                        return "Downloaded";
                    else if (Downloading)
                    {
                        return String.Format("{0}/{1}", DownloadedPages,
                            (ChapterInfo.Pages == null) ? 0 : ChapterInfo.Pages.Count());
                    }
                    else if (Waiting)
                        return "Waiting";
                    else
                        return "";
                }
            }
        }

        public void Finish(bool a_error)
        {
            lock (m_lock)
            {
                m_error = a_error;
                m_downloaded = !a_error;
                m_downloading = false;

                if (m_cancellationTokenSource.IsCancellationRequested)
                    Initialize();
            }
        }

        public void Initialize()
        {
            lock (m_lock)
            {
                m_waiting = false;
                m_downloaded = false;
                m_cancellationTokenSource = new CancellationTokenSource();
                m_downloadedPages = 0;
                m_downloading = false;
                m_error = false;
            }
        }

        public override string ToString()
        {
            return ChapterInfo.Name + " " + TaskProgress;
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
