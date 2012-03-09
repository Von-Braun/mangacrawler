using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    internal class UnixMangaCrawler : Crawler
    {
        public override int MaxConnectionsPerServer
        {
            get
            {
                return 1;
            }
        }

        public override string Name
        {
            get 
            {
                return "Unix Manga";
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, 
            IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes(
                "/html/body/center/div/div[2]/div/div[2]/table/tr/td/a");

            var result = from serie in series
                         select new Serie(
                             a_server,
                             serie.GetAttributeValue("href", ""),
                             serie.GetAttributeValue("title", ""));

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, 
            IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters_or_volumes_enum =
                doc.DocumentNode.SelectNodes("//table[@class='snif']/tr/td/a");

            if (chapters_or_volumes_enum == null)
            {
                a_progress_callback(100, 
                    new [] { new Chapter(a_serie, a_serie.URL, a_serie.Title) } );
            }
            else
            {
                var chapters_or_volumes = 
                    chapters_or_volumes_enum.Skip(3).Reverse().Skip(1).Reverse().ToList();

                int chapters_progress = 0;

                ConcurrentBag<Tuple<int, int, Chapter>> chapters = 
                    new ConcurrentBag<Tuple<int, int, Chapter>>();

                Action<int> update = (progress) =>
                {
                    var result = from chapter in chapters
                                 orderby chapter.Item1, chapter.Item2
                                 select chapter.Item3;

                    a_progress_callback(progress, result);
                };

                Parallel.ForEach(chapters_or_volumes, 
                    new ParallelOptions() 
                    {
                        MaxDegreeOfParallelism = MaxConnectionsPerServer,
                        TaskScheduler = Limiter.Scheduler
                    },
                    (chapter_or_volume, state) =>
                {
                    try
                    {
                        doc = DownloadDocument(a_serie, 
                            chapter_or_volume.GetAttributeValue("href", ""));

                        var pages = doc.DocumentNode.SelectNodes(
                            "/html/body/center/div/div[2]/div/fieldset/ul/label/a");

                        if (pages != null)
                        {
                            chapters.Add(new Tuple<int, int, Chapter>(
                                chapters_or_volumes.IndexOf(chapter_or_volume),
                                0,
                                new Chapter(a_serie, chapter_or_volume.GetAttributeValue(
                                    "href", ""), chapter_or_volume.InnerText)
                            ));
                        }
                        else
                        {
                            if (doc.DocumentNode.InnerText.Contains("500 - Internal server error"))
                                return;

                            var chapters1 =
                                doc.DocumentNode.SelectNodes(
                                    "/html/body/center/div/div[2]/div/div[2]/table/tr/td/a").
                                        Skip(3).Reverse().Skip(1).Reverse().ToList();
                            if (chapters1[0].InnerText.ToLower() == "thumbs.jpg")
                                chapters1.RemoveAt(0);

                            foreach (var chapter in chapters1)
                            {
                                chapters.Add(new Tuple<int, int, Chapter>(
                                    chapters_or_volumes.IndexOf(chapter_or_volume),
                                    chapters1.IndexOf(chapter),
                                    new Chapter(a_serie, chapter.GetAttributeValue("href", ""), 
                                        chapter.InnerText)
                                ));
                            }
                        }

                        Interlocked.Increment(ref chapters_progress);
                        update(chapters_progress * 100 / chapters_or_volumes.Count);
                    }
                    catch
                    {
                        state.Break();
                        throw;
                    }
                });

                update(100);
            }
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var pages = doc.DocumentNode.SelectNodes(
                "/html/body/center/div/div[2]/div/fieldset/ul/label/a");

            if (pages == null)
                yield break;

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                Page pi = new Page(a_chapter, page.GetAttributeValue("href", ""), index, 
                    Path.GetFileNameWithoutExtension(page.InnerText));

                yield return pi;
            }
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);

            string script = doc.DocumentNode.SelectSingleNode("/html/body/div/table/tr[2]/td/div[2]/table/tr/td/center/script").InnerText;

            Regex regex1 = new Regex("([Ss][Rr][Cc])=\".*\"");
            Match m1 = regex1.Match(script);
            string str = m1.Value.RemoveFromLeft(5).RemoveFromRight(1);

            return str;
        }

        public override string GetServerURL()
        {
            return "http://unixmanga.com/onlinereading/manga-lists.html";
        }
    }
}
