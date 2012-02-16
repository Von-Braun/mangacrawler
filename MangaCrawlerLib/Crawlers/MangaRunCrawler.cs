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

        public override void DownloadSeries(ServerInfo a_info, Action<int, 
            IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            List<string> pages = new List<string>();
            pages.Add(a_info.URL);
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
                             select new SerieInfo(a_info, serie.Item4, serie.Item3);

                a_progress_callback(progress, result.ToArray());
            };

            Parallel.For(0, pages.Count,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer,
                    TaskScheduler = a_info.Scheduler[Priority.Series], 
                },
                (page, state) =>
                {
                    try
                    {
                        HtmlDocument page_doc = DownloadDocument(
                            a_info, pages[page]);

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
                            string link = page_series[i].SelectSingleNode("a").GetAttributeValue("href", "");
                            string name = page_series[i + 1].SelectSingleNode("span").InnerText;

                            Tuple<int, int, string, string> s =
                                new Tuple<int, int, string, string>(page, index, name, link);

                            series.Add(s);

                            index += 1;
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

        public override void DownloadChapters(SerieInfo a_info, Action<int, 
            IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            List<string> pages = new List<string>();
            pages.Add(a_info.URL);
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
                             select new ChapterInfo(a_info, serie.Item4, serie.Item3);

                a_progress_callback(progress, result.Reverse());
            };

            Parallel.For(0, pages.Count,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer,
                    TaskScheduler = a_info.Server.Scheduler[Priority.Chapters]
                },
                (page, state) =>
                {
                    try
                    {
                        HtmlDocument page_doc = DownloadDocument(
                            a_info.Server, pages[page]);

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
                            string link = page_chapters[i].SelectSingleNode("a").GetAttributeValue("href", "");
                            string name = page_chapters[i + 1].SelectSingleNode("span").InnerText;

                            Tuple<int, int, string, string> s =
                                new Tuple<int, int, string, string>(page, index, name, link);

                            chapters.Add(s);

                            index += 1;
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

        public override IEnumerable<PageInfo> DownloadPages(TaskInfo a_info)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            List<string> pages = new List<string>();
            pages.Add(a_info.URL);
            var pages_list = doc.DocumentNode.SelectNodes("/html/body/div[2]/a");
            if (pages_list != null)
            {
                foreach (var page in pages_list.SkipLast())
                    pages.Add(GetServerURL() + page.GetAttributeValue("href", ""));
            }

            ConcurrentBag<Tuple<int, int, string, string>> result =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            int pages_progress = 0;

            Parallel.For(0, pages.Count,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer,
                    TaskScheduler = a_info.Server.Scheduler[Priority.Pages], 
                },
                (page, state) =>
                {
                    try
                    {
                        HtmlDocument page_doc = DownloadDocument(
                            a_info.Server, pages[page]);

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
                            string link = p.SelectSingleNode("div[2]/a[1]").GetAttributeValue("href", "");
                            string name = p.SelectSingleNode("div[1]").InnerText;

                            Tuple<int, int, string, string> s =
                                new Tuple<int, int, string, string>(page, index, name, link);

                            result.Add(s);

                            index += 1;
                        }

                        if (a_info.Token.IsCancellationRequested)
                        {
                            Loggers.Cancellation.InfoFormat(
                                "Pages - token cancelled, a_url: {0}",
                                a_info.URL);

                            a_info.Token.ThrowIfCancellationRequested();
                        }

                        pages_progress++;
                    }
                    catch
                    {
                        state.Break();
                        throw;
                    }
                });


            return from serie in result
                   orderby serie.Item1, serie.Item2
                   select new PageInfo(a_info, serie.Item4, result.IndexOf(serie) + 1, serie.Item3);
        }

        public override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var node = doc.DocumentNode.SelectSingleNode(
                "/html/body/div[4]/img");

            return GetServerURL() + node.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://www.mangarun.com/";
        }

        public override string GetChapterURL(ChapterInfo a_info)
        {
            return "http://www.mangarun.com/" + a_info.URLPart;
        }

        public override string GetSerieURL(SerieInfo a_info)
        {
            return "http://www.mangarun.com/" + a_info.URLPart;
        }

        public override string GetPageURL(PageInfo a_info)
        {
            return "http://www.mangarun.com/" + a_info.URLPart;
        }
    }
}
