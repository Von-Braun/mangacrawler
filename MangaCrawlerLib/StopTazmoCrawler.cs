using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using System.Threading;

namespace MangaCrawlerLib
{
    internal class StopTazmoCrawler : Crawler
    {
        internal override string Name
        {
            get
            {
                return "StopTazmo";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes("/html/body/div[2]/div/div/div/ul/li/table[2]/tr/td[1]/a");

            var result = from serie in series
                         where serie.InnerText.Trim() != "[LATEST_DOWNLOADS]"
                         where serie.InnerText.Trim() != "[VOLUMES]"
                         select new SerieInfo(a_info, serie.GetAttributeValue("href", ""), serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("/html/body/div[2]/div/div/div/ul[2]/li/table/tr");

            var result = from chapter in chapters
                         select new ChapterInfo(a_info, 
                                                chapter.SelectSingleNode("td[3]/a").GetAttributeValue("href", ""), 
                                                Path.GetFileNameWithoutExtension(chapter.SelectSingleNode("td[1]").InnerText));

            a_progress_callback(100, result);
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var serie = doc.DocumentNode.SelectSingleNode("//select[@name='series']/option[@selected]").GetAttributeValue("value", "");
            var chapter = doc.DocumentNode.SelectSingleNode("//select[@name='chapter']/option[@selected]").GetAttributeValue("value", "");
            var pages = doc.DocumentNode.SelectNodes("//select[@class='selectpage']/option");
            var post_url = doc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div/table/tr/td/form").GetAttributeValue("action", "");

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo(
                    a_info, serie + "\t" + chapter + "\t" + page.GetAttributeValue("value", "") + "\t" + post_url, index,
                    Path.GetFileNameWithoutExtension(page.NextSibling.InnerText.Trim()));

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            string[] ar = a_info.URLPart.Split(new[] { '\t' });

            HtmlDocument doc = ConnectionsLimiter.Submit(a_info, a_token, ar[3],
                new Dictionary<string, string>() { { "manga_hid", ar[0] }, { "chapter_hid", ar[1] }, { "image_hid", ar[2] }, 
                                                    { "series", ar[0] }, { "chapter", ar[1] }, { "pagesel1", ar[2] }});

            var image = doc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div/table/tr/td").SelectSingleNode("table/tr[2]/td/a/img");

            return image.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://stoptazmo.com/downloads/manga_series.php";
        }
    }
}
