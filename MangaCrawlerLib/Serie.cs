using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading;

namespace MangaCrawlerLib
{
    public class Serie : Entity
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Object m_lock = new Object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SerieState m_state;

        public Server Server { get; protected set; }
        public string Title { get; protected set; }
        public int DownloadProgress { get; protected set; }
        public List<Chapter> Chapters { get; protected set; }

        internal Serie(Server a_server, string a_url, string a_title)
        {
            Chapters = new List<Chapter>();
            URL = HttpUtility.HtmlDecode(a_url);
            Server = a_server;

            a_title = a_title.Trim();
            a_title = a_title.Replace("\t", " ");
            while (a_title.IndexOf("  ") != -1)
                a_title = a_title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(a_title);
        }

        internal override Crawler Crawler
        {
            get
            {
                return Server.Crawler;
            }
        }

        internal void DownloadChapters()
        {
            try
            {
                Crawler.DownloadChapters(this, (progress, result) =>
                {
                    lock (m_lock)
                    {
                        IList<Chapter> added;
                        IList<Chapter> removed;
                        List<Chapter> chapters = Chapters.ToList();
                        Sync(result, chapters, chapter => (chapter.Title + chapter.URL),
                            progress == 100, out added, out removed);
                        Chapters = chapters;
                    }

                    DownloadProgress = progress;
                });

                State = SerieState.Downloaded;

            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                State = SerieState.Downloaded;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Server.Name, Title);
        }

        public bool DownloadRequired
        {
            get
            {
                return (State == SerieState.Error) || (State == SerieState.Initial);
            }
        }

        public SerieState State
        {
            get
            {
                return m_state;
            }
            set
            {
                switch (value)
                {
                    case SerieState.Initial:
                    {
                        break;
                    }
                    case SerieState.Waiting:
                    {
                        Debug.Assert((State == SerieState.Downloaded) ||
                                     (State == SerieState.Initial) ||
                                     (State == SerieState.Error));
                        break;
                    }
                    case SerieState.Downloading:
                    {
                        Debug.Assert(State == SerieState.Waiting);
                        DownloadProgress = 0;
                        break;
                    }
                    case SerieState.Downloaded:
                    {
                        Debug.Assert(State == SerieState.Downloading);
                        Debug.Assert(DownloadProgress == 100);
                        break;
                    }
                    case SerieState.Error:
                    {
                        Debug.Assert(State == SerieState.Downloading);
                        break;
                    }
                    default:
                    {
                        throw new InvalidOperationException(String.Format("Unknown state: {0}", value));
                    }
                }

                m_state = value;
            }
        }
    }
}
