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
using System.Diagnostics;
using System.Text.RegularExpressions;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    internal class StopTazmoCrawler : Crawler
    {
        public override string Name
        {
            get
            {
                return "StopTazmo";
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, 
            IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes(
                "/html/body/div/div[3]/div/table[2]/tbody/tr/td[1]/a");

            var result = from serie in series
                            select new Serie(a_server,
                                                serie.GetAttributeValue("href", ""),
                                                serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, 
            IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters = doc.DocumentNode.SelectNodes(
                "/html/body/div/div[3]/div/table/tbody/tr");

            var result = from chapter in chapters.Skip(1)
                            select new Chapter(a_serie,
                                chapter.SelectSingleNode("td[3]/a").GetAttributeValue("href", ""),
                                Path.GetFileNameWithoutExtension(chapter.SelectSingleNode("td[1]").
                                InnerText));

            a_progress_callback(100, result.Reverse());
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var images = Regex.Matches(doc.DocumentNode.InnerText, 
                "s.src = '.*(http://read\\.stoptazmo\\.com/.*//.*\\.(jpg|png|gif|bmp|jpeg))");

            for (int i=0; i<images.Count; i++)
            {
                string img_url = images[i].Groups[1].Value;
                string name = Path.GetFileNameWithoutExtension(img_url);
                yield return new Page(a_chapter, img_url, i, name);
            }
        }

        internal override string GetImageURL(Page a_page)
        {
            return a_page.URL;
        }

        public override string GetServerURL()
        {
            return "http://stoptazmo.com/downloads/manga_series.php?action=entire_list";
        }
    }
}
