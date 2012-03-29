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

            protected override IEnumerable<MergeRule> MergeRules
            {
                get
                {
                    yield return new MergeRule()
                    {
                        KeySelector = s => s.Title,
                        Merge = (src, dest) =>
                        {
                            dest.Title = src.Title;
                            dest.URL = src.URL;
                        }
                    };
                    yield return new MergeRule()
                    {
                        KeySelector = s => s.URL,
                        Merge = (src, dest) =>
                        {
                            dest.Title = src.Title;
                            dest.URL = src.URL;
                        }
                    };
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
        public string Title { get; internal set; }
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
                    if (!m_chapters.IsLoadedFromXml)
                        m_chapters.ReplaceInnerCollection(result, false);
                    else if (progress == 100)
                        m_chapters.ReplaceInnerCollection(result, true);
                    DownloadProgress = progress; 
                });

                State = SerieState.Downloaded;
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex1)
            {
                State = SerieState.Error;

                Loggers.MangaCrawler.Error(
                    String.Format("Exception #1, serie: {0} state: {1}", this, State), ex1);

                try
                {
                    DownloadManager.Instance.DownloadSeries(Server, true);
                }
                catch (Exception ex2)
                {
                    Loggers.MangaCrawler.Error(
                        String.Format("Exception #2, serie: {0} state: {1}", this, State), ex2);
                }
            }

            Catalog.Save(this);
            m_check_date_time = DateTime.Now;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Server.Name, Title);
        }

        public bool IsDownloadRequired(bool a_force)
        {
            if (State == SerieState.Downloaded)
            {
                if (!a_force)
                {
                    if (DateTime.Now - m_check_date_time > DownloadManager.Instance.MangaSettings.CheckTimePeriod)
                        return true;
                    else
                        return false;
                }
                return true;
            }
            else
                return (State == SerieState.Error) || (State == SerieState.Initial);
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
                        Debug.Assert((State == SerieState.Waiting) ||
                                     (State == SerieState.Downloading));
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

        public IEnumerable<Chapter> GetNewChapters()
        {
            return (from chapter in Chapters
                    where !chapter.BookmarkIgnored
                    select chapter).ToList();
        }

        internal void Debug_LoadAllFromCatalog(ref int a_servers, ref int a_series, ref int a_chapters, ref int a_pages)
        {
            a_series++;
            m_chapters.Count.ToString(); 
            foreach (var chapter in m_chapters)
                chapter.Debug_LoadAllFromCatalog(ref a_servers, ref a_series, ref a_chapters, ref a_pages);
        }
    }
}
