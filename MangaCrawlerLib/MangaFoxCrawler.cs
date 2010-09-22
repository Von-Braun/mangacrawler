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
    internal class MangaFoxCrawler : Crawler
    {
        private volatile int m_progress;

        internal override string Name
        {
            get 
            {
                return "MangaFox";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var numbers = doc.DocumentNode.SelectNodes("/html/body/div[5]/div[3]/div[6]/ul/li/a");
            var number = Int32.Parse(numbers.Reverse().Take(2).Last().InnerText);

            ConcurrentBag<Tuple<int, int, string, string>> series = 
                new ConcurrentBag<Tuple<int, int, string, string>>();

            m_progress = 0;

            Parallel.For(1, number + 1, (page) =>
            {
                String url = String.Format("http://www.mangafox.com/directory/all/{0}.htm", page);

                HtmlDocument page_doc = ConnectionsLimiter.DownloadDocument(a_info, url);

                var page_series = page_doc.DocumentNode.SelectNodes("/html/body/div[5]/div[3]/table/tr/td[1]/a");

                int index = 0;
                foreach (var serie in page_series)
                {
                    Tuple<int, int, string, string> s = 
                        new Tuple<int, int, string, string>(page, index++, serie.InnerText, 
                                                            serie.GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1));

                    series.Add(s);
                }

                var result = from serie in series
                             orderby serie.Item1, serie.Item2
                             select new SerieInfo(a_info, serie.Item4, serie.Item3);

                m_progress++;
                a_progress_callback(m_progress * 100 / number, result);
            });
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("/html/body/div[5]/div[3]/table/tr/td/a[2]");

            var result = from chapter in chapters
                         select new ChapterInfo(a_info, 
                                                chapter.GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1), chapter.InnerText);

            a_progress_callback(100, result);            
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var pages = doc.DocumentNode.SelectSingleNode("//select[@class='middle']").SelectNodes("option");

            a_info.PagesCount = pages.Count;

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo(
                    a_info,
                    String.Format("http://www.mangafox.com/{0}/{1}.html", a_info.URLPart, page.GetAttributeValue("value", "")),
                    index);

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var node = doc.DocumentNode.SelectSingleNode("//img[@id='image']");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.mangafox.com/directory/all/1.htm";
        }

        internal override string GetSerieURL(SerieInfo a_info)
        {
            return "http://www.mangafox.com/" + a_info.URLPart;
        }

        internal override string GetChapterURL(ChapterInfo a_info)
        {
            return "http://www.mangafox.com/" + a_info.URLPart;
        }
    }
}
