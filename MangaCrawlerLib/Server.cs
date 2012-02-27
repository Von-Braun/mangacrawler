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
        private CustomTaskScheduler m_scheduler;

        public virtual int ID { get; protected set; }
        public virtual ServerState State { get; protected set; }
        public virtual DateTime LastChange { get; protected set; }
        public virtual int DownloadProgress { get; protected set; }
        protected virtual IList<Serie> Series { get; set; }
        public virtual string URL { get; protected set; }
        public virtual string Name { get; protected set; }

        protected Server()
        {
        }

        internal Server(string a_url, string a_name)
        {
            ID = IDGenerator.Next();
            URL = a_url;
            Name = a_name;
            Series = new List<Serie>();
        }

        private void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<Server>(m =>
            {
                m.Id(c => c.ID, mapping => mapping.Generator(Generators.Native));
                m.Version(c => c.LastChange, mapping => { });
                m.Property(c => c.URL, mapping => mapping.NotNullable(true));
                m.Property(c => c.Name, mapping => mapping.NotNullable(true));
                m.Property(c => c.DownloadProgress, mapping => mapping.NotNullable(true));
                m.Property(c => c.State, mapping => mapping.NotNullable(true));
                m.List<Serie>("Series", list_mapping => list_mapping.Inverse(true), mapping => mapping.OneToMany());
            });
        }

        public virtual IEnumerable<Serie> GetSeries()
        {
            return Series;
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

        private bool DownloadRequired
        {
            get
            {
                var s = State;
                return (s == ServerState.Error) || (s == ServerState.Initial);
            }
        }

        protected internal virtual bool BeginDownloading()
        {
            if (!DownloadRequired)
                return false;
            State = ServerState.Waiting;
            return false;
        }
    }
}
