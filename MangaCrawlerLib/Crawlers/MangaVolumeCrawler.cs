using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Xml;
using System.Net;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using TomanuExtensions;

namespace MangaCrawlerLib.Crawlers
{
    internal class MangaVolumeCrawler : Crawler
    {
        public override string Name
        {
            get
            {
                return "Manga Volume";
            }
        }

        internal override void DownloadSeries(Server a_server, 
            Action<int, IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            List<string> pages = new List<string>();

            Func<object, string> prepare_page_url = index =>
                 String.Format(
                    "http://www.mangavolume.com/manga-archive/mangas/npage-{0}",
                    index);

            int current_page = 1;
            pages.Add(a_server.URL);

            for (;;)
            {
                var links = doc.DocumentNode.SelectNodes(
                    "//div[@id='NavigationPanel']/ul/li/a | //div[@id='NavigationPanel']/ul/li/span");
                var nodes = links.Select(el => el.InnerText.ToLower()).ToList();

                nodes.Remove("next");
                nodes.Remove("prev");

                var indexes = nodes.Select(el => Int32.Parse(el)).ToList();

                foreach (var index in indexes)
                {
                    if (index != 1)
                        pages.Add(prepare_page_url(index));
                }

                string next_pages_group = null;

                if (current_page == indexes.Last())
                {
                    if (links.Last().Name == "span")
                        break;

                    next_pages_group = prepare_page_url(indexes.Last() + 1);
                    current_page = indexes.Last() + 1;
                }
                else
                {
                    next_pages_group = prepare_page_url(indexes.Last());
                    current_page = indexes.Last();
                }

                doc = DownloadDocument(a_server, next_pages_group);
            }

            ConcurrentBag<Tuple<int, int, string, string>> series =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            pages = pages.Distinct().ToList();
            int series_progress = 0;

            Action<int> update = (progress) =>
            {
                var result = from serie in series
                             orderby serie.Item1, serie.Item2
                             select new Serie(a_server, serie.Item4, serie.Item3);

                a_progress_callback(progress, result.ToArray());
            };

            Parallel.ForEach(pages, 
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer
                },
                (page, state, page_index) =>
            {
                try
                {
                    IEnumerable<HtmlNode> page_series = null;

                    HtmlDocument page_doc = DownloadDocument(
                        a_server, page);
                    page_series = page_doc.DocumentNode.SelectNodes(
                        "//table[@id='MostPopular']/tr/td/a");

                    int index = 0;
                    foreach (var serie in page_series)
                    {
                        Tuple<int, int, string, string> s =
                            new Tuple<int, int, string, string>((int)page_index, index++, 
                                serie.SelectSingleNode("span").InnerText,
                                "http://www.mangavolume.com" + serie.GetAttributeValue("href", ""));

                        series.Add(s);
                    }

                    Interlocked.Increment(ref series_progress);
                    update(series_progress * 100 / pages.Count);
                }
                catch
                {
                    state.Break();
                    throw;
                }
            });

            update(100);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, 
            IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            List<string> pages = new List<string>();
            pages.Add(a_serie.URL);

            var license = doc.DocumentNode.SelectSingleNode("//div[@id='LicenseWarning']");

            if (license != null)
            {
                a_progress_callback(100, new Chapter[0]);
                return;
            }

            do
            {
                var nodes_enum = doc.DocumentNode.SelectNodes(
                    "//div[@id='NavigationPanel']/ul/li/a");

                if (nodes_enum == null)
                {
                    if (pages.Count > 1)
                        pages.RemoveLast();
                    break;
                }

                var nodes = nodes_enum.ToList();

                if (nodes.First().InnerText.ToLower() == "prev")
                    nodes.RemoveAt(0);
                if (nodes.Last().InnerText.ToLower() == "next")
                    nodes.RemoveLast();

                pages.AddRange(from node in nodes
                               select "http://www.mangavolume.com" + 
                               node.GetAttributeValue("href", ""));

                string next_pages_group = String.Format("{0}npage-{1}", a_serie.URL, 
                    Int32.Parse(nodes.Last().InnerText) + 1);

                doc = DownloadDocument(a_serie, next_pages_group);

                if (doc != null)
                    pages.Add(next_pages_group);
            }
            while (doc != null);

