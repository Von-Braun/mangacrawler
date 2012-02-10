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

            var chapters = doc.DocumentNode.SelectNodes(
                "//div[@id='chapters']/ul/li/div/h3/a").Concat(
                    doc.DocumentNode.SelectNodes(
                        "//div[@id='chapters']/ul/li/div/h4/a"));

            var result = from chapter in chapters
                         select new ChapterInfo(a_info, chapter.GetAttributeValue("href", ""), 
                             chapter.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<PageInfo> DownloadPages(TaskInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var pages = doc.DocumentNode.SelectSingleNode("//div[@class='r m']").
                SelectNodes("div[@class='l']/select[@class='m']/option");

            int index = 1;

            foreach (var page in pages)
            {
                PageInfo pi = new PageInfo(
                    a_info,
                    a_info.URL.Replace("1.html", String.Format("{0}.html", page.GetAttributeValue("value", ""))), 
                    index);

                index++;

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var node = doc.DocumentNode.SelectSingleNode("//img[@id='image']");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.mangafox.com/manga/";
        }
    }
}
