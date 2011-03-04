using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading;

namespace MangaCrawlerLib
{
    internal class BleachExileCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "BleachExile";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, 
            IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes(
                "/html/body/table/tr[3]/td/table/tr/td/table/tr[3]/td/table/tr/td/a");

            var result = from serie in series
                         select new SerieInfo(a_info,
                                              serie.GetAttributeValue("href", ""),
                                              serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes(
                "/html/body/table/tr[3]/td/table/tr/td/table/tr[5]/td[2]/div/select[2]/option");

            var result = from chapter in chapters
                         select new ChapterInfo(a_info, chapter.GetAttributeValue("value", ""), 
                             chapter.NextSibling.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var pages = doc.DocumentNode.SelectNodes(
                "/html/body/table/tr[3]/td/table/tr/td/table/tr[5]/td[2]/select/option");

            return from page in pages
                   select new PageInfo(a_info, page.GetAttributeValue("value", ""), pages.IndexOf(page),
                       page.NextSibling.InnerText);
        }

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var image = doc.DocumentNode.SelectSingleNode(
                "/html/body/table/tr[3]/td/table/tr/td/table/tr[3]/td/a/img");

            if (image == null)
            {
                image = doc.DocumentNode.SelectSingleNode(
                    "/html/body/table/tr[3]/td/table/tr/td/table/tr[3]/td/img");
            }

            return image.GetAttributeValue("src", "");
        }

        internal override string GetPageURL(PageInfo a_info)
        {
            return String.Format("{0}-page-{1}.html", a_info.ChapterInfo.URL.RemoveFromRight(5), a_info.URLPart);
        }

        internal override string GetChapterURL(ChapterInfo a_info)
        {
            return String.Format("{0}-chapter-{1}.html", a_info.SerieInfo.URL.RemoveFromRight(5), a_info.URLPart);
        }

        internal override string GetSerieURL(SerieInfo a_info)
        {
            return "http://manga.bleachexile.com" + a_info.URLPart;
        }

        internal override string GetServerURL()
        {
            return "http://manga.bleachexile.com/series.html";
        }
    }
}
