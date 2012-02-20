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

namespace MangaCrawlerLib
{
    public class Chapter
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ChapterState m_state = ChapterState.Initial;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Page> m_pages = new List<Page>();

        public Work Work;
        public string URL { get; private set; }
        public Serie Serie { get; private set; }
        public string Title { get; private set; }
        public DateTime LastChange { get; internal set; }
        
        internal Chapter(Serie a_serie, string a_url, string a_title)
        {
            Serie = a_serie;
            URL = HttpUtility.HtmlDecode(a_url);

            Title = a_title.Trim();
            Title = Title.Replace("\t", " ");
            while (Title.IndexOf("  ") != -1)
                Title = Title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(Title);

            LastChange = DateTime.Now;
        }

        public IEnumerable<Page> Pages 
        {
            get
            {
                return m_pages;
            }
        }

        public void AddPages(IEnumerable<Page> a_pages)
        {
            m_pages.AddRange(a_pages);
            LastChange = DateTime.Now;
        }

        internal Work FindWork()
        {
            foreach (var work in DownloadManager.Works)
            {
                if (work.Chapter == this)
                    return work;
            }

            return null;
        }

        public ChapterState State
        {
            get
            {
                if (Work == null)
                    return m_state;
                else
                {
                    switch (Work.State)
                    {
                        case WorkState.Aborted: return ChapterState.Aborted;
                        case WorkState.Deleting: return ChapterState.Deleting;
                        case WorkState.Downloaded: return ChapterState.Downloaded;
                        case WorkState.Downloading: return ChapterState.Downloading;
                        case WorkState.Error: return ChapterState.Error;
                        case WorkState.Waiting: return ChapterState.Waiting;
                        case WorkState.Zipping: return ChapterState.Zipping;
                        default: throw new NotImplementedException();
                    }
                }
            }
           
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Serie, Title);
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
    }
}
