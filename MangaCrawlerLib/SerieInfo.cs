using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    public class SerieInfo
    {
        private string m_url;
        private IEnumerable<ChapterInfo> m_chapters;
        private Object m_lock = new Object();

        public ServerInfo ServerInfo { get; private set; }
        internal string URLPart { get; private set; }
        public int DownloadProgress { get; private set; }
        public string Title { get; private set; }
        public ItemState State { get; private set; }

        internal SerieInfo(ServerInfo a_serverInfo, string a_urlPart, string a_title)
        {
            URLPart = a_urlPart;
            ServerInfo = a_serverInfo;

            Title = a_title.Trim();
            Title = Title.Replace("\t", " ");
            while (Title.IndexOf("  ") != -1)
                Title = Title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(Title);
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

        public string URL
        {
            get
            {
                if (m_url == null)
                    m_url = HttpUtility.HtmlDecode(ServerInfo.Crawler.GetSerieURL(this));

                return m_url;
            }
        }

        internal void DownloadChapters()
        {
            try
            {
                State = ItemState.Downloading;
                DownloadProgress = 0;

                ServerInfo.Crawler.DownloadChapters(this, (progress, result) =>
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

                    DownloadProgress = progress;
                });

                State = ItemState.Downloaded;

            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                State = ItemState.Error;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", ServerInfo.Name, Title);
        }

        internal bool DownloadRequired
        {
            get
            {
                lock (m_lock)
                {
                    return (State == ItemState.Error) || (State == ItemState.Initial);
                }
            }
        }
    }
}
