using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Threading;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    [DebuggerDisplay("ChapterInfo, {ToString()}")]
    public class ChapterInfo
    {
        private string m_title;
        private List<PageInfo> m_pages;
        private string m_url;
        private string m_urlPart;
        private SerieInfo m_serieInfo;
        private ChapterState m_state;

        internal ChapterInfo(SerieInfo a_serieInfo, string a_urlPart, string a_title)
        {
            m_serieInfo = a_serieInfo;
            m_urlPart = a_urlPart;

            m_title = a_title.Trim();
            m_title = m_title.Replace("\t", " ");
            while (m_title.IndexOf("  ") != -1)
                m_title = m_title.Replace("  ", " ");
            m_title = HttpUtility.HtmlDecode(m_title);
        }

        public ChapterState State
        {
            get
            {
                if (m_state == null)
                    m_state = new ChapterState(this);
                return m_state;
            }

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

        public string Title
        {
            get
            {
                return m_title;
            }
        }

        public void DownloadPages()
        {
            m_pages = Crawler.DownloadPages(this).ToList();
        }

        public IEnumerable<PageInfo> Pages
        {
            [DebuggerStepThrough]
            get
            {
                if (m_pages == null)
                    return new PageInfo[0];

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
            return String.Format("{0} - {1}", SerieInfo, Title);
        }
    }
}