            pages = pages.Distinct().ToList();

            ConcurrentBag<Tuple<int, int, string, string>> chapters =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            int chapters_progress = 0;

            Action<int> update = (progress) =>
            {
                var result = from serie in chapters
                                orderby serie.Item1, serie.Item2
                                select new Chapter(a_serie, serie.Item4, serie.Item3);

                a_progress_callback(progress, result.ToArray());
            };

            bool empty = false;

            Parallel.For(0, pages.Count,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer
                },
                (page, state) =>
            {
                try
                {
                    HtmlDocument page_doc = 
                        DownloadDocument(a_serie, pages[page]);

                    var page_series = page_doc.DocumentNode.SelectNodes(
                        "//table[@id='MainList']/tr/td[1]/a");

                    if (page_series == null)
                    {
                        if (pages.Count == 1)
                        {
                            var similiar_series = page_doc.DocumentNode.SelectSingleNode("//table[@id='MostPopular']");
                            if (similiar_series != null)
                            {
                                var h2 = similiar_series.PreviousSibling;
                                if ((h2 != null) && (h2.Name == "#text"))
                                {
                                    h2 = h2.PreviousSibling;
                                    if ((h2 != null) && (h2.Name == "h2") && (h2.InnerText == "Similar series"))
                                    {
                                        var p = h2.PreviousSibling;
                                        if ((p != null) && (p.Name == "#text"))
                                        {
                                            p = p.PreviousSibling;
                                            if ((p != null) && (p.Name == "p") && (p.InnerText.Trim() == "&nbsp;"))
                                            {
                                                empty = true;
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    int index = 0;
                    foreach (var serie in page_series)
                    {
                        Tuple<int, int, string, string> s =
                            new Tuple<int, int, string, string>(page, index++, serie.InnerText,
                                "http://www.mangavolume.com" + serie.GetAttributeValue("href", ""));

                        chapters.Add(s);
                    }

                    Interlocked.Increment(ref chapters_progress);
                    update(chapters_progress * 100 / pages.Count);
                }
                catch
                {
                    state.Break();
                    throw;
                }
            });

            update(100);

            if (!empty)
                if (chapters.Count == 0)
                    throw new Exception("Serie has no chapters");
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var pages = doc.DocumentNode.SelectNodes("//select[@id='pages']/option");

            var result = new List<Page>();

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                Page pi = new Page(
                    a_chapter,
                    String.Format("http://www.mangavolume.com{0}", 
                        page.GetAttributeValue("value", "")),
                    index, 
                    "");

                result.Add(pi);
            }

            if (result.Count == 0)
                throw new Exception("Chapter has no pages");

            return result;
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);

            var img = doc.DocumentNode.SelectSingleNode(
                "/html[1]/body[1]/div[1]/div[3]/div[1]/table[2]/tr[5]/td[1]/a[1]/img[1]");
            if (img == null)
            {
                img = doc.DocumentNode.SelectSingleNode(
                    "/html[1]/body[1]/div[1]/div[3]/div[1]/table[2]/tr[5]/td[1]/img[1]");
            }
            if (img == null)
            {
                img = doc.DocumentNode.SelectSingleNode(
                    "/html[1]/body[1]/div[1]/div[3]/div[1]/table[1]/tr[5]/td[1]/a[1]/img[1]");
            }
            if (img == null)
            {
                img = doc.DocumentNode.SelectSingleNode(
                    "/html[1]/body[1]/div[1]/div[3]/div[1]/table[1]/tr[5]/td[1]/img[1]");
            }
            
            return img.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://www.mangavolume.com/manga-archive/mangas/";
        }
    }
}
