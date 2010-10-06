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

namespace MangaCrawlerLib
{
    internal class MangaVolumeCrawler : Crawler
    {
        private int m_progress;

        internal override string Name
        {
            get 
            {
                return "MangaVolume";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var numbers = doc.DocumentNode.SelectNodes("//ul[@id='pagination']/li[@class='current']");
            var number = numbers.Count;

            ConcurrentBag<Tuple<int, int, string, string>> series =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            m_progress = 0;

            Parallel.For(1, number + 1, (page, state) =>
            {
                try
                {
                    HtmlDocument page_doc;

                    if (page == 1)
                    {
                        page_doc = doc;
                    }
                    else
                    {
                        String url = "http://www.mangavolume.com" +
                            numbers[page - 1].ChildNodes[0].GetAttributeValue("href", "");

                        page_doc = ConnectionsLimiter.DownloadDocument(a_info, url);
                    }

                    var page_series = page_doc.DocumentNode.SelectNodes("//table[@id='series_list']/tr/td[1]/a");

                    int index = 0;
                    foreach (var serie in page_series)
                    {
                        if (serie.ParentNode.ParentNode.ChildNodes[3].InnerText == "0")
                            continue;

                        Tuple<int, int, string, string> s =
                            new Tuple<int, int, string, string>(page, index++, serie.InnerText,
                                                                serie.GetAttributeValue("href", "").RemoveFromLeft(1));

                        series.Add(s);
                    }

                    var result = from serie in series
                                 orderby serie.Item1, serie.Item2
                                 select new SerieInfo(a_info, serie.Item4, serie.Item3);

                    m_progress++;
                    a_progress_callback(m_progress * 100 / number, result);
                }
                catch
                {
                    state.Break();
                    throw;
                }
            });
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("//table[@id='series_list']/tr/td[1]/a");

            var result = from chapter in chapters
                         select new ChapterInfo(a_info, chapter.GetAttributeValue("href", "").RemoveFromLeft(1), chapter.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var pages = doc.DocumentNode.SelectNodes("//select[@id='id_page_select']/option");

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo(
                    a_info,
                    String.Format("http://www.mangavolume.com/{0}index.php?serie={1}&page_nr={2}",
                        a_info.URLPart, a_info.URLPart.Replace("/chapter-", "&chapter=").RemoveFromRight(1), page.GetAttributeValue("value", "")),
                    index);

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var node = doc.DocumentNode.SelectSingleNode("//img[@id='mangaPage']");

            return node.GetAttributeValue("src", "");
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
