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
        private ChapterState m_state;

        public Work Work;
        public string URL { get; private set; }
        public Serie Serie { get; private set; }
        public string Title { get; private set; }
        
        internal Chapter(Serie a_serie, string a_url, string a_title)
        {
            Serie = a_serie;
            URL = HttpUtility.HtmlDecode(a_url);

            Title = a_title.Trim();
            Title = Title.Replace("\t", " ");
            while (Title.IndexOf("  ") != -1)
                Title = Title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(Title);

            //m_state = DownloadManager.Downloaded.WasDownloaded(this) ?
            //    ChapterState.WasDownloaded : ChapterState.Initial;
            m_state = ChapterState.Initial;
        }

        public Work FindWork()
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
    }
}
