using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading;
using TomanuExtensions;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    internal class SpectrumNexusCrawler : Crawler
    {
        public override string Name
        {
            get 
            {
                return "Spectrum Nexus";
            }
        }

        public override void DownloadSeries(Server a_server, Action<int, 
            IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes("//div[@class='mangaJump']/select").Elements().ToList();


            for (int i = series.Count - 1; i >= 0; i--)
            {
                if (series[i].NodeType != HtmlNodeType.Text)
                    continue;
                string str = series[i].InnerText;
                str = str.Trim();
                str = str.Replace("\n", "");
                if (str == "")
                    series.RemoveAt(i);
            }
            
            var splitter = series.FirstOrDefault(s => s.InnerText.Contains("---"));
            if (splitter != null)
            {
                int splitter_index = series.IndexOf(splitter);
                series.RemoveRange(0, splitter_index + 1);
            }

            List<Serie> result = new List<Serie>();

            for (int i = 0; i < series.Count; i += 2)
            {
                Serie si = new Serie(
                    a_server,
                    "http://www.thespectrum.net" + series[i].GetAttributeValue("value", ""), 
                    series[i + 1].InnerText);

                result.Add(si);
            }

            a_progress_callback(100, result);
        }

        public override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var link_strong_nodes = doc.DocumentNode.SelectNodes("//a");
            var begin_reading_strong_node = link_strong_nodes.Where(
                n => n.InnerText.StartsWith("Begin Reading"));
            var href = begin_reading_strong_node.First().GetAttributeValue("href", "");

            doc = DownloadDocument(a_serie.Server, href);

            var chapters = doc.DocumentNode.SelectNodes("//select[@name='ch']/option");

            var result = from chapter in chapters
                         select new Chapter(
                             a_serie,
                             "http://www.thespectrum.net" + href + "?ch=" +
                                 chapter.GetAttributeValue("value", "").Replace(" ", "+") + "&page=1",
                             chapter.NextSibling.InnerText);

            a_progress_callback(100, result.Reverse());
        }

        public override IEnumerable<Page> DownloadPages(Work a_work)
        {
            HtmlDocument doc = DownloadDocument(a_work);

            var pages = doc.DocumentNode.SelectNodes("//select[@name='page']/option");

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                Page pi = new Page(a_work, a_work.URL + "&page=" + 
                    page.GetAttributeValue("value", ""), 
                    index, page.NextSibling.InnerText);

                yield return pi;
            }
        }

        public override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);

            var img = doc.DocumentNode.SelectSingleNode("//div[@class='imgContainer']/a/img");

            if (a_page.URL.ToLower().Contains("view.thespectrum.net"))
            {
                return "http://view.thespectrum.net/" + img.GetAttributeValue("src", "").
                    RemoveFromLeft(1);
            }
            else
            {
                return "http://view.mangamonger.com/" + img.GetAttributeValue("src", "").
                    RemoveFromLeft(1);
            }
        }

        public override string GetServerURL()
        {
            return "http://www.thespectrum.net/";
        }
    }
}
