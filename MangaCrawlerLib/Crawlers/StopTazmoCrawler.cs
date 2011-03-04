using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;

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

        internal override void DownloadSeries(ServerInfo a_info, Action<int, 
            IEnumerable<SerieInfo>> a_progress_callback)
        {
            try
            {
                HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

                var series = doc.DocumentNode.SelectNodes(
                    "/html/body/div/div[3]/div/table[2]/tbody/tr/td[1]/a");

                var result = from serie in series
                             select new SerieInfo(a_info,
                                                  serie.GetAttributeValue("href", "").
                                                    RemoveFromRight(1),
                                                  serie.InnerText);

                a_progress_callback(100, result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, 
            IEnumerable<ChapterInfo>> a_progress_callback)
        {
            try
            {
                HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

                var chapters = doc.DocumentNode.SelectNodes(
                    "/html/body/div/div[3]/div/table/tbody/tr");

                var result = from chapter in chapters.Skip(1)
                             select new ChapterInfo(a_info,
                                 chapter.SelectSingleNode("td[3]/a").GetAttributeValue("href", ""),
                                 Path.GetFileNameWithoutExtension(chapter.SelectSingleNode("td[1]").
                                    InnerText));

                a_progress_callback(100, result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info,
            CancellationToken a_token)
        {
            try
            {
                HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

                var serie = doc.DocumentNode.SelectSingleNode(
                    "//select[@name='series']/option[@selected]").GetAttributeValue("value", "");
                var chapter = doc.DocumentNode.SelectSingleNode(
                    "//select[@name='chapter']/option[@selected]").GetAttributeValue("value", "");
                var pages = doc.DocumentNode.SelectNodes(
                    "/html/body/div/div[3]/div/table/tbody/tr/td/table/tbody/tr/td/select[3]/option");
                var post_url = doc.DocumentNode.SelectSingleNode(
                    "/html/body/div/div[3]/div/table/tbody/tr/td/form").
                    GetAttributeValue("action", "");

                int index = 0;

                List<PageInfo> result = new List<PageInfo>();
                foreach (var page in pages)
                {
                    index++;

                    PageInfo pi = new PageInfo(
                        a_info, serie + "\t" + chapter + "\t" +
                            page.GetAttributeValue("value", "") + "\t" + post_url, index,
                        Path.GetFileNameWithoutExtension(page.NextSibling.InnerText.Trim()));

                    result.Add(pi);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            string[] ar = a_info.URLPart.Split(new[] { '\t' });

            var dict = new Dictionary<string, string>() 
            { 
                { "manga_hid", ar[0] }, 
                { "chapter_hid", ar[1] }, 
                { "image_hid", ar[2] }, 
                { "series", ar[0] }, 
                { "chapter", ar[1] }, 
                { "pagesel1", ar[2] }
            };

            HtmlDocument doc = ConnectionsLimiter.Submit(a_info, a_token, ar[3], dict);

            var image = doc.DocumentNode.SelectSingleNode(
                "/html/body/div/div[3]/div/table/tbody/tr/td/table/tbody/tr[2]/td/a/img");

            return image.GetAttributeValue("src", "");
        }

        internal override string GetSerieURL(SerieInfo a_info)
        {
            return base.GetSerieURL(a_info) + "/";
        }

        internal override string GetServerURL()
        {
            return "http://stoptazmo.com/downloads/manga_series.php?action=entire_list";
        }
    }
}
