using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MangaCrawlerLib.Crawlers
{
    // TODO: czas na przerobienie katalogu by byl gotowy na nowe cralwerly 
    internal class MangaStreamCrawler : Crawler
    {
        public override string Name
        {
            get
            {
                return "Manga Stream";
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes(
                "//div[@id='contentwrap-inner']//strong[@style='font-size:14px;']");

            var result = from serie in series
                         select new Serie(
                             a_server,
                             GetServerURL(),
                             serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters = doc.DocumentNode.SelectNodes("//div[@id='contentwrap-inner']//tr/td/strong|//div[@id='contentwrap-inner']//tr/td/a");

            if (chapters == null)
            {
                a_progress_callback(100, new Chapter[0]);
                return;
            }

            var pos = -1;
            List<Chapter> result = new List<Chapter>();

            foreach (HtmlNode chapter in chapters)
            {
                // find the manga
                if (chapter.InnerText == a_serie.Title)
                {
                    // found it, next 'a' items are the chapters
                    pos = chapters.IndexOf(chapter);
                }
                if (pos != -1 && chapter.Name != "strong")
                {
                    result.Add(new Chapter(
                                    a_serie,
                                    "http://www.mangastream.com" + chapter.GetAttributeValue("href", ""),
                                    chapter.InnerText));
                }
                if (chapter.Name == "strong" && chapter.InnerText != a_serie.Title)
                {
                    pos = -1;
                }
            }
            
            a_progress_callback(100, result);

            if (result.Count == 0)
                throw new Exception("Serie has no chapters");
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var pages = doc.DocumentNode.SelectNodes("//div[@id='controls']/a");

            List<Page> result = new List<Page>();

            foreach (HtmlNode page in pages)
            {
                if (!page.InnerText.Contains("Prev") && !page.InnerText.Contains("Next"))
                {
                    result.Add(new Page(
                       a_chapter,
                       "http://www.mangastream.com" + page.GetAttributeValue("href", ""),
                       pages.IndexOf(page) + 1,
                       page.InnerText));
                }
            }

            return result;
        }

        public override string GetServerURL()
        {
            return "http://mangastream.com/manga";
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);
            var image = doc.DocumentNode.SelectSingleNode("//img[@id='p']");
            return image.GetAttributeValue("src", "");
        }
    }
}
