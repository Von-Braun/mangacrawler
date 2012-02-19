using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    public class Serie
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Chapter> m_chapters = new List<Chapter>();

        public Server Server { get; private set; }
        public string URL { get; private set; }
        public int DownloadProgress { get; private set; }
        public string Title { get; private set; }
        public SerieState State;

        internal Serie(Server a_server, string a_url, string a_title)
        {
            URL = HttpUtility.HtmlDecode(a_url);
            Server = a_server;

            Title = a_title.Trim();
            Title = Title.Replace("\t", " ");
            while (Title.IndexOf("  ") != -1)
                Title = Title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(Title);
        }

        public IEnumerable<Chapter> Chapters
        {
            get
            {
                return m_chapters;
            }
        }

        internal void DownloadChapters()
        {
            try
            {
                DownloadProgress = 0;

                Server.Crawler.DownloadChapters(this, (progress, result) =>
                {
                    var chapters = result.ToList();

                    foreach (var chapter in m_chapters)
                    {
                        var el = chapters.Find(s => (s.Title == chapter.Title) && (s.URL == chapter.URL));
                        if (el != null)
                            chapters[chapters.IndexOf(el)] = chapter;
                    }

                    m_chapters = chapters;

                    DownloadProgress = progress;
                });

                State = SerieState.Downloaded;

            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                State = SerieState.Error;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Server.Name, Title);
        }

        internal bool DownloadRequired
        {
            get
            {
                var s = State;
                return (s == SerieState.Error) || (s == SerieState.Initial);
            }
        }
    }
}
