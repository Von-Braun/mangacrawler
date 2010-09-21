using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MangaCrawlerLib
{
    internal class Manga1000Crawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "1000Manga";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes("//table[@class='ch-table']/tr/td[1]/a");

            var result = from serie in series.Skip(2)
                         select new SerieInfo(a_info,
                                              "http://www.1000manga.com/" + serie.GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1),
                                              serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("//table[@class='ch-table mb20']/tr/td[1]/a");

            var result = from chapter in chapters
                         select new ChapterInfo(a_info,
                                                "http://www.1000manga.com/" + 
                                                    chapter.GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1),
                                                chapter.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapter_link = doc.DocumentNode.SelectSingleNode("//div[@id='chapter-link']/a").
                GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1);

            doc = ConnectionsLimiter.DownloadDocument(a_info, "http://www.1000manga.com/" + chapter_link);

            var pages = doc.DocumentNode.SelectNodes("//select[@id='id_page_select']/option");

            a_info.PagesCount = pages.Count;

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo(a_info, a_info.URLPart + "/" + page.GetAttributeValue("value", ""), 
                    index, page.NextSibling.InnerText);

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var node = doc.DocumentNode.SelectSingleNode("//div[@class='one-page']/a/img");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.1000manga.com/directory/";
        }
    }
}
