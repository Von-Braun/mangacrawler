using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace MangaCrawlerLib
{
    internal abstract class Crawler
    {
        internal abstract string Name { get; }
        internal abstract void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback);
        internal abstract void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback);
        internal abstract IEnumerable<PageInfo> DownloadPages(TaskInfo a_info);
        internal abstract string GetImageURL(PageInfo a_info);

        internal abstract string GetServerURL();

        internal virtual string GetSerieURL(SerieInfo a_info)
        {
            return a_info.URLPart;
        }

        internal virtual string GetChapterURL(ChapterInfo a_info)
        {
            return a_info.URLPart;
        }

        internal virtual string GetPageURL(PageInfo a_info)
        {
            return a_info.URLPart;
        }

        internal virtual int MaxConnectionsPerServer
        {
            get
            {
                return ConnectionsLimiter.MAX_CONNECTIONS_PER_SERVER;
            }
        }
    }
}
