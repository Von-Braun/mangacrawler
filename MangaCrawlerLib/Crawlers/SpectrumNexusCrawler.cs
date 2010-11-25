using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading;

namespace MangaCrawlerLib
{
    internal class SpectrumNexusCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "Spectrum Nexus";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes("//div[@class='mangaJump']/select/option");

            var result = from serie in series.Skip(2) 
                         select new SerieInfo(a_info, 
                                              GetServerURL() + serie.GetAttributeValue("value", "").RemoveFromLeft(1), 
                                              serie.NextSibling.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var link_strong_nodes = doc.DocumentNode.SelectNodes("//a/strong");
            var begin_reading_strong_node = link_strong_nodes.Where(n => n.InnerText.StartsWith("Begin Reading"));
            var href = begin_reading_strong_node.First().ParentNode.GetAttributeValue("href", "");

            doc = ConnectionsLimiter.DownloadDocument(a_info, href);

            var chapters = doc.DocumentNode.SelectNodes("//select[@name='ch']/option");

            var result = from chapter in chapters
                         select new ChapterInfo(a_info, href + "\t" + chapter.GetAttributeValue("value", ""),
                                                chapter.NextSibling.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info, CancellationToken a_token)
        {
            string[] ar = a_info.URLPart.Split(new[] { '\t' });
            String url = String.Format("{0}?ch={1}&page={2}", ar[0], ar[1], 1);

            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token, url);

            var pages = doc.DocumentNode.SelectNodes("//select[@name='page']/option");

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo(a_info, ar[0] + "\t" + ar[1] + "\t" + page.GetAttributeValue("value", ""), 
                    index, page.NextSibling.InnerText);

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            string[] ar = a_info.URLPart.Split(new[] { '\t' });
            String url = String.Format("{0}?ch={1}&page={2}", ar[0], ar[1], ar[2]);

            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token, url);

            var img = doc.DocumentNode.SelectSingleNode("//div[@class='imgContainer']/a/img");

            return "http://view.thespectrum.net/" + img.GetAttributeValue("src", "").RemoveFromLeft(1);
        }

        internal override string GetServerURL()
        {
            return "http://www.thespectrum.net/";
        }
    }
}
