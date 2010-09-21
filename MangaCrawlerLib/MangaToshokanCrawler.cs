using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

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

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var rows = doc.DocumentNode.SelectNodes("/html/body/div/div/div[6]/div[2]/div/table/tr/td/table[2]/tr/td[2]/table/tr");

            foreach (var row in rows)
            {
                if (row.ChildNodes.Count < 8)
                    continue;

                if (row.ChildNodes[3].InnerText == "None")
                    continue;

                yield return new SerieInfo(
                    a_info, 
                    row.ChildNodes[1].GetAttributeValue("href", ""), 
                    row.ChildNodes[1].InnerText);
            }
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("/html/body/div/div/div[6]/div[2]/div/div/table[3]/tr/td[2]/table/tr/td[2]/a");

            string url = "http://www.mangatoshokan.com" + chapters[0].GetAttributeValue("href", "");

            doc = ConnectionsLimiter.DownloadDocument(a_info, url);

            chapters = doc.DocumentNode.SelectNodes("/html/body/div/div/table/tr/td[2]/select/option");
            
            foreach (var chapter in chapters.Reverse().Skip(3).Reverse())
            {
                if (chapter.NextSibling.InnerText == "[Series End]")
                    continue;

                yield return new ChapterInfo(a_info, chapter.GetAttributeValue("value", ""), chapter.NextSibling.InnerText);
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var pages = doc.DocumentNode.SelectNodes("/html/body/div/div/table/tr/td[3]/select/option").AsEnumerable();

            pages = from page in pages
                    where page.GetAttributeValue("value", "").Trim() != ""
                    select page;

            a_info.PagesCount = pages.Count();

            int index = 0;

            foreach (var page in pages)
            {
                index++;

                yield return new PageInfo(a_info, page.GetAttributeValue("value", ""), index, page.NextSibling.InnerText);
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

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
