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
                lock (m_load_from_xml_lock)
                {
                    if (m_list != null)
                        return;

                    m_list = Catalog.LoadServerSeries(m_server);

                    if (m_list.Count != 0)
                        m_loaded_from_xml = true;
                }
            }

            internal override void ClearCache()
            {
                lock (m_load_from_xml_lock)
                {
                    Catalog.SaveServerSeries(m_server);
                    m_list = null;
                    m_loaded_from_xml = false;
                }
            }
        }
        #endregion

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Crawler m_crawler;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ServerState m_state;

        private CachedList<Serie> m_series;

        public int DownloadProgress { get; private set; }
        public string Name { get; private set; }

        internal Server(string a_url, string a_name)
            : this(a_url, a_name, Catalog.NextID())
        {
        }

        internal Server(string a_url, string a_name, ulong a_id)
            : base(a_id)
        {
            m_series = new SeriesCachedList(this);
            URL = a_url;
            Name = a_name;
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

        internal void DownloadSeries()
        {
            try
            {
                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    DownloadProgress = progress;

                    if (m_series.LoadedFromXml)
                    {
                        if (progress == 100)
                        {
                            var merged = Entity.MergeAndRemoveOrphans(m_series, result, s => s.URL + s.Title);
                            m_series.ReplaceInnerCollection(merged);
                        }
                    }
                    else
                        m_series.ReplaceInnerCollection(result.ToList());
                });

                State = ServerState.Downloaded;
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                State = ServerState.Error;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public bool DownloadRequired
        {
            get
            {
                return (State == ServerState.Error) || (State == ServerState.Initial);
            }
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
                        Debug.Assert((State == ServerState.Downloaded) ||
                                     (State == ServerState.Initial) ||
                                     (State == ServerState.Error));
                        break;
                    }
                    case ServerState.Downloading:
                    {
                        Debug.Assert(State == ServerState.Waiting);
                        DownloadProgress = 0;
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

        internal void Save()
        {
            if (!m_series.Changed)
                return;

            Catalog.SaveServerSeries(this);

            m_series.Changed = false;

            foreach (var serie in Series)
                serie.Save();
        }

        protected internal override void RemoveOrphan()
        {
            foreach (var s in Series)
                s.RemoveOrphan();

            Catalog.DeleteCatalogFile(ID);
        }
    }
}
