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
using NHibernate.Type;
using NHibernate;

namespace MangaCrawlerLib
{
    public class Server : IClassMapping
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Crawler m_crawler;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CustomTaskScheduler m_scheduler;

        public virtual int ID { get; protected set; }
        public virtual ServerState State { get;  protected set; }
        protected virtual int Version { get; set; }
        public virtual int DownloadProgress { get; protected set; }
        protected virtual IList<Serie> Series { get; set; }
        public virtual string URL { get;  protected set; }
        public virtual string Name { get;  protected set; }
        public virtual int SeriesCount { get; protected set; }

        protected Server()
        {
        }

        internal Server(string a_url, string a_name)
        {
            URL = a_url;
            Name = a_name;
            Series = new List<Serie>();
        }

        private void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<Server>(m =>
            {
                m.Id(c => c.ID, mapping => mapping.Generator(Generators.Native));
                m.Version("Version", mapping => { });
                m.Property(c => c.URL, mapping => mapping.NotNullable(true));
                m.Property(c => c.Name, mapping => mapping.NotNullable(true));
                m.Property(c => c.DownloadProgress, mapping => mapping.NotNullable(true));
                m.Property(c => c.State, mapping => mapping.NotNullable(true));
                m.Property(c => c.SeriesCount, mapping => mapping.NotNullable(true));

                m.List<Serie>(
                    "Series",
                    list_mapping => list_mapping.Cascade(Cascade.All), 
                    mapping => mapping.OneToMany()
                );
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
                // TODO: na 100% usun nie istniejace
                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    NH.TransactionLockUpdate(this, () => 
                    {
                        int index = 0;
                        foreach (var serie in result)
                        {
                            if (Series.Count <= index)
                                Series.Insert(index, serie);
                            else
                            {
                                var s = Series[index];
                                
                                if ((s.Title != serie.Title) || (s.URL != serie.URL))
                                    Series.Insert(index, serie);
                            }
                            index++;
                        }

                        SeriesCount = index;
                        DownloadProgress = progress;
                    });
                });

                NH.TransactionLockUpdate(this, () => State = ServerState.Downloaded);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                NH.TransactionLockUpdate(this, () => State = ServerState.Error);
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

        protected internal virtual void ResetState()
        {
            State = ServerState.Initial;
        }

        protected internal virtual bool BeginWaiting()
        {
            if (!DownloadRequired)
                return false;
            State = ServerState.Waiting;
            return true;
        }

        protected internal virtual void DownloadingStarted()
        {
            State = ServerState.Downloading;
            DownloadProgress = 0;
        }
    }
}
