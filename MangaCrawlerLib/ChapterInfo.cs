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
        private List<PageInfo> m_pages;
        private string m_url;
        private string m_urlPart;
        private SerieInfo m_serieInfo;

        internal int PagesCount;

        public volatile int DownloadedPages;
        public volatile bool Downloading = false;
        public volatile bool Queue = false;

        internal ChapterInfo(SerieInfo a_serieInfo, string a_urlPart, string a_name)
        {
            m_serieInfo = a_serieInfo;
            m_urlPart = a_urlPart;

            m_name = a_name.Trim();
            m_name = m_name.Replace("\t", " ");
            while (m_name.IndexOf("  ") != -1)
                m_name = m_name.Replace("  ", " ");
        }

        internal SerieInfo SerieInfo
        {
            get
            {
                return m_serieInfo;
            }
        }

        internal string URLPart
        {
            get
            {
                return m_urlPart;
            }
        }

        internal Crawler Crawler
        {
            get
            {
                return SerieInfo.Crawler;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public IEnumerable<PageInfo> Pages
        {
            get
            {
                if (m_pages == null)
                    m_pages = Crawler.DownloadPages(this).ToList();

                return from page in m_pages
                       select page;
            }
        }

        internal string DecoratedName
        {
            get
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
        }

        public override string ToString()
        {
            return DecoratedName;
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
