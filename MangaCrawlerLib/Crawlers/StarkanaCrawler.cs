using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading;
using TomanuExtensions;

namespace MangaCrawlerLib.Crawlers
{
    internal class StarkanaCrawler : Crawler
    {
        public override string Name
        {
            get 
            {
                return "Starkana";
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, 
            IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes(
                "//div[@id='inner_page']/div[@class='c_h2b' or @class='c_h2']/a");


            var result = from serie in series
                         select new Serie(a_server, 
                                          "http://starkana.com" + serie.GetAttributeValue("href", ""),
                                          serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters = doc.DocumentNode.SelectNodes(
                "//div/div/a[@class='download-link']");

            if (chapters == null)
            {
                var mature = doc.DocumentNode.SelectSingleNode("//a[@href='?mature_confirm=1']");
                if (mature != null)
                {
                    a_serie.URL = a_serie.URL + "?mature_confirm=1";
                    DownloadChapters(a_serie, a_progress_callback);
                    return;
                }

                var no_chapters = doc.DocumentNode.SelectNodes("//div/div/div[@class='c_h2']");
                if (no_chapters != null)
                {
                    if (no_chapters.Any(el => el.InnerText.Contains("We don't have any chapters for this manga. Do you?")))
                    {
                        a_progress_callback(100, new Chapter[0]);
                        return;
                    }
                }
            }

            var result = (from chapter in chapters
                          select new Chapter(a_serie,
                                             "http://starkana.com" + chapter.GetAttributeValue("href", ""),
                                             chapter.InnerText)).ToList();

            a_progress_callback(100, result);

            if (result.Count == 0)
                throw new Exception("Serie has no chapters");
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var pages = doc.DocumentNode.SelectNodes("//select[@id='page_switch']/option");

            var result = (from page in pages
                          select new Page(a_chapter,
                                          "http://starkana.com" + page.GetAttributeValue("value", ""),
                                          pages.IndexOf(page) + 1,
                                          page.NextSibling.InnerText)).ToList();

            if (result.Count == 0)
                throw new Exception("Chapter has no pages");

            return result;
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);
            
            var image = doc.DocumentNode.SelectSingleNode("//div[@id='pic']/div/img");

            return image.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://www.starkana.com/manga/list";
        }
    }
}
