using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading;

namespace MangaCrawlerLib
{
    internal class MangaToshokanCrawler : Crawler
    {
        public override string Name
        {
            get 
            {
                return "MangaToshokan";
            }
        }

        public override void DownloadSeries(Server a_server, Action<int, IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var rows = doc.DocumentNode.SelectNodes("/html/body/div/div/div[6]/div[2]/div/table/tr/td/table[2]/tr/td[2]/table/tr");

            var result = from row in rows 
                         where (row.ChildNodes.Count >= 8)
                         where (row.ChildNodes[3].InnerText != "None")
                         select new Serie(a_server, 
                                              "http://www.mangatoshokan.com" + 
                                                  row.ChildNodes[1].ChildNodes[0].GetAttributeValue("href", ""), 
                                              row.ChildNodes[1].ChildNodes[0].InnerText);

            a_progress_callback(100, result);
        }

        public override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters = doc.DocumentNode.SelectNodes("/html/body/div/div/div[6]/div[2]/div/div/table[3]/tr/td[2]/table/tr/td[2]/a");

            string url = "http://www.mangatoshokan.com" + chapters[0].GetAttributeValue("href", "");

            doc = DownloadDocument(a_serie.Server, url);

            chapters = doc.DocumentNode.SelectNodes("/html/body/div/div/table/tr/td[2]/select/option");

            var result = from chapter in chapters.Reverse().Skip(3).Reverse()
                         where chapter.NextSibling.InnerText != "[Series End]"
                         select new Chapter(a_serie, 
                                                "http://www.mangatoshokan.com" + chapter.GetAttributeValue("value", ""), 
                                                chapter.NextSibling.InnerText);

            a_progress_callback(100, result.Reverse());
        }

        public override IEnumerable<Page> DownloadPages(Work a_work)
        {
            HtmlDocument doc = DownloadDocument(a_work);

            var pages = doc.DocumentNode.SelectNodes("/html/body/div/div/table/tr/td[3]/select/option").AsEnumerable();

            pages = from page in pages
                    where page.GetAttributeValue("value", "").Trim() != ""
                    select page;

            int index = 0;

            foreach (var page in pages)
            {
                index++;

                yield return new Page(a_work, 
                                          "http://www.mangatoshokan.com" + page.GetAttributeValue("value", ""), 
                                          index, 
                                          page.NextSibling.InnerText);
            }
        }

        public override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);

            var image = doc.DocumentNode.SelectSingleNode("//*[@id=\"readerPage\"]");

            return image.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://www.mangatoshokan.com/read";
        }
    }
}
