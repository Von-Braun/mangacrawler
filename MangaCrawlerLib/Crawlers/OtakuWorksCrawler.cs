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
        internal override string Name
        {
            get 
            {
                return "Otakuworks";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var numbers = doc.DocumentNode.SelectSingleNode("//div[@class='pagenav']").SelectNodes("div/a");
            var number = Int32.Parse(numbers.Reverse().Take(2).Last().InnerText);

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

            Parallel.For(1, number + 1,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxConnectionsPerServer,
                    TaskScheduler = a_info.Scheduler[Priority.Series], 
                },
                (page, state) =>
            {
                try
                {
                    page = number + 1 - page;

                    HtmlDocument page_doc = ConnectionsLimiter.DownloadDocument(a_info, 
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

                    series_progress++;
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

        internal override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var page_chapters = doc.DocumentNode.SelectNodes("//div[@id='filelist']/div[@class='sbox3']/a");

            if (page_chapters != null)
            {
                var result = from chapter in page_chapters
                                select new ChapterInfo(a_info, chapter.GetAttributeValue("href", ""), chapter.InnerText);

                a_progress_callback(100, result);
            }
            else
                a_progress_callback(100, new List<ChapterInfo>());
        }

        internal override IEnumerable<PageInfo> DownloadPages(TaskInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);
            var pages = Int32.Parse(doc.DocumentNode.SelectSingleNode("//select[@id='fpage1']/../strong").InnerText);

            for (int i = 1; i <= pages; i++)
            {
                PageInfo pi = new PageInfo(a_info, a_info.URLPart + "/" + i.ToString(), i);
                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var node = doc.DocumentNode.SelectSingleNode("//div[@id='filelist']/a/img");

            if (node == null)
                node = doc.DocumentNode.SelectSingleNode("//div[@id='filelist']/img");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetChapterURL(ChapterInfo a_info)
        {
            return "http://www.otakuworks.com" + a_info.URLPart;
        }

        internal override string GetPageURL(PageInfo a_info)
        {
            return "http://www.otakuworks.com" + a_info.URLPart;
        }

        internal override string GetServerURL()
        {
            return "http://www.otakuworks.com/manga";
        }
    }
}
