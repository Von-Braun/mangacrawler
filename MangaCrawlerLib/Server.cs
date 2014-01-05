using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using TomanuExtensions;
using System.Threading;
using MangaCrawlerLib.Crawlers;
using System.Collections.ObjectModel;
using System.IO;
using TomanuExtensions.Utils;

namespace MangaCrawlerLib
{
    public class Server : Entity
    {
        #region SeriesCachedList
        private class SeriesCachedList : CachedList<Serie>
        {
            private Server m_server;

            public SeriesCachedList(Server a_server)
            {
                m_server = a_server;
            }

            protected override void EnsureLoaded()
            {
                lock (m_lock)
                {
                    if (m_list != null)
                        return;

                    m_list = Catalog.LoadServerSeries(m_server);
                }
            }
        }
        #endregion

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Crawler m_crawler;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ServerState m_state;

        private CachedList<Serie> m_series;
        private DateTime m_check_date_time = DateTime.MinValue;

        public int DownloadProgress { get; private set; }
        public string Name { get; private set; }
        protected internal bool SeriesDownloadedFirstTime { get; protected set; }

        internal Server(string a_url, string a_name)
            : this(a_url, a_name, Catalog.NextID(), ServerState.Initial, false)
        {
        }

        internal Server(string a_url, string a_name, ulong a_id, ServerState a_state, 
            bool a_series_downloaded_first_time)
            : base(a_id)
        {
            m_series = new SeriesCachedList(this);
            URL = a_url;
            Name = a_name;
            m_state = a_state;
            SeriesDownloadedFirstTime = a_series_downloaded_first_time;

            if (m_state == ServerState.Downloading)
                m_state = ServerState.Initial;
            if (m_state == ServerState.Waiting)
                m_state = ServerState.Initial;
            if (m_state == ServerState.Downloaded)
                m_state = ServerState.Initial;
        }

        internal override Crawler Crawler
        {
            get
            {
                if (m_crawler == null)
                    m_crawler = CrawlerList.Get(this);

                return m_crawler;
            }
        }

        /// <summary>
        /// Thread safe.
        /// </summary>
        public IList<Serie> Series 
        {
            get
            {
                return m_series;
            }
        }

        internal void ResetCheckDate()
        {
            m_check_date_time = DateTime.MinValue;

            if (m_series.Filled)
            {
                foreach (var serie in Series)
                    serie.ResetCheckDate();
            }
        }

        internal void DownloadSeries()
        {
            Object locker = new Object();

            try
            {
                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    lock (locker)
                    {
                        Merge<Serie> merge = (cats, news) =>
                        {
                            cats.URL = news.URL;
                        };

                        if (progress == 100)
                        {
                            result = EliminateDoubles(result.ToList());
                            m_series.ReplaceInnerCollection(result, SeriesDownloadedFirstTime, s => s.Title, merge);
                        }

                        DownloadProgress = progress;
                    }
                });

                DownloadManager.Instance.Bookmarks.RemoveNotExisted();
                State = ServerState.Downloaded;
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error(String.Format(
                    "Exception, server: {0} state: {1}", this, State), ex);
                State = ServerState.Error;
            }

            SeriesDownloadedFirstTime = true;
            Catalog.Save(this);
            m_check_date_time = DateTime.Now;
        }

        private static List<Serie> EliminateDoubles(List<Serie> a_series)
        {
            var same_name_same_url = (from serie in a_series
                                      group serie by new { serie.Title, serie.URL } into gr
                                      from s in gr.Skip(1)
                                      select s).ToList();

            if (same_name_same_url.Any())
                a_series = a_series.Except(same_name_same_url).ToList();

            var same_name_diff_url = from serie in a_series
                                     group serie by serie.Title into gr
                                     where gr.Count() > 1
                                     select gr;

            foreach (var gr in same_name_diff_url)
            {
                int index = 1;

                foreach (var serie in gr)
                {
                    for (; ; )
                    {
                        string new_title = String.Format("{0} ({1})", serie.Title, index);
                        index++;

                        if (a_series.Any(ch => ch.Title == new_title))
                            continue;

                        serie.Title = new_title;
                        break;
                    }
                }
            }

            return a_series;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", ID, Name);
        }

        public bool IsDownloadRequired(bool a_force)
        {
            if (State == ServerState.Downloaded)
            {
                if (!a_force)
                {
                    if (DateTime.Now - m_check_date_time > DownloadManager.Instance.MangaSettings.CheckTimePeriod)
                        return true;
                    else
                        return false;
                }
                else
                    return true;
            }
            else
                return (State == ServerState.Error) || (State == ServerState.Initial);
        }

        public override string GetDirectory()
        {
            string manga_root_dir = DownloadManager.Instance.MangaSettings.GetMangaRootDir(true); ;

            return manga_root_dir +
                   Path.DirectorySeparatorChar +
                   FileUtils.RemoveInvalidFileCharacters(Name) +
                   Path.DirectorySeparatorChar;
        }

        public ServerState State
        {
            get
            {
                return m_state;
            }
            set
            {
                switch (value)
                {
                    case ServerState.Initial:
                    {
                        break;
                    }
                    case ServerState.Waiting:
                    {
                        Debug.Assert((State == ServerState.Initial) ||
                                     (State == ServerState.Error) || 
                                     (State == ServerState.Downloaded));
                        DownloadProgress = 0;
                        break;
                    }
                    case ServerState.Downloading:
                    {
                        Debug.Assert((State == ServerState.Waiting) ||
                                     (State == ServerState.Downloading));
                        break;
                    }
                    case ServerState.Downloaded:
                    {
                        Debug.Assert(State == ServerState.Downloading);
                        Debug.Assert(DownloadProgress == 100);
                        break;
                    }
                    case ServerState.Error:
                    {
                        Debug.Assert(State == ServerState.Downloading);
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

        public override bool IsDownloading
        {
            get
            {
                return (State == ServerState.Downloading) ||
                       (State == ServerState.Waiting);
            }
        }
    }
}
