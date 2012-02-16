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

        public override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var rows = doc.DocumentNode.SelectNodes("/html/body/div/div/div[6]/div[2]/div/table/tr/td/table[2]/tr/td[2]/table/tr");

            var result = from row in rows 
                         where (row.ChildNodes.Count >= 8)
                         where (row.ChildNodes[3].InnerText != "None")
                         select new SerieInfo(a_info, 
                                              row.ChildNodes[1].ChildNodes[0].GetAttributeValue("href", ""), 
                                              row.ChildNodes[1].ChildNodes[0].InnerText);

            a_progress_callback(100, result);
        }

        public override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("/html/body/div/div/div[6]/div[2]/div/div/table[3]/tr/td[2]/table/tr/td[2]/a");

            string url = "http://www.mangatoshokan.com" + chapters[0].GetAttributeValue("href", "");

            doc = DownloadDocument(a_info.Server, url);

            chapters = doc.DocumentNode.SelectNodes("/html/body/div/div/table/tr/td[2]/select/option");

            var result = from chapter in chapters.Reverse().Skip(3).Reverse()
                         where chapter.NextSibling.InnerText != "[Series End]"
                         select new ChapterInfo(a_info, chapter.GetAttributeValue("value", ""), 
                             chapter.NextSibling.InnerText);

            a_progress_callback(100, result.Reverse());
        }

        public override IEnumerable<PageInfo> DownloadPages(TaskInfo a_info)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var pages = doc.DocumentNode.SelectNodes("/html/body/div/div/table/tr/td[3]/select/option").AsEnumerable();

            pages = from page in pages
                    where page.GetAttributeValue("value", "").Trim() != ""
                    select page;

            int index = 0;

            foreach (var page in pages)
            {
                index++;

                yield return new PageInfo(a_info, page.GetAttributeValue("value", ""), index, page.NextSibling.InnerText);
            }
        }

        public override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var image = doc.DocumentNode.SelectSingleNode("//*[@id=\"readerPage\"]");

            return image.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://www.mangatoshokan.com/read";
        }

        public override string GetSerieURL(SerieInfo a_info)
        {
            return "http://www.mangatoshokan.com" + a_info.URLPart;
        }

        public override string GetChapterURL(ChapterInfo a_info)
        {
            return "http://www.mangatoshokan.com" + a_info.URLPart;
        }

        public override string GetPageURL(PageInfo a_info)
        {
            return "http://www.mangatoshokan.com" + a_info.URLPart;
        }
    }
}
