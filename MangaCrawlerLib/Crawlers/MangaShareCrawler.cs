using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    internal class MangaShareCrawler : Crawler
    {
        public override string Name
        {
            get
            {
                return "Manga Share";
            }
        }

        public override void DownloadSeries(Server a_server, Action<int, IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes("//table[@class='datalist']/tr[@class='datarow']");

            var result = from serie in series 
                         select new Serie(a_server, 
                                              "http://read.mangashare.com/" + 
                                                  serie.SelectSingleNode("td[@class='datarow-0']/a").
                                                  GetAttributeValue("href", "").Split(new char[] { '/' }).Last(), 
                                              serie.SelectSingleNode("td[@class='datarow-1']/text()").InnerText);

            a_progress_callback(100, result);
        }

        public override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            string url = String.Format("{0}/chapter-001/page001.html", a_serie.URL);
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters = doc.DocumentNode.SelectNodes("//table[@class='datalist']/tr/td[4]/a");

            var result = from chapter in chapters 
                         select new Chapter(a_serie, chapter.GetAttributeValue("href", ""),
                             chapter.ParentNode.ParentNode.ChildNodes[3].InnerText);

            a_progress_callback(100, result);
        }

        public override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var pages = doc.DocumentNode.SelectNodes("//select[@name='pagejump']/option");

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                string link = a_chapter.URL;
                int page_index = link.LastIndexOf("/page");
                link = link.Left(page_index + 5);
                link += page.GetAttributeValue("Value", "") + ".html";

                Page pi = new Page(a_chapter, link, index);

                yield return pi;
            }
        }

        public override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@id='page']/a/img");

            if (node != null)
                return node.GetAttributeValue("src", "");

            return doc.DocumentNode.SelectSingleNode("//div[@id='page']/img").GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://read.mangashare.com/dir";
        }
    }
}
