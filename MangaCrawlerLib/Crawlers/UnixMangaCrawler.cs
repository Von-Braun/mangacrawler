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

namespace MangaCrawlerLib
{
    internal class UnixMangaCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "UnixManga";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, 
            IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes(
                "/html/body/center/div/div[2]/div/div[2]/table/tr/td/a");

            var result = from serie in series
                         select new SerieInfo(
                             a_info,
                             serie.GetAttributeValue("href", ""),
                             serie.GetAttributeValue("title", ""));

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, 
            IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters_or_volumes_enum =
                doc.DocumentNode.SelectNodes("//table[@class='snif']/tr/td/a");

            if (chapters_or_volumes_enum == null)
            {
                var pages = doc.DocumentNode.SelectNodes(
                    "/html/body/center/div/div[2]/div/fieldset/ul/label/a");

                a_progress_callback(100, 
                    new [] { new ChapterInfo(a_info, a_info.URL, a_info.Name) } );
            }
            else
            {
                var chapters_or_volumes = 
                    chapters_or_volumes_enum.Skip(3).Reverse().Skip(1).Reverse().ToList();

                int progress = 0;

                ConcurrentBag<Tuple<int, int, ChapterInfo>> chapters = 
                    new ConcurrentBag<Tuple<int, int, ChapterInfo>>();

                Parallel.ForEach(chapters_or_volumes, 
                    new ParallelOptions() { MaxDegreeOfParallelism = 1},
                    (chapter_or_volume, state) =>
                {
                    try
                    {
                        doc = ConnectionsLimiter.DownloadDocument(a_info, 
                            chapter_or_volume.GetAttributeValue("href", ""));

                        var pages = doc.DocumentNode.SelectNodes(
                            "/html/body/center/div/div[2]/div/fieldset/ul/label/a");

                        if (pages != null)
                        {
                            chapters.Add(new Tuple<int, int, ChapterInfo>(
                                chapters_or_volumes.IndexOf(chapter_or_volume),
                                0,
                                new ChapterInfo(a_info, chapter_or_volume.GetAttributeValue(
                                    "href", ""), chapter_or_volume.InnerText)
                            ));
                        }
                        else
                        {
                            var chapters1 =
                                doc.DocumentNode.SelectNodes(
                                    "/html/body/center/div/div[2]/div/div[2]/table/tr/td/a").
                                        Skip(3).Reverse().Skip(1).Reverse().ToList();
                            if (chapters1[0].InnerText.ToLower() == "thumbs.jpg")
                                chapters1.RemoveAt(0);

                            foreach (var chapter in chapters1)
                            {
                                chapters.Add(new Tuple<int, int, ChapterInfo>(
                                    chapters_or_volumes.IndexOf(chapter_or_volume),
                                    chapters1.IndexOf(chapter),
                                    new ChapterInfo(a_info, chapter.GetAttributeValue("href", ""), 
                                        chapter.InnerText)
                                ));
                            }
                        }

                        var result = from chapter in chapters
                                     orderby chapter.Item1, chapter.Item2
                                     select chapter.Item3;

                        progress++;
                        a_progress_callback(progress * 100 / chapters_or_volumes.Count, result);
                    }
                    catch
                    {
                        state.Break();
                        throw;
                    }
                });
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info, 
            CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var pages = doc.DocumentNode.SelectNodes(
                "/html/body/center/div/div[2]/div/fieldset/ul/label/a");

            if (pages == null)
                yield break;

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo(a_info, page.GetAttributeValue("href", ""), index, 
                    Path.GetFileNameWithoutExtension(page.InnerText));

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            string script = doc.DocumentNode.SelectSingleNode(
                "/html/body/div/table/tr[2]/td/div[2]/table/tr/td/center/script").InnerText;

            Regex regex1 = new Regex("([Ss][Rr][Cc])=\".*\"");
            Match m1 = regex1.Match(script);
            string str = m1.Value.RemoveFromLeft(5).RemoveFromRight(1);

            return str;
        }

        internal override string GetServerURL()
        {
            return "http://unixmanga.com/onlinereading/manga-lists.html";
        }
    }
}
