using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;
using System.Diagnostics;

namespace MangaCrawler
{
    [DebuggerDisplay("{ServerInfo}")]
    public class ServerItem
    {
        public readonly ServerInfo ServerInfo;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Object m_lock = new Object();

        private int m_progress;

        private bool m_error;
        private bool m_downloading;
        private bool m_finished;

        public ServerItem(ServerInfo a_info)
        {
            ServerInfo = a_info;
            Initialize();
        }

        public void SetProgress(int a_progress)
        {
            lock (m_lock)
            {
                m_progress = a_progress;
            }
        }

        public void Initialize()
        {
            lock (m_lock)
            {
                m_progress = 0;
                m_error = false;
                m_downloading = false;
                m_finished = false;
            }
        }

        public override string ToString()
        {
            lock (m_lock)
            {
                if (m_error)
                    return ServerInfo.Name + " - error";
                if (m_downloading)
                {
                    if (m_progress != 0)
                        return String.Format("{0} - downloading {1}%", ServerInfo.Name, m_progress);
                    else
                        return ServerInfo.Name + " - downloading";
                }
                else if (m_finished)
                    return ServerInfo.Name + " - downloaded";
                else
                    return ServerInfo.Name;
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
            set
            {
                lock (m_lock)
                {
                    m_error = value;
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
            set
            {
                lock (m_lock)
                {
                    m_finished = value;
                }
            }
        }
    }
}
