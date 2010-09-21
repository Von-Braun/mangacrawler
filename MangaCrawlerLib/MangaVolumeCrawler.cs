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

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var numbers = doc.DocumentNode.SelectNodes("//ul[@id='pagination']/li[@class='current']");
            var number = numbers.Count;

            ConcurrentBag<Tuple<int, int, string, string>> series =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            m_progress = 0;

            Parallel.For(1, number + 1, (page) =>
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

                m_progress++;
                a_progress_callback(m_progress * 100 / number);
            });

            var sorted_series = from serie in series
                                orderby serie.Item1, serie.Item2
                                select serie;

            foreach (var serie in sorted_series)
            {
                yield return new SerieInfo(a_info, serie.Item4, serie.Item3);
            }
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("//table[@id='series_list']/tr/td[1]/a");

            foreach (var chapter in chapters)
            {
                yield return new ChapterInfo(a_info, 
                    chapter.GetAttributeValue("href", "").RemoveFromLeft(1), chapter.InnerText);
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var pages = doc.DocumentNode.SelectNodes("//select[@id='id_page_select']/option");

            a_info.PagesCount = pages.Count;

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

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

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
