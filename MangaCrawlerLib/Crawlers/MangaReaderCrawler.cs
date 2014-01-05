using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MangaCrawlerLib.Crawlers
{
    internal class MangaReaderCrawler : Crawler
    {
        public override string Name
        {
            get 
            {
                return "Manga Reader";
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes(
                "//div[@class='series_alpha']//ul[@class='series_alpha']/li/a");

            var result = from serie in series
                         select new Serie(
                             a_server, 
                             "http://www.mangareader.net" + serie.GetAttributeValue("href", ""), 
                             serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters = doc.DocumentNode.SelectNodes("//table[@id='listing']/tr/td/a");

            var result = (from chapter in chapters.Reverse()
                          select new Chapter(
                              a_serie,
                              "http://www.mangareader.net" + chapter.GetAttributeValue("href", ""),
                              chapter.InnerText)).ToList();

            a_progress_callback(100, result);

            if (result.Count == 0)
                throw new Exception("Serie has no chapters");
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var pages = doc.DocumentNode.SelectNodes("//div[@id='selectpage']/select/option");

            var result = (from page in pages
                          select new Page(
                              a_chapter,
                              "http://www.mangareader.net" + page.GetAttributeValue("value", ""),
                              pages.IndexOf(page) + 1,
                              page.NextSibling.InnerText)).ToList();

            if (result.Count == 0)
                throw new Exception("Chapter has no pages");

            return result;
        }

        public override string GetServerURL()
        {
            return "http://www.mangareader.net/alphabetical";
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);
            var image = doc.DocumentNode.SelectSingleNode("//div[@id='imgholder']/a/img");
            return image.GetAttributeValue("src", "");
        }
    }
}
