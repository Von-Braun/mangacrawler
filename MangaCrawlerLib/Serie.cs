using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using NHibernate.Mapping.ByCode;
using System.Collections.ObjectModel;
using System.Threading;

namespace MangaCrawlerLib
{
    public class Serie : IClassMapping
    {
        public virtual int ID { get; protected set; }
        public virtual DateTime LastChange { get; protected set; }
        public virtual SerieState State { get; protected set; }
        protected virtual IList<Chapter> Chapters { get; set; }
        public virtual string URL { get; protected set; }
        public virtual Server Server { get; protected set; }
        public virtual string Title { get; protected set; }
        public virtual int DownloadProgress { get; protected set; }

        protected Serie()
        {
        }

        internal Serie(Server a_server, string a_url, string a_title)
        {
            URL = HttpUtility.HtmlDecode(a_url);
            Server = a_server;
            ID = IDGenerator.Next();
            Chapters = new List<Chapter>();

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
                m.Version(c => c.LastChange, mapping => { });
                m.Property(c => c.URL, mapping => mapping.NotNullable(true));
                m.Property(c => c.Title, mapping => mapping.NotNullable(true));
                m.Property(c => c.DownloadProgress, mapping => mapping.NotNullable(true));
                m.Property(c => c.State, mapping => mapping.NotNullable(true));
                m.List<Chapter>("Chapters", list_mapping => list_mapping.Inverse(true), mapping => mapping.OneToMany());
                m.ManyToOne(c => c.Server, mapping => mapping.NotNullable(true));
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

        public virtual IEnumerable<Chapter> GetChapters()
        {
            return Chapters;
        }

        protected internal virtual void DownloadChapters()
        {
            try
            {
                DownloadProgress = 0;

                Crawler.DownloadChapters(this, (progress, result) =>
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

        private bool DownloadRequired
        {
            get
            {
                var s = State;
                return (s == SerieState.Error) || (s == SerieState.Initial);
            }
        }

        protected internal virtual bool BeginDownloading()
        {
            if (!DownloadRequired)
                return false;
            State = SerieState.Waiting;
            return false;
        }
    }
}
