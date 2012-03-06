using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using NHibernate.Mapping.ByCode;
using System.Collections.ObjectModel;
using System.Threading;
using NHibernate;

namespace MangaCrawlerLib
{
    public class Serie : IClassMapping
    {
        public virtual int ID { get; protected set; }
        protected virtual int Version { get; set; }
        public virtual SerieState State { get; protected set; }
        public virtual string URL { get; protected set; }
        public virtual Server Server { get; protected set; }
        public virtual string Title { get; protected set; }
        public virtual int DownloadProgress { get; protected set; }
        protected internal virtual IList<Chapter> Chapters { get; protected set; }

        protected Serie()
        {
            Chapters = new List<Chapter>();
        }

        internal Serie(Server a_server, string a_url, string a_title)
            : this()
        {
            URL = HttpUtility.HtmlDecode(a_url);
            Server = a_server;

            a_title = a_title.Trim();
            a_title = a_title.Replace("\t", " ");
            while (a_title.IndexOf("  ") != -1)
                a_title = a_title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(a_title);
        }

        private void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<Serie>(m =>
            {
                m.Id(c => c.ID, mapping => mapping.Generator(Generators.Native));
                m.Version("Version", mapping => { });
                m.Property(c => c.URL, mapping => mapping.NotNullable(true));
                m.Property(c => c.Title, mapping => mapping.NotNullable(true));
                m.Property(c => c.DownloadProgress, mapping => mapping.NotNullable(true));
                m.Property(c => c.State, mapping => mapping.NotNullable(true));

                m.List<Chapter>(
                    "Chapters", 
                    list_mapping => 
                    {
                        list_mapping.Cascade(Cascade.All); 
                    }, 
                    mapping => mapping.OneToMany()
                );

                m.ManyToOne(
                    c => c.Server, 
                    mapping => 
                    {
                        mapping.Fetch(FetchKind.Join);
                        mapping.NotNullable(false); 
                    }
                );
            });
        }

        protected internal virtual CustomTaskScheduler Scheduler
        {
            get
            {
                return Server.Scheduler;
            }
        }

        protected internal virtual Crawler Crawler
        {
            get
            {
                return Server.Crawler;
            }
        }

        public virtual IEnumerable <Chapter> GetChapters()
        {
            return Chapters;
        }

        protected internal virtual void DownloadChapters()
        {
            try
            {
                Crawler.DownloadChapters(this, (progress, result) =>
                {
                    NH.TransactionLockUpdate(this, () =>
                    {
                        bool added;
                        IList<Chapter> removed;
                        DownloadManager.Sync(result, Chapters, chapter => (chapter.Title + chapter.URL),
                            progress == 100, out added, out removed);

                        DownloadProgress = progress;
                    });
                });

                NH.TransactionLockUpdate(this, () => SetState(SerieState.Downloaded));

            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                NH.TransactionLockUpdate(this, () => SetState(SerieState.Downloaded));
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Server.Name, Title);
        }

        public virtual bool DownloadRequired
        {
            get
            {
                return (State == SerieState.Error) || (State == SerieState.Initial);
            }
        }

        public virtual void SetState(SerieState a_state)
        {
            switch (a_state)
            {
                case SerieState.Initial:
                {
                    break;
                }
                case SerieState.Waiting:
                {
                    Debug.Assert((State == SerieState.Downloaded) ||
                                 (State == SerieState.Initial) ||
                                 (State == SerieState.Error));
                    break;
                }
                case SerieState.Downloading:
                {
                    Debug.Assert(State == SerieState.Waiting);
                    DownloadProgress = 0;
                    break;
                }
                case SerieState.Downloaded:
                {
                    Debug.Assert(State == SerieState.Downloading);
                    Debug.Assert(DownloadProgress == 100);
                    break;
                }
                case SerieState.Error:
                {
                    Debug.Assert(State == SerieState.Downloading);
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
