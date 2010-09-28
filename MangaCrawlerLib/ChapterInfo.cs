using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Threading;

namespace MangaCrawlerLib
{
    public class ChapterInfo
    {
        private string m_name;
        private List<PageInfo> m_pages;
        private string m_url;
        private string m_urlPart;
        private SerieInfo m_serieInfo;

        internal ChapterInfo(SerieInfo a_serieInfo, string a_urlPart, string a_name)
        {
            m_serieInfo = a_serieInfo;
            m_urlPart = a_urlPart;

            m_name = a_name.Trim();
            m_name = m_name.Replace("\t", " ");
            while (m_name.IndexOf("  ") != -1)
                m_name = m_name.Replace("  ", " ");
        }

        public SerieInfo SerieInfo
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

        public void DownloadPages(CancellationToken a_token)
        {
            m_pages = Crawler.DownloadPages(this, a_token).ToList();
        }

        public IEnumerable<PageInfo> Pages
        {
            get
            {
                if (m_pages == null)
                    return null;

                return from page in m_pages
                       select page;
            }
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

        public override string ToString()
        {
            return String.Format("{0} - {1}", SerieInfo, Name);
        }
    }
}
