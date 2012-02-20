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
        public override string Name
        {
            get 
            {
                return "MangaFox";
            }
        }

        public override void DownloadSeries(Server a_server, Action<int, 
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

        public override void DownloadChapters(Serie a_serie, Action<int, 
            IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters = doc.DocumentNode.SelectNodes(
                "//div[@id='chapters']/ul/li/div/h3/a").Concat(
                    doc.DocumentNode.SelectNodes(
                        "//div[@id='chapters']/ul/li/div/h4/a"));

            var result = from chapter in chapters
                         select new Chapter(a_serie, chapter.GetAttributeValue("href", ""), 
                             chapter.InnerText);

            a_progress_callback(100, result);
        }

        public override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var pages = doc.DocumentNode.SelectSingleNode("//div[@class='r m']").
                SelectNodes("div[@class='l']/select[@class='m']/option");

            int index = 1;

            foreach (var page in pages)
            {
                Page pi = new Page(
                    a_chapter,
                    a_chapter.URL.Replace("1.html", String.Format("{0}.html", page.GetAttributeValue("value", ""))), 
                    index);

                index++;

                yield return pi;
            }
        }

        public override string GetImageURL(Page a_page)
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
