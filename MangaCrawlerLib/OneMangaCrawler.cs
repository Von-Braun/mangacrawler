using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MangaCrawlerLib
{
    internal class OneMangaCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "OneManga";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes("//table[@class='ch-table']/tr/td[1]/a");

            var result = from serie in series.Skip(2)
                         select new SerieInfo(a_info,
                                              serie.GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1),
                                              serie.InnerText);

            a_progress_callback(100, result);  
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("//table[@class='ch-table']/tr/td[1]/a");

            var result = from chapter in chapters 
                         select new ChapterInfo(a_info, chapter.GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1), 
                             chapter.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var pages = doc.DocumentNode.SelectNodes("//select[@id='id_page_select']/option");

            a_info.PagesCount = pages.Count;

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo(a_info, page.GetAttributeValue("value", ""), index, page.NextSibling.InnerText);

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var node = doc.DocumentNode.SelectSingleNode("/html/body/div/div[3]/div/div[4]/a/img");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.onemanga.com/directory/";
        }

        internal override string GetSerieURL(SerieInfo a_info)
        {
            return "http://www.onemanga.com/" + a_info.URLPart;
        }

        internal override string GetChapterURL(ChapterInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, "http://www.onemanga.com/" + a_info.URLPart + "/");

            var url = doc.DocumentNode.SelectSingleNode("/html/body/div[2]/div[3]/div/ul/li/a");

            return "http://www.onemanga.com/" + url.GetAttributeValue("href", "").RemoveFromLeft(1);
        }

        internal override string GetPageURL(PageInfo a_info)
        {
            return String.Format("http://www.onemanga.com/{0}/{1}", a_info.ChapterInfo.URLPart, a_info.URLPart);
        }
    }
}
