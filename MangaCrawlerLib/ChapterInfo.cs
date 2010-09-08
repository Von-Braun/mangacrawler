using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace MangaCrawlerLib
{
    public class ChapterInfo
    {
        private string m_name;
        private string m_URLPart;
        private List<PageInfo> m_pages;
        private string m_url;

        public SerieInfo SerieInfo;
        internal int PagesCount;
        public volatile int DownloadedPages;
        public volatile bool Downloading = false;
        public volatile bool Queue = false;

        internal Crawler Crawler
        {
            get
            {
                return SerieInfo.Crawler;
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
                m_URLPart = value;
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

        public List<PageInfo> Pages
        {
            get
            {
                if (m_pages == null)
                    m_pages = Crawler.DownloadPages(this).ToList();

                return m_pages;
            }
        }

        internal string GetDecoratedName()
        {
            if (!Downloading && (DownloadedPages == 0))
            {
                if (Queue)
                    return m_name + " (queued)";
                else
                    return m_name;
            }
            else if (!Downloading && (DownloadedPages == PagesCount) && (DownloadedPages != 0))
                return m_name + "*";
            else if (Downloading)
                return String.Format("{0} ({1}/{2})", m_name, DownloadedPages, PagesCount);
            else
                return m_name;
        }

        public override string ToString()
        {
            return GetDecoratedName();
        }

        public string URL
        {
            get
            {
                if (m_url == null)
                    m_url = HttpUtility.HtmlDecode(Crawler.GetChapterURL(this));

                return m_url;
            }
        }
    }
}
