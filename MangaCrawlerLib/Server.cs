using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using TomanuExtensions;
using System.Threading;
using MangaCrawlerLib.Crawlers;
using NHibernate.Mapping.ByCode;
using System.Collections.ObjectModel;

namespace MangaCrawlerLib
{
    public class Server : IClassMapping
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Crawler m_crawler;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ServerState m_state = ServerState.Initial;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CustomTaskScheduler m_scheduler;

        public virtual int ID { get; protected internal set; }
        public virtual string URL { get; protected internal set; }
        public virtual string Name { get; protected internal set; }
        public virtual int DownloadProgress { get; protected internal set; }
        public virtual DateTime LastChange { get; protected internal set; }
        protected internal virtual List<Serie> Series { get; set; }

        protected internal Server()
        {
        }

        protected internal Server(string a_url, string a_name)
        {
            ID = IDGenerator.Next();
            Series = new List<Serie>();
            URL = a_url;
            Name = a_name;
            LastChange = DateTime.Now;
        }

        public virtual void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<Server>(m =>
            {
                m.Lazy(true);
                m.Id(c => c.ID);
                m.Property(c => c.LastChange);
                m.Property(c => c.URL);
                m.Property(c => c.DownloadProgress);
                m.Property(c => c.Name);
                m.Property(c => c.State);
                //m.Property(c => c.Series);
            });
        }

        public virtual ReadOnlyCollection<Serie> GetSeries()
        {
            return Series.AsReadOnly();
        }

        public virtual ServerState State
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

        protected internal virtual CustomTaskScheduler Scheduler 
        {
            get
            {
                if (m_scheduler == null)
                    m_scheduler = SchedulerList.Get(this);

                return m_scheduler;
            }
        }

        protected internal virtual Crawler Crawler
        {
            get
            {
                if (m_crawler == null)
                    m_crawler = CrawlerList.Get(this);

                return m_crawler;
            }
        }

        protected internal virtual void DownloadSeries()
        {
            try
            {
                DownloadProgress = 0;

                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    var series = result.ToList();

                    foreach (var serie in Series)
                    {
                        var el = series.Find(s => (s.Title == serie.Title) && (s.URL == serie.URL));
                        if (el != null)
                            series[series.IndexOf(el)] = serie;
                    }

                    Series = series;
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

        protected internal virtual bool DownloadRequired
        {
            get
            {
                var s = State;
                return (s == ServerState.Error) || (s == ServerState.Initial);
            }
        }
    }
}
