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
        internal override string Name
        {
            get 
            {
                return "MangaToshokan";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var rows = doc.DocumentNode.SelectNodes("/html/body/div/div/div[6]/div[2]/div/table/tr/td/table[2]/tr/td[2]/table/tr");

            var result = from row in rows 
                         where (row.ChildNodes.Count >= 8)
                         where (row.ChildNodes[3].InnerText != "None")
                         select new SerieInfo(a_info, 
                                              row.ChildNodes[1].ChildNodes[0].GetAttributeValue("href", ""), 
                                              row.ChildNodes[1].ChildNodes[0].InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("/html/body/div/div/div[6]/div[2]/div/div/table[3]/tr/td[2]/table/tr/td[2]/a");

            string url = "http://www.mangatoshokan.com" + chapters[0].GetAttributeValue("href", "");

            doc = ConnectionsLimiter.DownloadDocument(a_info, url);

            chapters = doc.DocumentNode.SelectNodes("/html/body/div/div/table/tr/td[2]/select/option");

            var result = from chapter in chapters.Reverse().Skip(3).Reverse()
                         where chapter.NextSibling.InnerText != "[Series End]"
                         select new ChapterInfo(a_info, chapter.GetAttributeValue("value", ""), chapter.NextSibling.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

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

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var image = doc.DocumentNode.SelectSingleNode("//*[@id=\"readerPage\"]");

            return image.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.mangatoshokan.com/read";
        }

        internal override string GetSerieURL(SerieInfo a_info)
        {
            return "http://www.mangatoshokan.com" + a_info.URLPart;
        }

        internal override string GetChapterURL(ChapterInfo a_info)
        {
            return "http://www.mangatoshokan.com" + a_info.URLPart;
        }

        internal override string GetPageURL(PageInfo a_info)
        {
            return "http://www.mangatoshokan.com" + a_info.URLPart;
        }
    }
}
