using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    [DebuggerDisplay("SerieInfo, {ToString()}")]
    public class SerieInfo
    {
        private string m_title;
        private string m_url;
        private string m_urlPart;
        private IEnumerable<ChapterInfo> m_chapters;
        private ServerInfo m_serverInfo;
        private SerieState m_state;

        internal SerieInfo(ServerInfo a_serverInfo, string a_urlPart, string a_title)
        {
            m_urlPart = a_urlPart;
            m_serverInfo = a_serverInfo;

            m_title = a_title.Trim();
            m_title = m_title.Replace("\t", " ");
            while (m_title.IndexOf("  ") != -1)
                m_title = m_title.Replace("  ", " ");
            m_title = HttpUtility.HtmlDecode(m_title);
        }

        public SerieState State
        {
            get
            {
                if (m_state == null)
                    m_state = new SerieState(this);
                return m_state;
            }
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
            [DebuggerStepThrough]
            get
            {
                if (m_chapters == null)
                    return new ChapterInfo[0];

                return from ch in m_chapters
                       select ch;
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

        public string Title
        {
            get
            {
                return m_title;
            }
        }

        public void DownloadChapters(Action<int> a_progress_callback = null)
        {
            if (a_progress_callback != null)
                a_progress_callback(0);

            Crawler.DownloadChapters(this, (progress, result) =>
            {
                var chapters = result.ToList();

                if (m_chapters != null)
                {
                    foreach (var chapter in m_chapters)
                    {
                        var el = chapters.Find(s => (s.Title == chapter.Title) && (s.URL == chapter.URL));
                        if (el != null)
                            chapters[chapters.IndexOf(el)] = chapter;
                    }
                }

                m_chapters = chapters;

                if (a_progress_callback != null)
                    a_progress_callback(progress);
            });
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", ServerInfo.Name, Title);
        }
    }
}
