using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Xml;
using System.Net;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    internal class MangaFoxCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "MangaFox";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, 
            IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes(
                "//div[@class='manga_list']/ul/li/a");

            var result = from serie in series
                         select new SerieInfo(a_info,
                                              serie.GetAttributeValue("href", ""),
                                              serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, 
            IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var ch1 = doc.DocumentNode.SelectNodes("//ul[@class='chlist']/li/div/h3/a");
            var ch2 = doc.DocumentNode.SelectNodes("//ul[@class='chlist']/li/div/h4/a");

            List<HtmlNode> chapters = new List<HtmlNode>();
            if (ch1 != null)
                chapters.AddRange(ch1);
            if (ch2 != null)
                chapters.AddRange(ch2);

            var result = from chapter in chapters
                         select new ChapterInfo(a_info, chapter.GetAttributeValue("href", ""), 
                             chapter.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info, 
            CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var m = doc.DocumentNode.SelectSingleNode("//div[@class='r m']");

            if (m == null)
                yield break;

            var pages = m.SelectNodes("div[@class='l']/select[@class='m']/option");

            int index = 1;

            foreach (var page in pages)
            {
                if (page.NextSibling != null)
                {
                    if (page.NextSibling.InnerText == "Comments")
                        continue;
                }

                PageInfo pi = new PageInfo(
                    a_info,
                    a_info.URL.Replace("1.html", String.Format("{0}.html", page.GetAttributeValue("value", ""))), 
                    index);

                index++;

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var node = doc.DocumentNode.SelectSingleNode("//img[@id='image']");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.mangafox.com/manga/";
        }
    }
}
