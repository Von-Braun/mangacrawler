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

                    if (m_list.Count != 0)
                        m_loaded_from_xml = true;
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

        internal Server(string a_url, string a_name)
            : this(a_url, a_name, Catalog.NextID(), ServerState.Initial)
        {
        }

        internal Server(string a_url, string a_name, ulong a_id, ServerState a_state)
            : base(a_id)
        {
            m_series = new SeriesCachedList(this);
            URL = a_url;
            Name = a_name;
            m_state = a_state;

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
            try
            {
                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    if (!m_series.IsLoadedFromXml)
                    {
                        result = EliminateDoubles(result);
                        m_series.ReplaceInnerCollection(result, false, s => s.Title);
                    }
                    else if (progress == 100)
                    {
                        result = EliminateDoubles(result);
                        m_series.ReplaceInnerCollection(result, true, s => s.Title);
                    }
                    DownloadProgress = progress;
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

            Catalog.Save(this);
            m_check_date_time = DateTime.Now;
        }

        private static IEnumerable<Serie> EliminateDoubles(IEnumerable<Serie> a_series)
        {
            a_series = a_series.ToList();

            var doubles = 
                a_series.Select(s => s.Title).ExceptExact(a_series.Select(s => s.Title).Distinct()).ToArray();

            var same_name_same_url = from serie in a_series
                                     where doubles.Contains(serie.Title)
                                     group serie by new { serie.Title, serie.URL } into gr
                                     from s in gr.Skip(1)
                                     select s;

            a_series = 
                a_series.Except(same_name_same_url.ToList()).ToList();

            doubles = a_series.Select(s => s.Title).ExceptExact(a_series.Select(s => s.Title).Distinct()).ToArray();

            var same_name_diff_url = from serie in a_series
                                     where doubles.Contains(serie.Title)
                                     group serie by serie.Title;

            foreach (var gr in same_name_diff_url)
            {
                int index = 1;

                foreach (var serie in gr)
                {
                    serie.Title += String.Format(" ({0})", index);
                    index++;
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

        internal void Debug_LoadAllFromCatalog(ref int a_servers, ref int a_series, ref int a_chapters, ref int a_pages)
        {
            m_series.Count.ToString();
            if (m_series.IsLoadedFromXml)
                a_servers++;
            foreach (var serie in m_series)
                serie.Debug_LoadAllFromCatalog(ref a_servers, ref a_series, ref a_chapters, ref a_pages);
        }
    }
}
