using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading;
using TomanuExtensions;

namespace MangaCrawlerLib.Crawlers
{
    internal class MangaAccessCrawler : Crawler
    {
        public override string Name
        {
            get 
            {
                return "Manga Access";
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, 
            IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes(
                "/html/body/div/div[2]/table/tr/td[2]/div[3]/div/div[@class='c_h2b' or @class='c_h2']/a");

            var result = from serie in series
                         select new Serie(a_server,
                                              "http://manga-access.com" + serie.GetAttributeValue("href", ""),
                                              serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters = doc.DocumentNode.SelectNodes(
                "/html/body/div/div[2]/table/tr/td[2]/div[3]/div/div[@class='episode c_h2b' or @class='episode c_h2']/div/a");

            var result = from chapter in chapters
                         select new Chapter(a_serie, 
                                            "http://manga-access.com" + chapter.GetAttributeValue("href", ""), 
                                            chapter.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var pages = doc.DocumentNode.SelectNodes("//select[@id='page_switch']/option");

            return from page in pages
                   select new Page(a_chapter, 
                                   "http://manga-access.com" + page.GetAttributeValue("value", ""), pages.IndexOf(page) + 1,
                                   page.NextSibling.InnerText);
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);
            
            var image = doc.DocumentNode.SelectSingleNode("//div[@id='pic']/img");

            return image.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://www.manga-access.com/manga/list";
        }
    }
}
