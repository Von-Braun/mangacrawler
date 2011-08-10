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
        internal override string Name
        {
            get 
            {
                return "OurManga";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes("//div[@class='m_s_title']/a");

            var result = from serie in series.Skip(1)
                         select new SerieInfo(a_info,
                                              serie.GetAttributeValue("href", "").RemoveFromRight(1),
                                              serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("//div[@class='manga_naruto_title']/a").AsEnumerable();

            chapters = from ch in chapters
                       where ch.ParentNode.ParentNode.ChildNodes[5].InnerText != "Soon!"
                       select ch;

            var result = from chapter in chapters 
                         select new ChapterInfo(a_info, chapter.GetAttributeValue("href", ""), chapter.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var url = doc.DocumentNode.SelectSingleNode("//div[@id='Summary']/p[2]/a[2]");

            doc = ConnectionsLimiter.DownloadDocument(a_info, a_token, url.GetAttributeValue("href", ""));

            a_token.ThrowIfCancellationRequested();

            var pages = doc.DocumentNode.SelectNodes("//div[@class='inner_heading_right']/h3/select[2]/option");

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo(a_info, page.GetAttributeValue("value", ""), index, page.NextSibling.InnerText);

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token, a_info.ChapterInfo.URLPart + "/" + a_info.URLPart);

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
