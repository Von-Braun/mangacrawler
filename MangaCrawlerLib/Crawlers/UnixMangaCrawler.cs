﻿using System;
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

namespace MangaCrawlerLib.Crawlers
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
                "//div/div/table/tr/td/a");

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
                return;
            }

            int chapters_progress = 0;

            ConcurrentBag<Tuple<int, int, Chapter>> chapters = 
                new ConcurrentBag<Tuple<int, int, Chapter>>();

            var chapters_or_volumes =
                chapters_or_volumes_enum.Skip(3).Reverse().Skip(1).Reverse().ToList();

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
                    MaxDegreeOfParallelism = 1, //MaxConnectionsPerServer,
                    TaskScheduler = Limiter.Scheduler
                },
                (chapter_or_volume, state) =>
            {
                try
                {
                    List<Chapter> result = SearchChaptersOrVolumes(a_serie, chapter_or_volume);

                    foreach (var ch in result)
                    {
                        chapters.Add(new Tuple<int, int, Chapter>(
                            chapters_or_volumes.IndexOf(chapter_or_volume),
                            result.IndexOf(ch),
                            ch
                        ));
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

            if (chapters.Count == 0)
                throw new Exception("Serie has no chapters");
        }

        private List<Chapter> SearchChaptersOrVolumes(Serie a_serie, HtmlNode a_chapter_or_volume)
        {
            HtmlDocument doc = DownloadDocument(a_serie,
                        a_chapter_or_volume.GetAttributeValue("href", ""));

            List<Chapter> result = new List<Chapter>();

            if (doc.DocumentNode.InnerText.Contains("500 - Internal server error"))
                return result;

            var chapter = doc.DocumentNode.SelectNodes(
                "/html/body/center/div/div[2]/div/fieldset/ul/label/a");

            if (chapter != null)
            {
                result.Add(new Chapter(a_serie, a_chapter_or_volume.GetAttributeValue(
                    "href", ""), a_chapter_or_volume.InnerText));
            }
            else
            {
                var chapters_or_volumes = doc.DocumentNode.SelectNodes(
                    "//tr[@class='snF snEven' or @class='snF snOdd']/td/a").Skip(1).ToList();
                if (chapters_or_volumes[0].InnerText.ToLower() == "thumbs.jpg")
                    chapters_or_volumes.RemoveAt(0);

                foreach (var chapter_or_volume in chapters_or_volumes)
                    result.AddRange(SearchChaptersOrVolumes(a_serie, chapter_or_volume));
            }

            return result;
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
            var nodes = doc.DocumentNode.SelectNodes("//div[@id='contentRH']/div/script");

            foreach (var node in nodes)
            {
                string script = node.InnerText;
                string url = Regex.Match(script, ".*SRC=\"(.*)\".*").Groups[1].Value;

                if (!String.IsNullOrWhiteSpace(url))
                    return url;

            }

            throw new InvalidDataException();
        }

        public override string GetServerURL()
        {
            return "http://unixmanga.com/onlinereading/manga-lists.html";
        }
    }
}
