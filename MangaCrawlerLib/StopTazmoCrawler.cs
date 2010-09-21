﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;

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

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes("/html/body/div[2]/div/div/div/ul/li/table[2]/tr/td[1]/a");

            var series_filtered = from serie in series
                                  where serie.InnerText.Trim() != "[LATEST_DOWNLOADS]"
                                  where serie.InnerText.Trim() != "[VOLUMES]"
                                  select serie;

            foreach (var serie in series_filtered)
            {
                yield return new SerieInfo(a_info, serie.GetAttributeValue("href", ""), serie.InnerText);
            }
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("/html/body/div[2]/div/div/div/ul[2]/li/table/tr");

            foreach (var chapter in chapters.Skip(1))
            {
                yield return new ChapterInfo(a_info, 
                    chapter.SelectSingleNode("td[3]/a").GetAttributeValue("href", ""), 
                    Path.GetFileNameWithoutExtension(chapter.SelectSingleNode("td[1]").InnerText));
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var serie = doc.DocumentNode.SelectSingleNode("//select[@name='series']/option[@selected]").GetAttributeValue("value", "");
            var chapter = doc.DocumentNode.SelectSingleNode("//select[@name='chapter']/option[@selected]").GetAttributeValue("value", "");
            var pages = doc.DocumentNode.SelectNodes("//select[@class='selectpage']/option");
            var post_url = doc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div/table/tr/td/form").GetAttributeValue("action", "");

            a_info.PagesCount = pages.Count;

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

        internal override string GetImageURL(PageInfo a_info)
        {
            string[] ar = a_info.URLPart.Split(new[] { '\t' });

            ConnectionsLimiter.Aquire(a_info.ChapterInfo.SerieInfo.ServerInfo);

            try
            {
                HtmlDocument doc = HTTPUtils.Submit(ar[3],
                    new Dictionary<string, string>() { { "manga_hid", ar[0] }, { "chapter_hid", ar[1] }, { "image_hid", ar[2] }, 
                                                       { "series", ar[0] }, { "chapter", ar[1] }, { "pagesel1", ar[2] }});

                var image = doc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div/table/tr/td").SelectSingleNode("table/tr[2]/td/a/img");

                return image.GetAttributeValue("src", "");
            }
            finally
            {
                ConnectionsLimiter.Release(a_info.ChapterInfo.SerieInfo.ServerInfo);
            }
        }

        internal override string GetServerURL()
        {
            return "http://stoptazmo.com/downloads/manga_series.php";
        }
    }
}
