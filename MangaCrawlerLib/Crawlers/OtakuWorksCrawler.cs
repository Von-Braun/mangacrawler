using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    internal class OtakuWorksCrawler : Crawler
    {
        public override string Name
        {
            get 
            {
                return "Otaku Works";
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var numbers = doc.DocumentNode.SelectSingleNode("//div[@class='pagenav']").SelectNodes("div/a");
            var number = Int32.Parse(numbers.Reverse().Take(2).Last().InnerText);

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

            Parallel.For(1, number + 1,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer,
                    TaskScheduler = a_server.Scheduler[Priority.Series], 
                },
                (page, state) =>
            {
                try
                {
                    page = number + 1 - page;

                    HtmlDocument page_doc = DownloadDocument(a_server, 
                        "http://www.otakuworks.com/manga/" + page);

                    var page_series = page_doc.DocumentNode.SelectNodes("//div[@id='subframe']/div").
                        Where(n => n.GetAttributeValue("class", "").StartsWith("clchild")).
                        Select(n => n.SelectSingleNode("div/a"));

                    int index = 0;

                    foreach (var serie in page_series)
                    {
                        Tuple<int, int, string, string> s =
                            new Tuple<int, int, string, string>(page, index++, serie.InnerText,
                                                                serie.GetAttributeValue("href", ""));

                        series.Add(s);
                    }

                    Interlocked.Increment(ref series_progress);
                    update(series_progress * 100 / number);
                }
                catch
                {
                    state.Break();
                    throw;
                }
            });

            update(100);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var page_chapters = doc.DocumentNode.SelectNodes("//div[@id='filelist']/div[@class='sbox3']/a");

            if (page_chapters != null)
            {
                var result = from chapter in page_chapters
                                select new Chapter(
                                    a_serie, 
                                    "http://www.otakuworks.com" + chapter.GetAttributeValue("href", ""), 
                                    chapter.InnerText);

                a_progress_callback(100, result);
            }
            else
                a_progress_callback(100, new List<Chapter>());
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);
            var pages = Int32.Parse(doc.DocumentNode.SelectSingleNode("//select[@id='fpage1']/../strong").InnerText);

            for (int i = 1; i <= pages; i++)
            {
                Page pi = new Page(
                    a_chapter,
                    a_chapter.URL + "/" + i.ToString(), 
                    i);

                yield return pi;
            }
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);

            var node = doc.DocumentNode.SelectSingleNode("//div[@id='filelist']/a/img");

            if (node == null)
                node = doc.DocumentNode.SelectSingleNode("//div[@id='filelist']/img");

            return node.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://www.otakuworks.com/manga";
        }
    }
}
