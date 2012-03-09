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

        internal override void DownloadSeries(Server a_server, Action<int, 
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

        internal override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var n1 = doc.DocumentNode.SelectNodes("//b");
            var n2 = n1.Where(n => n.InnerText.StartsWith("Current Status")).First();

            var n3 = n2.NextSibling;
            while (n3.Name != "b")
                n3 = n3.NextSibling;
            while (n3.Name != "a")
                n3 = n3.NextSibling;

            var href = n3.GetAttributeValue("href", "");

            doc = DownloadDocument(a_serie, href);

            var chapters = doc.DocumentNode.SelectNodes("//select[@name='ch']/option");

            var result = from chapter in chapters
                         select new Chapter(
                             a_serie,
                             href + "?ch=" + chapter.GetAttributeValue("value", "").Replace(" ", "+") + "&page=1",
                             chapter.NextSibling.InnerText);

            a_progress_callback(100, result.Reverse());
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var pages = doc.DocumentNode.SelectNodes("//select[@name='page']/option");

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                Page pi = new Page(a_chapter, a_chapter.URL + "&page=" + 
                    page.GetAttributeValue("value", ""), 
                    index, page.NextSibling.InnerText);

                yield return pi;
            }
        }

        internal override string GetImageURL(Page a_page)
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
