using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MangaCrawlerLib;
using System.IO;
using System.Diagnostics;

namespace MangaCrawler
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
        private bool m_finished;

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
                lock (m_lock)
                {
                    return m_downloadedPages;
                }
            }
        }

        public void PageDownloaded()
        {
            lock (m_lock)
            {
                m_downloadedPages++;
            }
        }

        public bool Error
        {
            get
            {
                lock (m_lock)
                {
                    return m_error;
                }
            }
        }

        public bool Waiting
        {
            get
            {
                lock (m_lock)
                {
                    return m_waiting;
                }
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
                lock (m_lock)
                {
                    return m_downloading;
                }
            }
            set
            {
                lock (m_lock)
                {
                    m_downloading = value;
                }
            }
        }

        public bool Finished
        {
            get
            {
                lock (m_lock)
                {
                    return m_finished;
                }
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
                if (Finished)
                    Initialize();
                else
                    m_cancellationTokenSource.Cancel();
            }
        }

        public string Progress
        {
            get
            {
                lock (m_lock)
                {
                    if (m_cancellationTokenSource.IsCancellationRequested & !Finished)
                        return "Deleting";
                    else if (Error)
                        return String.Format("Error ({0}/{1})", DownloadedPages, ChapterInfo.Pages.Count());
                    else if (Finished)
                        return String.Format("Downloaded");
                    else if (Downloading)
                    {
                        if (ChapterInfo.Pages == null)
                            return "Downloading";
                        else
                            return String.Format("Downloading ({0}/{1})", DownloadedPages, ChapterInfo.Pages.Count());
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
                m_finished = true;
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
                m_finished = false;
                m_cancellationTokenSource = new CancellationTokenSource();
                m_downloadedPages = 0;
                m_downloading = false;
                m_error = false;
            }
        }

        public override string ToString()
        {
            lock (m_lock)
            {
                if (Progress == "")
                    return ChapterInfo.Name;
                else
                    return String.Format("{0} - {1}", ChapterInfo.Name, Progress);
            }
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
