using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;

namespace MangaCrawlerLib
{
    internal class MangaRunCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "MangaRun";
            }
        }

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes("/html/body/div[2]/table/tr/td/div/a");

            foreach (var serie in series)
            {
                yield return new SerieInfo(
                    a_info,
                    GetServerURL() + serie.GetAttributeValue("href", "").RemoveFromLeft(1),
                    serie.InnerText);
            }
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("/html/body/div[2]/table/tr/td/div/a");

            foreach (var chapter in chapters)
            {
                yield return new ChapterInfo(a_info, 
                    GetServerURL() + chapter.GetAttributeValue("href", "").RemoveFromLeft(1), chapter.InnerText);
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var pages = doc.DocumentNode.SelectNodes("/html/body/div[2]/table/tr/td/div/a");

            a_info.PagesCount = pages.Count;

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo(a_info, GetServerURL() + page.GetAttributeValue("href", "").RemoveFromLeft(1), index);

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var node = doc.DocumentNode.SelectSingleNode("/html/body/div[2]/table/tr/td[2]/div/img");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.mangarun.com/";
        }
    }
}
