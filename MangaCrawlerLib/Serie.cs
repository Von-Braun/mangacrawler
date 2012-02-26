using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using NHibernate.Mapping.ByCode;
using System.Collections.ObjectModel;

namespace MangaCrawlerLib
{
    public class Serie : IClassMapping
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SerieState m_state = SerieState.Initial;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IList<Chapter> m_chapters = new List<Chapter>();

        public virtual int ID { get; protected internal set; }
        public virtual DateTime LastChange { get; protected internal set; }
        public virtual string URL { get; protected internal set; }
        public virtual Server Server { get; protected internal set; }
        public virtual int DownloadProgress { get; protected internal set; }
        public virtual string Title { get; protected internal set; }

        protected internal Serie()
        {
        }

        internal Serie(Server a_server, string a_url, string a_title)
        {
            ID = IDGenerator.Next();
            URL = HttpUtility.HtmlDecode(a_url);
            Server = a_server;

            Title = a_title.Trim();
            Title = Title.Replace("\t", " ");
            while (Title.IndexOf("  ") != -1)
                Title = Title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(Title);
        }

        private void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<Serie>(m =>
            {
                m.Id(c => c.ID, mapping => mapping.Generator(Generators.Native));
                m.Version(c => c.LastChange, mapping => { });
                m.Property(c => c.URL, mapping => mapping.NotNullable(true));
                m.Property(c => c.DownloadProgress);
                m.Property(c => c.Title, mapping => mapping.NotNullable(true));
                m.Property(c => c.State, mapping => mapping.NotNullable(true));
                m.List<Chapter>("m_chapters", list_mapping => list_mapping.Inverse(true), mapping => mapping.OneToMany());
                m.ManyToOne(c => c.Server, mapping => mapping.NotNullable(true));
            });
        }

        public virtual SerieState State
        {
            get
            {
                return m_state;
            }
            set // TODO: private
            {
                m_state = value;
                LastChange = DateTime.Now;
            }
        }

        public virtual IEnumerable<Chapter> Chapters
        {
            get
            {
                return m_chapters;
            }
        }

        protected internal virtual void DownloadChapters()
        {
            try
            {
                DownloadProgress = 0;

                Server.Crawler.DownloadChapters(this, (progress, result) =>
                {
                    var chapters = result.ToList();

                    foreach (var chapter in Chapters)
                    {
                        var el = chapters.Find(s => (s.Title == chapter.Title) && (s.URL == chapter.URL));
                        if (el != null)
                            chapters[chapters.IndexOf(el)] = chapter;
                    }

                    m_chapters = chapters;
                    DownloadProgress = progress;
                    LastChange = DateTime.Now;
                });

                State = SerieState.Downloaded;

            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                State = SerieState.Error;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Server.Name, Title);
        }

        protected internal virtual bool DownloadRequired
        {
            get
            {
                var s = State;
                return (s == SerieState.Error) || (s == SerieState.Initial);
            }
        }
    }
}
