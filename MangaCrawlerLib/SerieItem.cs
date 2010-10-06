using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    [DebuggerDisplay("{SerieInfo}")]
    public class SerieItem
    {
        public readonly SerieInfo SerieInfo;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Object m_lock = new Object();

        private int m_progress;

        private bool m_error;
        private bool m_downloading;
        private bool m_downloaded;


        public SerieItem(SerieInfo a_info)
        {
            SerieInfo = a_info;
            Initialize();
        }

        public int Progress
        {
            get
            {
                return m_progress;
            }
            set
            {
                lock (m_lock)
                {
                    m_progress = value;
                }
            }
        }

        public void Initialize()
        {
            lock (m_lock)
            {
                m_progress = 0;
                m_error = false;
                m_downloading = false;
                m_downloaded = false;
            }
        }

        public override string ToString()
        {
            lock (m_lock)
            {
                if (m_error)
                    return SerieInfo.Name + " (Error)";
                else if (m_downloading)
                    return String.Format("{0} ({1}%)", SerieInfo.Name, m_progress);
                else if (m_downloaded)
                    return SerieInfo.Name + " (OK)";
                else
                    return SerieInfo.Name;
            }
        }

        public bool Error
        {
            get
            {
                return m_error;
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
            set
            {
                lock (m_lock)
                {
                    m_downloaded = value;
                }
            }
        }
    }
}
