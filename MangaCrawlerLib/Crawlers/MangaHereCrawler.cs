using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace MangaCrawlerLib.Crawlers
{
    internal class MangaHereCrawler : Crawler
    {
        public override string Name
        {
            get 
            {
                return "Manga Here";
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes("//div[@class='list_manga']/ul/li/a");

            var result = from serie in series
                         select new Serie(a_server, serie.GetAttributeValue("href", ""), serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters = doc.DocumentNode.SelectNodes("//div[@class='detail_list']/ul/li/span/a");

            if (chapters == null)
            {
                var no_chapters = doc.DocumentNode.SelectSingleNode("//div[@class='detail_list']/ul/li/div");
                if ((no_chapters != null) && no_chapters.InnerText.Contains("No Manga Chapter"))
                {
                    a_progress_callback(100, new Chapter[0]);
                    return;
                }

                var licensed = doc.DocumentNode.SelectSingleNode("//div[@class='detail_list']/div");
                if ((licensed != null) && licensed.InnerText.Contains("has been licensed, it is not available in"))
                {
                    a_progress_callback(100, new Chapter[0]);
                    return;
                }
            }

            var result = (from chapter in chapters
                          select new Chapter(a_serie, chapter.GetAttributeValue("href", ""), chapter.InnerText)).ToList();

            a_progress_callback(100, result);

            if (result.Count == 0)
                throw new Exception("Serie has no chapters");
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var pages = doc.DocumentNode.SelectNodes("//section[@class='readpage_top']/div[3]/span/select/option");

            var result = (from page in pages
                          select new Page(
                              a_chapter,
                              page.GetAttributeValue("value", ""),
                              pages.IndexOf(page) + 1,
                              page.NextSibling.InnerText)).ToList();

            if (result.Count == 0)
                throw new Exception("Chapter has no pages");

            return result;
        }

        public override string GetServerURL()
        {
            return "http://www.mangahere.com/mangalist/";
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);
            var image = doc.DocumentNode.SelectSingleNode("//img[@id='image']");
            return image.GetAttributeValue("src", "");
        }

        public override string GetImageURLExtension(string a_image_url)
        {
            var ext = base.GetImageURLExtension(a_image_url);
            var match = Regex.Match(ext, "\\.(?i)(jpg|gif|png|bmp)");
            return match.Value;
        }
    }
}
