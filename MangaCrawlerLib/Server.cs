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
    public class Server : Entity
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Crawler m_crawler;

        private IList<Serie> m_series;

        public virtual ServerState State { get;  protected set; }
        public virtual int DownloadProgress { get; protected set; }
        public virtual string Name { get;  protected set; }
        protected internal virtual CacheList<Serie> CacheSeries { get; protected set; }

        protected Server()
        {
            Series = new List<Serie>();
        }

        internal Server(string a_url, string a_name)
            : this()
        {
            URL = a_url;
            Name = a_name;
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

                m.List<Serie>(
                    "Series",
                    list_mapping => 
                    {
                        list_mapping.Cascade(Cascade.All);
                    },
                    mapping => mapping.OneToMany()
                );
            });
        }

        protected internal virtual IList<Serie> Series
        {
            get
            {
                return m_series;
            }

            protected set
            {
                m_series = value;
                CacheSeries = new CacheList<Serie>(this, value);

            }
        }

        public virtual IEnumerable<Serie> GetSeries()
        {
            return CacheSeries;
        }

        protected internal override Crawler Crawler
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
                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    NH.TransactionLockUpdate(this, () =>
                    {
                        IList<Serie> removed;
                        IList<Serie> added;
                        CacheSeries.Sync(result, serie => (serie.Title + serie.URL), progress == 100,
                            out added, out removed);
                        DownloadProgress = progress;
                    });
                });

                NH.TransactionLockUpdate(this, () => SetState(ServerState.Downloaded));
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                NH.TransactionLockUpdate(this, () => SetState(ServerState.Error));
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual bool DownloadRequired
        {
            get
            {
                return (State == ServerState.Error) || (State == ServerState.Initial);
            }
        }

        protected internal virtual void SetState(ServerState a_state)
        {
            switch (a_state)
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
                    throw new InvalidOperationException(String.Format("Unknown state: {0}", a_state));
                }
            }

            State = a_state;
        }
    }
}
