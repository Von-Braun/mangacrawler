using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

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

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var series = doc.DocumentNode.SelectSingleNode("/html/body/div/div[8]/div[2]").SelectNodes("select/option");
            foreach (var serie in series.Skip(2))
            {
                yield return new SerieInfo()
                {
                    ServerInfo = a_info,
                    Name = serie.NextSibling.InnerText,
                    URLPart = GetServerURL() + serie.GetAttributeValue("value", "").RemoveFromLeft(1)
                };
            }
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var link_strong_nodes = doc.DocumentNode.SelectNodes("//a/strong");
            var begin_reading_strong_node = link_strong_nodes.Where(n => n.InnerText == "Begin Reading");
            var href = begin_reading_strong_node.First().ParentNode.GetAttributeValue("href", "");

            doc = new HtmlWeb().Load(href);

            var chapters = doc.DocumentNode.SelectNodes("//select[@name='ch']/option");

            foreach (var chapter in chapters)
            {
                yield return new ChapterInfo()
                {
                    SerieInfo = a_info,
                    Name = chapter.NextSibling.InnerText,
                    URLPart = href + "\t" + chapter.GetAttributeValue("value", "")
                };
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            string[] ar = a_info.URLPart.Split(new[] { '\t' });
            String url = String.Format("{0}?ch={1}&page={2}", ar[0], ar[1], 1);

            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(url);

            var pages = doc.DocumentNode.SelectNodes("//select[@name='page']/option");

            a_info.PagesCount = pages.Count;

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo()
                {
                    ChapterInfo = a_info,
                    Index = index,
                    URLPart = ar[0] + "\t" + ar[1] + "\t" + page.GetAttributeValue("value", ""),
                    Name = page.NextSibling.InnerText
                };

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            string[] ar = a_info.URLPart.Split(new[] { '\t' });
            String url = String.Format("{0}?ch={1}&page={2}", ar[0], ar[1], ar[2]);

            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(url);

            var img = doc.DocumentNode.SelectSingleNode("//div[@class='imgContainer']/a/img");

            return "http://view.thespectrum.net/" + img.GetAttributeValue("src", "").RemoveFromLeft(1);
        }

        internal override string GetServerURL()
        {
            return "http://www.thespectrum.net/";
        }
    }
}
