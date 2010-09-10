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
        internal string URLPart;
        public List<ChapterInfo> Chapters;
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
                    m_url = HttpUtility.HtmlDecode(Crawler.GetSerieURL(this));

                return m_url;
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

            if (Chapters == null)
            {
                DownloadingChapters = true;

                try
                {
                    if (a_progress_callback != null)
                        a_progress_callback();

                    Chapters = Crawler.DownloadChapters(this, (progress) =>
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

        internal string GetDecoratedName()
        {
            if (DownloadingChapters)
                return String.Format("{0} ({1}%)", m_name, m_downloadingProgress);
            else if (Chapters == null)
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
