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
        #region ChaptersCachedList
        private class ChaptersCachedList : CachedList<Chapter>
        {
            private Serie m_serie;

            public ChaptersCachedList(Serie a_serie)
            {
                m_serie = a_serie;
            }

            protected override void EnsureLoaded()
            {
                lock (m_load_from_xml_lock)
                {
                    if (m_list != null)
                        return;

                    m_list = Catalog.LoadSerieChapters(m_serie);

                    if (m_list.Count != 0)
                        m_loaded_from_xml = true;
                }
            }

            internal override void ClearCache()
            {
                lock (m_load_from_xml_lock)
                {
                    Catalog.SaveSerieChapters(m_serie);
                    m_list = null;
                    m_loaded_from_xml = false;
                }
            }
        }
        #endregion

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Object m_lock = new Object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SerieState m_state;

        private CachedList<Chapter> m_chapters;

        public Server Server { get; protected set; }
        public string Title { get; protected set; }
        public int DownloadProgress { get; protected set; }

        internal Serie(Server a_server, string a_url, string a_title)
            : this(a_server, a_url, a_title, Catalog.NextID())
        {
        }

        internal Serie(Server a_server, string a_url, string a_title, ulong a_id)
            : base(a_id)
        {
            m_chapters = new ChaptersCachedList(this);
            URL = HttpUtility.HtmlDecode(a_url);
            Server = a_server;

            a_title = a_title.Trim();
            a_title = a_title.Replace("\t", " ");
            while (a_title.IndexOf("  ") != -1)
                a_title = a_title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(a_title);
        }

        public IList<Chapter> Chapters
        {
            get
            {
                return m_chapters;
            }
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
                    DownloadProgress = progress;
                    bool merge = m_chapters.LoadedFromXml && (progress == 100);
                    m_chapters.ReplaceInnerCollection(result, merge, s => s.URL + s.Title);
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

        internal void Save()
        {
            if (!m_chapters.Changed)
                return;

            Catalog.SaveSerieChapters(this);

            m_chapters.Changed = false;

            foreach (var chapter in Chapters)
                chapter.Save();
        }

        protected internal override void RemoveOrphan()
        {
            foreach (var c in Chapters)
                c.RemoveOrphan();

            Catalog.DeleteCatalogFile(ID);
        }
    }
}
