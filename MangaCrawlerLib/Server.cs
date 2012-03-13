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
                lock (m_load_from_xml_lock)
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

            if (m_state == ServerState.Checking)
                m_state = ServerState.Initial;
            if (m_state == ServerState.Waiting)
                m_state = ServerState.Initial;
            if (m_state == ServerState.Checked)
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

        internal void DownloadSeries()
        {
            try
            {
                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    DownloadProgress = progress;
                    bool merge = m_series.LoadedFromXml && (progress == 100);
                    m_series.ReplaceInnerCollection(result, merge, s => s.URL + s.Title);
                });

                State = ServerState.Checked;
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                State = ServerState.Error;
            }

            Catalog.Save(this);
            m_check_date_time = DateTime.Now;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool DownloadRequired
        {
            get
            {
                if (State == ServerState.Checked)
                {
                    if (DateTime.Now - m_check_date_time > DownloadManager.GetCheckTimeDelta())
                        return true;
                    else
                        return false;
                }
                else
                    return (State == ServerState.Error) || (State == ServerState.Initial);
            }
        }

        public string GetServerDirectory()
        {
            string manga_root_dir = DownloadManager.GetMangaRootDir();

            if (manga_root_dir.Last() == Path.DirectorySeparatorChar)
                manga_root_dir = manga_root_dir.RemoveFromRight(1);

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
                                     (State == ServerState.Checked));
                        break;
                    }
                    case ServerState.Checking:
                    {
                        Debug.Assert(State == ServerState.Waiting);
                        DownloadProgress = 0;
                        break;
                    }
                    case ServerState.Checked:
                    {
                        Debug.Assert(State == ServerState.Checking);
                        Debug.Assert(DownloadProgress == 100);
                        break;
                    }
                    case ServerState.Error:
                    {
                        Debug.Assert(State == ServerState.Checking);
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
