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

namespace MangaCrawlerLib.Crawlers
{
    internal class MangaFoxCrawler : Crawler
    {
        public override string Name
        {
            get 
            {
                return "Manga Fox";
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, 
            IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes(
                "//div[@class='manga_list']/ul/li/a");

            var result = from serie in series
                         select new Serie(a_server,
                                              serie.GetAttributeValue("href", ""),
                                              serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, 
            IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var ch1 = doc.DocumentNode.SelectNodes("//ul[@class='chlist']/li/div/h3/a");
            var ch2 = doc.DocumentNode.SelectNodes("//ul[@class='chlist']/li/div/h4/a");

            List<HtmlNode> chapters = new List<HtmlNode>();
            if (ch1 != null)
                chapters.AddRange(ch1);
            if (ch2 != null)
                chapters.AddRange(ch2);

            var result = from chapter in chapters
                         select new Chapter(a_serie, chapter.GetAttributeValue("href", ""), 
                             chapter.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

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

                Page pi = new Page(
                    a_chapter,
                    a_chapter.URL.Replace("1.html", String.Format("{0}.html", page.GetAttributeValue("value", ""))), 
                    index, 
                    "");

                index++;

                yield return pi;
            }
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);

            var node = doc.DocumentNode.SelectSingleNode("//img[@id='image']");

            return node.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://www.mangafox.com/manga/";
        }
    }
}
