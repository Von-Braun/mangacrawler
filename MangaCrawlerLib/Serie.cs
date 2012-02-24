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

        public virtual int ID { get; protected internal set; }
        public virtual DateTime LastChange { get; protected internal set; }
        public virtual string URL { get; protected internal set; }
        public virtual Server Server { get; protected internal set; }
        public virtual int DownloadProgress { get; protected internal set; }
        public virtual string Title { get; protected internal set; }
        protected internal virtual List<Chapter> Chapters { get; set; }

        protected internal Serie()
        {
        }

        internal Serie(Server a_server, string a_url, string a_title)
        {
            ID = IDGenerator.Next();
            URL = HttpUtility.HtmlDecode(a_url);
            Server = a_server;
            Chapters = new List<Chapter>();
            LastChange = DateTime.Now;

            Title = a_title.Trim();
            Title = Title.Replace("\t", " ");
            while (Title.IndexOf("  ") != -1)
                Title = Title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(Title);
        }

        public virtual void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<Serie>(m =>
            {
                m.Lazy(true);
                m.Id(c => c.ID);
                m.Property(c => c.LastChange);
                m.Property(c => c.URL);
                //m.Property(c => c.Server);
                m.Property(c => c.DownloadProgress);
                m.Property(c => c.Title);
                m.Property(c => c.State);
                //m.Property(c => c.Chapters);
            });
        }

        public virtual SerieState State
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

        public virtual ReadOnlyCollection<Chapter> GetChapters()
        {
            return Chapters.AsReadOnly();
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

                    Chapters = chapters;
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
