using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    internal class StopTazmoCrawler : Crawler
    {
        public override string Name
        {
            get
            {
                return "StopTazmo";
            }
        }

        public override void DownloadSeries(ServerInfo a_info, Action<int, 
            IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes(
                "/html/body/div/div[3]/div/table[2]/tbody/tr/td[1]/a");

            var result = from serie in series
                            select new SerieInfo(a_info,
                                                serie.GetAttributeValue("href", ""),
                                                serie.InnerText);

            a_progress_callback(100, result);
        }

        public override void DownloadChapters(SerieInfo a_info, Action<int, 
            IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes(
                "/html/body/div/div[3]/div/table/tbody/tr");

            var result = from chapter in chapters.Skip(1)
                            select new ChapterInfo(a_info,
                                chapter.SelectSingleNode("td[3]/a").GetAttributeValue("href", ""),
                                Path.GetFileNameWithoutExtension(chapter.SelectSingleNode("td[1]").
                                InnerText));

            a_progress_callback(100, result.Reverse());
        }

        public override IEnumerable<PageInfo> DownloadPages(TaskInfo a_info)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var images = Regex.Matches(doc.DocumentNode.InnerText, 
                "s.src = '.*(http://read\\.stoptazmo\\.com/.*//.*\\.(jpg|png|gif|bmp|jpeg))");

            for (int i=0; i<images.Count; i++)
            {
                string img_url = images[i].Groups[1].Value;
                string name = Path.GetFileNameWithoutExtension(img_url);
                yield return new PageInfo(a_info, img_url, i, name);
            }
        }

        // TODO: 
        public override string GetImageURL(PageInfo a_info)
        {
            return a_info.URL;
        }

        public override string GetServerURL()
        {
            return "http://stoptazmo.com/downloads/manga_series.php?action=entire_list";
        }
    }
}
