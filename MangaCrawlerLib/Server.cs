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
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Object m_lock = new Object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Crawler m_crawler;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ServerState m_state;

        public int DownloadProgress { get; protected set; }
        public string Name { get;  protected set; }
        public List<Serie> Series { get; protected set; }

        internal Server(string a_url, string a_name)
        {
            Series = new List<Serie>();
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

        internal void DownloadSeries()
        {
            try
            {
                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    lock (m_lock)
                    {
                        IList<Serie> removed;
                        IList<Serie> added;
                        List<Serie> series = Series.ToList();
                        Sync(result, series, serie => (serie.Title + serie.URL), progress == 100,
                            out added, out removed);
                        Series = series;
                    }

                    DownloadProgress = progress;
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
    }
}
