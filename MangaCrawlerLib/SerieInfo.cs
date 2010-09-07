using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    public class SerieInfo
    {
        private string m_name;
        private string m_URLPart;
        private List<ChapterInfo> m_chapters;
        private string m_url;
        private volatile int m_downloadingProgress;

        public volatile bool DownloadingChapters;
        public ServerInfo ServerInfo;

        internal Crawler Crawler
        {
            get
            {
                return ServerInfo.Crawler;
            }
        }

        public string URL
        {
            get
            {
                if (m_url == null)
                    m_url = Crawler.GetSerieURL(this);

                return m_url;
            }
        }

        internal string URLPart
        {
            get
            {
                return m_URLPart;
            }
            set
            {
                m_URLPart = System.Web.HttpUtility.UrlDecode(value);
            }
        }

        internal string InternalName
        {
            get
            {
                return m_name;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value.Trim();
                m_name = m_name.Replace("\t", " ");
                while (m_name.IndexOf("  ") != -1)
                    m_name = m_name.Replace("  ", " ");
            }
        }

        public void DownloadChapters(Action a_progress_callback = null)
        {
            if (DownloadingChapters)
                return;

            if (m_chapters == null)
            {
                DownloadingChapters = true;

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
                    DownloadingChapters = false;
                }

                if (a_progress_callback != null)
                    a_progress_callback();
            }
        }

        public List<ChapterInfo> Chapters
        {
            get
            {
                return m_chapters;
            }
        }

        internal string GetDecoratedName()
        {
            if (DownloadingChapters)
                return String.Format("{0} ({1}%)", m_name, m_downloadingProgress);
            else if (m_chapters == null)
                return m_name;
            else
                return m_name + "*";
        }

        public override string ToString()
        {
            return GetDecoratedName();
        }
    }
}
