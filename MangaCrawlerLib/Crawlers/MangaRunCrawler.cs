using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using System.Threading;
using TomanuExtensions;

namespace MangaCrawlerLib 
{
    internal class MangaRunCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "MangaRun"; 
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, 
            IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes("/html/body/div[2]/table/tr/td/div/a");

            var result = from serie in series
                         select new SerieInfo(a_info,
                                              GetServerURL() + 
                                              serie.GetAttributeValue("href", "").
                                                RemoveFromLeft(1),
                                              serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, 
            IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("/html/body/div[2]/table/tr/td/div/a");

            var result = from chapter in chapters 
                         select new ChapterInfo(a_info, 
                                                GetServerURL() + 
                                                chapter.GetAttributeValue("href", "").
                                                    RemoveFromLeft(1), 
                                                chapter.InnerText);

            a_progress_callback(100, result.Reverse());            
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info, 
            CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var pages = doc.DocumentNode.SelectNodes("/html/body/div[2]/table/tr/td/div/a");

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo(a_info, GetServerURL() + 
                    page.GetAttributeValue("href", "").RemoveFromLeft(1), index,
                    Path.GetFileNameWithoutExtension(page.GetAttributeValue("href", "")));

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var node = doc.DocumentNode.SelectSingleNode(
                "/html/body/div[2]/table/tr/td[2]/div/img");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.mangarun.com/";
        }
    }
}
