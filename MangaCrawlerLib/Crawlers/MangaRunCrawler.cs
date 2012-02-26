using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using System.Threading;
using TomanuExtensions;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MangaCrawlerLib 
{
    internal class MangaRunCrawler : Crawler
    {
        public override string Name
        {
            get 
            {
                return "MangaRun"; 
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, 
            IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            List<string> pages = new List<string>();
            pages.Add(a_server.URL);
            var pages_list = doc.DocumentNode.SelectNodes("/html/body/div[1]/a").SkipLast();
            foreach (var page in pages_list)
                pages.Add(GetServerURL() + page.GetAttributeValue("href", ""));

            ConcurrentBag<Tuple<int, int, string, string>> series =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            int series_progress = 0;

            Action<int> update = (progress) =>
            {
                var result = from serie in series
                             orderby serie.Item1, serie.Item2
                             select new Serie(a_server, serie.Item4, serie.Item3);

                a_progress_callback(progress, result.ToArray());
            };

            Parallel.For(0, pages.Count,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer,
                    TaskScheduler = a_server.Scheduler[Priority.Series], 
                },
                (page, state) =>
                {
                    try
                    {
                        HtmlDocument page_doc = DownloadDocument(
                            a_server, pages[page]);

                        var page_series1 = page_doc.DocumentNode.SelectNodes(
                            "/html/body/table/tr/td/table/tr[2]/td/table/td");
                        var page_series2 = page_doc.DocumentNode.SelectNodes(
                            "/html/body/table/tr/td/table/tr[2]/td/table/tr/td");
                        HtmlNode[] page_series;
                        if (page_series1 != null)
                            page_series = page_series1.Concat(page_series2).ToArray();
                        else
                            page_series = page_series2.ToArray();

                        int index = 0;

                        for (int i = 0; i < page_series.Length; i += 2)
                        {
                            string link = "http://www.mangarun.com/" + 
                                page_series[i].SelectSingleNode("a").GetAttributeValue("href", "");
                            string name = page_series[i + 1].SelectSingleNode("span").InnerText;

                            Tuple<int, int, string, string> s =
                                new Tuple<int, int, string, string>(page, index, name, link);

                            series.Add(s);

                            index += 1;
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
            var pages_list = doc.DocumentNode.SelectNodes("/html/body/div[2]/a").SkipLast();
            foreach (var page in pages_list)
                pages.Add(GetServerURL() + page.GetAttributeValue("href", ""));

            ConcurrentBag<Tuple<int, int, string, string>> chapters =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            int chapters_progress = 0;

            Action<int> update = (progress) =>
            {
                var result = from serie in chapters
                             orderby serie.Item1, serie.Item2
                             select new Chapter(a_serie, serie.Item4, serie.Item3);

                a_progress_callback(progress, result.Reverse());
            };

            Parallel.For(0, pages.Count,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer,
                    TaskScheduler = a_serie.Server.Scheduler[Priority.Chapters]
                },
                (page, state) =>
                {
                    try
                    {
                        HtmlDocument page_doc = DownloadDocument(
                            a_serie.Server, pages[page]);

                        var page_chapters1 = page_doc.DocumentNode.SelectNodes(
                            "/html/body/table/tr/td/table/tr[2]/td/table/td");
                        var page_chapters2 = page_doc.DocumentNode.SelectNodes(
                            "/html/body/table/tr/td/table/tr[2]/td/table/tr/td");
                        if ((page_chapters1 == null) && (page_chapters2 == null))
                            return;
                        HtmlNode[] page_chapters;
                        if (page_chapters1 != null)
                        {
                            if (page_chapters2 != null)
                                page_chapters = page_chapters1.Concat(page_chapters2).ToArray();
                            else
                                page_chapters = page_chapters1.ToArray();
                        }
                        else
                            page_chapters = page_chapters2.ToArray();

                        int index = 0;

                        for (int i = 0; i < page_chapters.Length; i += 2)
                        {
                            string link = "http://www.mangarun.com/" + 
                                page_chapters[i].SelectSingleNode("a").GetAttributeValue("href", "");
                            string name = page_chapters[i + 1].SelectSingleNode("span").InnerText;

                            Tuple<int, int, string, string> s =
                                new Tuple<int, int, string, string>(page, index, name, link);

                            chapters.Add(s);

                            index += 1;
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
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            List<string> pages = new List<string>();
            pages.Add(a_chapter.URL);
            var pages_list = doc.DocumentNode.SelectNodes("/html/body/div[2]/a");
            if (pages_list != null)
            {
                foreach (var page in pages_list.SkipLast())
                    pages.Add(GetServerURL() + page.GetAttributeValue("href", ""));
            }

            ConcurrentBag<Tuple<int, int, string, string>> result =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            Parallel.For(0, pages.Count,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer,
                    TaskScheduler = a_chapter.Serie.Server.Scheduler[Priority.Pages], 
                },
                (page, state) =>
                {
                    try
                    {
                        HtmlDocument page_doc = DownloadDocument(
                            a_chapter.Serie.Server, pages[page]);

                        var page_pages1 = page_doc.DocumentNode.SelectNodes(
                            "/html/body/table/td");
                        var page_pages2 = page_doc.DocumentNode.SelectNodes(
                            "/html/body/table/tr/td");
                        if ((page_pages1 == null) && (page_pages2 == null))
                            return;
                        HtmlNode[] page_pages;
                        if (page_pages1 != null)
                        {
                            if (page_pages2 != null)
                                page_pages = page_pages1.Concat(page_pages2).ToArray();
                            else
                                page_pages = page_pages1.ToArray();
                        }
                        else
                            page_pages = page_pages2.ToArray();

                        int index = 0;

                        foreach (var p in page_pages)
                        {
                            string link = "http://www.mangarun.com/" + 
                                p.SelectSingleNode("div[2]/a[1]").GetAttributeValue("href", "");
                            string name = p.SelectSingleNode("div[1]").InnerText;

                            Tuple<int, int, string, string> s =
                                new Tuple<int, int, string, string>(page, index, name, link);

                            result.Add(s);

                            index += 1;
                        }

                        if (a_chapter.Token.IsCancellationRequested)
                        {
                            Loggers.Cancellation.InfoFormat(
                                "Pages - token cancelled, a_url: {0}",
                                a_chapter.URL);

                            a_chapter.Token.ThrowIfCancellationRequested();
                        }
                    }
                    catch
                    {
                        state.Break();
                        throw;
                    }
                });


            return from serie in result
                   orderby serie.Item1, serie.Item2
                   select new Page(a_chapter, serie.Item4, result.IndexOf(serie) + 1, serie.Item3);
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);

            var node = doc.DocumentNode.SelectSingleNode(
                "/html/body/div[4]/img");

            return GetServerURL() + node.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://www.mangarun.com/";
        }
    }
}
