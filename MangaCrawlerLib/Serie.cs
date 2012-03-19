using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
using TomanuExtensions.Utils;
using TomanuExtensions;

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
                lock (m_lock)
                {
                    if (m_list != null)
                        return;

                    m_list = Catalog.LoadSerieChapters(m_serie);

                    if (m_list.Count != 0)
                        m_loaded_from_xml = true;
                }
            }
        }
        #endregion

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Object m_lock = new Object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SerieState m_state;

        private CachedList<Chapter> m_chapters;
        private DateTime m_check_date_time = DateTime.MinValue;

        public Server Server { get; protected set; }
        public string Title { get; protected set; }
        public int DownloadProgress { get; protected set; }

        internal Serie(Server a_server, string a_url, string a_title)
            : this(a_server, a_url, a_title, Catalog.NextID(), SerieState.Initial)
        {
        }

        internal Serie(Server a_server, string a_url, string a_title, ulong a_id, SerieState a_state)
            : base(a_id)
        {
            m_chapters = new ChaptersCachedList(this);
            URL = HttpUtility.HtmlDecode(a_url);
            Server = a_server;
            m_state = a_state;

            if (m_state == SerieState.Downloading)
                m_state = SerieState.Initial;
            if (m_state == SerieState.Waiting)
                m_state = SerieState.Initial;

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

        internal void ResetCheckDate()
        {
            m_check_date_time = DateTime.MinValue;
        }

        internal void DownloadChapters()
        {
            try
            {
                Crawler.DownloadChapters(this, (progress, result) =>
                {
                    if (!m_chapters.LoadedFromXml)
                        m_chapters.ReplaceInnerCollection(result);
                    else if (progress == 100)
                        m_chapters.ReplaceInnerCollection(result, (c) => c.Title + c.URL);
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

            Catalog.Save(this);
            m_check_date_time = DateTime.Now;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Server.Name, Title);
        }

        public bool DownloadRequired
        {
            get
            {
                if (State == SerieState.Downloaded)
                {
                    if (DateTime.Now - m_check_date_time > DownloadManager.Instance.MangaSettings.CheckTimeDelta)
                        return true;
                    else
                        return false;
                }
                else
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

        public override string GetDirectory()
        {
            string manga_root_dir = DownloadManager.Instance.MangaSettings.GetMangaRootDir(true); ;

            return manga_root_dir +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileCharacters(Server.Name) +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileCharacters(Title) +
                   Path.DirectorySeparatorChar;
        }

        public override bool IsDownloading
        {
            get
            {
                return (State == SerieState.Downloading) ||
                       (State == SerieState.Waiting);
            }
        }

        public bool IsBookmarked
        {
            get
            {
                return DownloadManager.Instance.Bookmarks.List.Contains(this);
            }
        }
    }
}
