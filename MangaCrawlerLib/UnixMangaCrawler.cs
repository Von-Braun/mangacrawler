using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace MangaCrawlerLib
{
    internal class UnixMangaCrawler : Crawler
    {
        private volatile int m_progress = 0;

        internal override string Name
        {
            get 
            {
                return "UnixManga";
            }
        }

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var series = doc.DocumentNode.SelectNodes("/html/body/center/div/div[2]/div/div[2]/table/tr/td/a");

            List<SerieInfo> list = new List<SerieInfo>();

            foreach (var serie in series)
            {
                list.Add(new SerieInfo()
                {
                    Name = serie.GetAttributeValue("title", ""),
                    URLPart = serie.GetAttributeValue("href", ""),
                    ServerInfo = a_info
                });
            }

            return list;
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var chapters1 = doc.DocumentNode.SelectNodes("/html/body/center/div/div[2]/div/div[2]/table/tr/td/a");

            List<ChapterInfo> list = new List<ChapterInfo>();

            if (chapters1 == null)
            {
                List<PageInfo> pages = DownloadPages(new ChapterInfo()
                {
                    Name = a_info.Name,
                    URLPart = a_info.URLPart,
                    SerieInfo = a_info
                }).ToList();

                if (pages.Count != 0)
                {
                    list.Add(new ChapterInfo()
                    {
                        Name = "(no chapters - single serie)",
                        URLPart = a_info.URLPart,
                        SerieInfo = a_info

                    });
                }

                return list;
            }

            foreach (var chapter1 in chapters1.Skip(3).Reverse().Skip(1).Reverse())
            {
                if (chapter1.GetAttributeValue("title", "") == "Thumbs.jpg")
                    continue;

                list.Add(new ChapterInfo()
                {
                    URLPart = chapter1.GetAttributeValue("href", ""),
                    Name = chapter1.GetAttributeValue("title", ""),
                    SerieInfo = a_info
                });
            }

            if (list.First().Pages.Count == 0)
            {
                List<ChapterInfo> volumes = new List<ChapterInfo>(list);
                list.Clear();

                m_progress = 0;

                ConcurrentBag<Tuple<int, int, ChapterInfo>> tuples = new ConcurrentBag<Tuple<int, int, ChapterInfo>>();

                Parallel.ForEach(volumes, volume =>
                {
                    var chapters2 = DownloadChapters(new SerieInfo()
                    {
                        Name = volume.Name,
                        URLPart = volume.URLPart,
                        ServerInfo = a_info.ServerInfo
                    }, a_progress_callback).ToList();

                    foreach (var chapter2 in chapters2)
                    {
                        chapter2.Name = volume.Name + " - " + chapter2.Name;
                        tuples.Add(new Tuple<int, int, ChapterInfo>(volumes.IndexOf(volume), chapters2.IndexOf(chapter2), chapter2));
                    }

                    m_progress++;
                    a_progress_callback(m_progress * 100 / volumes.Count);
                });

                list = (from tuple in tuples
                        orderby tuple.Item1, tuple.Item2
                        select tuple.Item3).ToList();
            }

            return list;
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var pages = doc.DocumentNode.SelectNodes("/html/body/center/div/div[2]/div/fieldset/ul/label/a");

            if (pages == null)
                yield break;

            a_info.PagesCount = pages.Count;

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo()
                {
                    ChapterInfo = a_info,
                    Index = index,
                    URLPart = page.GetAttributeValue("href", ""),
                    Name = Path.GetFileNameWithoutExtension(page.InnerText)
                };

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            string script = doc.DocumentNode.SelectSingleNode("/html/body/div/table/tr[2]/td/div[2]/table/tr/td/center/script").InnerText;

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
