using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Threading;
using System.Diagnostics;
using TomanuExtensions;
using TomanuExtensions.Utils;
using NHibernate.Mapping.ByCode;
using System.Collections.ObjectModel;

namespace MangaCrawlerLib
{
    public class Chapter //: IClassMapping
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ChapterState m_state = ChapterState.Initial;

        internal virtual ChapterWork Work { get; private set; }
        public virtual string URL { get; private set; }
        public virtual Serie Serie { get; private set; }
        public virtual string Title { get; private set; }
        public virtual DateTime LastChange { get; internal set; }
        public virtual int ID { get; private set; }
        internal virtual List<Page> Pages { get; set; }

        private Chapter()
        {
        }

        internal Chapter(Serie a_serie, string a_url, string a_title)
        {
            Pages = new List<Page>();
            ID = IDGenerator.Next();
            Serie = a_serie;
            URL = HttpUtility.HtmlDecode(a_url);
            LastChange = DateTime.Now;

            Title = a_title.Trim();
            Title = Title.Replace("\t", " ");
            while (Title.IndexOf("  ") != -1)
                Title = Title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(Title);
        }

        public void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<Chapter>(m =>
            {
                m.Lazy(true);
                m.Id(c => c.ID, p =>
                {
                    p.Generator(Generators.HighLow);
                });
                m.Property(c => c.URL, p =>
                {
                    p.NotNullable(true);
                });
                m.Property(c => c.Serie, p =>
                {
                    p.NotNullable(true);
                });
                m.Property(c => c.Title, p =>
                {
                    p.NotNullable(true);
                });
                m.Property(c => c.Work, p =>
                {
                    p.NotNullable(false);
                });
                m.Version(c => c.LastChange, p =>
                {
                });
                m.Property(c => c.LastChange, p =>
                {
                    p.NotNullable(true);
                });
                m.Property(c => c.State, p =>
                {
                    p.NotNullable(true);
                });
                m.Property(c => c.Pages, p =>
                {
                    p.NotNullable(true);
                });
            });
        }

        public virtual ChapterState State
        {
            get
            {
                return m_state;
            }
            internal set
            {
                m_state = value;
                LastChange = DateTime.Now;
            }
        }

        public void AddPages(IEnumerable<Page> a_pages)
        {
            Pages.AddRange(a_pages);
            LastChange = DateTime.Now;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Serie, Title);
        }

        public ReadOnlyCollection<Page> GetPages()
        {
            return Pages.AsReadOnly();
        }

        internal bool DownloadRequired
        {
            get
            {
                return (State == ChapterState.Error) ||
                       (State == ChapterState.Initial) &&
                       (State == ChapterState.Aborted);
            }
        }

        public int DownloadedPages
        {
            get
            {
                return Pages.Count(p => p.Downloaded);
            }
        }

        public void DeleteWork()
        {
            ChapterWork work = Work;
            if (work == null)
                return;
            work.DeleteWork();
        }

        internal void CreateWork(string a_manga_root_dir, bool a_cbz)
        {
            Work = new ChapterWork(this, a_manga_root_dir, a_cbz);
        }
    }
}
