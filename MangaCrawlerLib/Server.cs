using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using TomanuExtensions;
using System.Threading;
using MangaCrawlerLib.Crawlers;

namespace MangaCrawlerLib
{
    public class Server
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Serie> m_series = new List<Serie>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Crawler m_crawler;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ServerState m_state = ServerState.Initial;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CustomTaskScheduler m_scheduler;

        public string URL { get; private set; }
        public string Name { get; private set; }
        public int DownloadProgress { get; private set; }
        public DateTime LastChange { get; private set; }

        internal Server(string a_url, string a_name)
        {
            URL = a_url;
            Name = a_name;
            LastChange = DateTime.Now;
        }

        public ServerState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
                LastChange = DateTime.Now;
            }
        }
        internal CustomTaskScheduler Scheduler 
        {
            get
            {
                if (m_scheduler == null)
                    m_scheduler = SchedulerList.Get(this);

                return m_scheduler;
            }
        }

        internal Crawler Crawler
        {
            get
            {
                if (m_crawler == null)
                    m_crawler = CrawlerList.Get(this);

                return m_crawler;
            }
        }

        public IEnumerable<Serie> Series
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
                DownloadProgress = 0;

                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    var series = result.ToList();

                    foreach (var serie in m_series)
                    {
                        var el = series.Find(s => (s.Title == serie.Title) && (s.URL == serie.URL));
                        if (el != null)
                            series[series.IndexOf(el)] = serie;
                    }

                    m_series = series;
                    DownloadProgress = progress;
                    LastChange = DateTime.Now;
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

        internal bool DownloadRequired
        {
            get
            {
                var s = State;
                return (s == ServerState.Error) || (s == ServerState.Initial);
            }
        }
    }
}
