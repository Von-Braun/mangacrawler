using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MangaCrawlerLib
{
    internal abstract class Crawler
    {
        internal abstract string Name { get; }
        internal abstract IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback);
        internal abstract IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback);
        internal abstract IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info);
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

        internal static string RemoveInvalidFileDirectoryCharacters(string a_path)
        {
            foreach (char c in Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct())
                a_path = a_path.Replace(new String(new char[] { c }), "");
            return a_path;
        }
    }
}
