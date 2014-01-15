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

            var result = (from chapter in chapters
                          select new Chapter(a_serie, chapter.GetAttributeValue("href", ""),
                              chapter.InnerText)).ToList();

            a_progress_callback(100, result);

            if (result.Count == 0)
            {
                if (!doc.DocumentNode.SelectSingleNode("//div[@id='chapters']/div[@class='clear']").
                    InnerText.Contains("No Manga Chapter"))
                {
                    throw new Exception("Serie has no chapters");
                }
            }
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            List<Page> result = new List<Page>();

            var top_center_bar = doc.DocumentNode.SelectSingleNode("//div[@id='top_center_bar']");
            var pages = top_center_bar.SelectNodes("div[@class='r m']/div[@class='l']/select[@class='m']/option");

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

                result.Add(pi);
            }

            if (result.Count == 0)
                throw new Exception("Chapter has no pages");

            return result;
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);

            var node = doc.DocumentNode.SelectSingleNode("//img[@id='image']");

            return node.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://mangafox.me//manga/";
        }
    }
}
