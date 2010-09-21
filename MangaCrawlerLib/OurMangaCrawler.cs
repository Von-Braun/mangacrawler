using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MangaCrawlerLib
{
    internal class OurMangaCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "OurManga";
            }
        }

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes("//div[@class='m_s_title']/a");

            foreach (var serie in series.Skip(1))
            {
                yield return new SerieInfo(
                    a_info,
                    serie.GetAttributeValue("href", "").RemoveFromRight(1),
                    serie.InnerText);
            }
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("//div[@class='manga_naruto_title']/a").AsEnumerable();

            chapters = from ch in chapters
                       where ch.ParentNode.ParentNode.ChildNodes[5].InnerText != "Soon!"
                       select ch;

            foreach (var chapter in chapters)
            {
                yield return new ChapterInfo(a_info, chapter.GetAttributeValue("href", ""), chapter.InnerText);
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var url = doc.DocumentNode.SelectSingleNode("//div[@id='Summary']/p[2]/a[2]");

            doc = ConnectionsLimiter.DownloadDocument(a_info, url.GetAttributeValue("href", ""));

            var pages = doc.DocumentNode.SelectNodes("//div[@class='inner_heading_right']/h3/select[2]/option");

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
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_info.ChapterInfo.URLPart + "/" + a_info.URLPart);

            var node = doc.DocumentNode.SelectSingleNode("//div[@class='inner_full_view']/h3/a/img");

            if (node == null)
                node = doc.DocumentNode.SelectSingleNode("//div[@class='inner_full_view']/h3/img");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.ourmanga.com/directory/";
        }
    }
}
