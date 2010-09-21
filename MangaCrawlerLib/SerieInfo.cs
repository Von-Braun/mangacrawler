using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MangaCrawlerLib
{
    public class SerieInfo
    {
        private string m_name;
        private string m_url;
        private volatile int m_downloadingProgress;
        private string m_urlPart;
        private List<ChapterInfo> m_chapters;
        private volatile bool m_downloadingChapters;
        private ServerInfo m_serverInfo;

        internal SerieInfo(ServerInfo a_serverInfo, string a_urlPart, string a_name)
        {
            m_urlPart = a_urlPart;
            m_serverInfo = a_serverInfo;

            m_name = a_name.Trim();
            m_name = m_name.Replace("\t", " ");
            while (m_name.IndexOf("  ") != -1)
                m_name = m_name.Replace("  ", " ");
        }

        internal Crawler Crawler
        {
            get
            {
                return m_serverInfo.Crawler;
            }
        }

        public IEnumerable<ChapterInfo> Chapters
        {
            get
            {
                if (m_chapters == null)
                    return null;
                return from ch in m_chapters
                       select ch;
            }
        }

        public bool DownloadingChapters
        {
            get
            {
                return m_downloadingChapters;
            }
        }

        public ServerInfo ServerInfo
        {
            get
            {
                return m_serverInfo;
            }
        }

        public string URL
        {
            get
            {
                if (m_url == null)
                    m_url = HttpUtility.HtmlDecode(Crawler.GetSerieURL(this));

                return m_url;
            }
        }

        internal string URLPart
        {
            get
            {
                return m_urlPart;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public void DownloadChapters(Action a_progress_callback = null)
        {
            if (m_downloadingChapters)
                return;

            if (Chapters == null)
            {
                m_downloadingChapters = true;

                try
                {
                    if (a_progress_callback != null)
                        a_progress_callback();

                    m_chapters = Crawler.DownloadChapters(this, (progress) =>
                    {
                        m_downloadingProgress = progress;
                        if (a_progress_callback != null)
                            a_progress_callback();
                    }).ToList();
                }
                finally
                {
                    m_downloadingChapters = false;
                }

                if (a_progress_callback != null)
                    a_progress_callback();
            }
        }

        internal string DecoratedName
        {
            get
            {
                if (m_downloadingChapters)
                    return String.Format("{0} ({1}%)", m_name, m_downloadingProgress);
                else if (Chapters == null)
                    return m_name;
                else
                    return m_name + "*";
            }
        }

        public override string ToString()
        {
            return DecoratedName;
        }
    }
}
