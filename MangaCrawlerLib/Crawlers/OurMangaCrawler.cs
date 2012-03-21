using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    internal class OurMangaCrawler : Crawler
    {
        public override string Name
        {
            get 
            {
                return "Our Manga";
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes("//div[@class='m_s_title']/a");

            var result = from serie in series.Skip(1)
                         select new Serie(a_server,
                                              serie.GetAttributeValue("href", "").RemoveFromRight(1),
                                              serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters = doc.DocumentNode.SelectNodes("//div[@id='manga_nareo']/div").Skip(1);

            var chs = (from ch in chapters
                       where !ch.SelectSingleNode("div[3]").InnerText.ToLower().Contains("soon")
                       select ch.SelectSingleNode("div[1]/a")).ToArray();

            List<Chapter> result = new List<Chapter>();
            foreach (var ch in chs)
            {
                if (ch != null)
                    result.Add(new Chapter(a_serie, ch.GetAttributeValue("href", ""), ch.InnerText));
            }

            a_progress_callback(100, result);
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var url = doc.DocumentNode.SelectSingleNode("//div[@id='Summary']/p[2]/a[2]");

            doc = DownloadDocument(a_chapter, url.GetAttributeValue("href", ""));

            var pages = doc.DocumentNode.SelectNodes("//div[@class='inner_heading_right']/h3/select[2]/option");

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                Page pi = new Page(
                    a_chapter, 
                    a_chapter.URL + "/" + page.GetAttributeValue("value", ""), 
                    index, 
                    page.NextSibling.InnerText);

                yield return pi;
            }
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);

            var node = doc.DocumentNode.SelectSingleNode("//div[@class='inner_full_view']/h3/a/img");

            if (node == null)
                node = doc.DocumentNode.SelectSingleNode("//div[@class='inner_full_view']/h3/img");

            return node.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://www.ourmanga.com/directory/";
        }
    }
}
