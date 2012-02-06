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

namespace MangaCrawlerLib
{
    internal class MangaVolumeCrawler : Crawler
    {
        internal override string Name
        {
            get
            {
                return "MangaVolume";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, 
            Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            List<string> pages = new List<string>();
            pages.Add(a_info.URL);

            do
            {
                var nodes_enum = doc.DocumentNode.SelectNodes(
                    "//div[@id='NavigationPanel']/ul/li/a");

                var nodes = nodes_enum.ToList();

                if (nodes.First().InnerText.ToLower() == "prev")
                    nodes.RemoveAt(0);
                if (nodes.Last().InnerText.ToLower() == "next")
                    nodes.RemoveLast();

                pages.AddRange(from node in nodes
                               select "http://www.mangavolume.com" +
                               node.GetAttributeValue("href", ""));

                string next_pages_group = String.Format(
                    "http://www.mangavolume.com/manga-archive/mangas/npage-{0}",
                    Int32.Parse(nodes.Last().InnerText) + 1);

                doc = ConnectionsLimiter.DownloadDocument(a_info, next_pages_group);

                if (doc != null)
                    pages.Add(next_pages_group);
            }
            while (doc != null);

            pages = pages.Distinct().ToList();

            ConcurrentBag<Tuple<int, int, string, string>> series =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            int series_progress = 0;

            Action<int> update = (progress) =>
            {
                var result = from serie in series
                             orderby serie.Item1, serie.Item2
                             select new SerieInfo(a_info, serie.Item4, serie.Item3);

                a_progress_callback(progress, result.ToArray());
            };

            Parallel.For(0, pages.Count,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer,
                    TaskScheduler = a_info.State.Scheduler[Priority.Series], 
                },
                (page, state) =>
            {
                try
                {
                    IEnumerable<HtmlNode> page_series = null;

                    HtmlDocument page_doc = ConnectionsLimiter.DownloadDocument(
                        a_info, pages[page]);
                    page_series = page_doc.DocumentNode.SelectNodes(
                        "//table[@id='MostPopular']/tr/td/a");

                    int index = 0;
                    foreach (var serie in page_series)
                    {
                        Tuple<int, int, string, string> s =
                            new Tuple<int, int, string, string>(page, index++, 
                                serie.SelectSingleNode("span").InnerText,
                                serie.GetAttributeValue("href", "").RemoveFromLeft(1));

                        series.Add(s);
                    }

                    series_progress++;
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

        internal override void DownloadChapters(SerieInfo a_info, Action<int, 
            IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            List<string> pages = new List<string>();
            pages.Add(a_info.URL);

            var license = doc.DocumentNode.SelectSingleNode("//div[@id='LicenseWarning']");

            if (license != null)
                return;

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

                string next_pages_group = String.Format("{0}npage-{1}", a_info.URL, 
                    Int32.Parse(nodes.Last().InnerText) + 1);

                doc = ConnectionsLimiter.DownloadDocument(a_info, next_pages_group);

                if (doc != null)
                    pages.Add(next_pages_group);
            }
            while (doc != null);

            pages = pages.Distinct().ToList();

            ConcurrentBag<Tuple<int, int, string, string>> series =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            int chapters_progress = 0;

            Action<int> update = (progress) =>
            {
                var result = from serie in series
                                orderby serie.Item1, serie.Item2
                                select new ChapterInfo(a_info, serie.Item4, serie.Item3);

                a_progress_callback(progress, result.ToArray());
            };

            Parallel.For(0, pages.Count,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer,
                    TaskScheduler = a_info.ServerInfo.State.Scheduler[Priority.Chapters], 
                },
                (page, state) =>
            {
                try
                {
                    HtmlDocument page_doc = 
                        ConnectionsLimiter.DownloadDocument(a_info, pages[page]);

                    var page_series = page_doc.DocumentNode.SelectNodes(
                        "//table[@id='MainList']/tr/td[1]/a");

                    if ((pages.Count == 1) && (page_series == null))
                    {
                        // No chapters in serie.
                        return;
                    }

                    int index = 0;
                    foreach (var serie in page_series)
                    {
                        Tuple<int, int, string, string> s =
                            new Tuple<int, int, string, string>(page, index++, serie.InnerText,
                                serie.GetAttributeValue("href", "").RemoveFromLeft(1));

                        series.Add(s);
                    }

                    chapters_progress++;
                    update(chapters_progress * 100 / pages.Count);
                }
                catch
                {
                    state.Break();
                    throw;
                }
            });

            update(100);
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var pages = doc.DocumentNode.SelectNodes("//select[@id='pages']/option");

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo(
                    a_info,
                    String.Format("http://www.mangavolume.com{0}", 
                        page.GetAttributeValue("value", "")),
                    index);

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

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

        internal override string GetServerURL()
        {
            return "http://www.mangavolume.com/manga-archive/mangas/";
        }

        internal override string GetSerieURL(SerieInfo a_info)
        {
            return "http://www.mangavolume.com/" + a_info.URLPart;
        }

        internal override string GetChapterURL(ChapterInfo a_info)
        {
            return "http://www.mangavolume.com/" + a_info.URLPart;
        }
    }
}
