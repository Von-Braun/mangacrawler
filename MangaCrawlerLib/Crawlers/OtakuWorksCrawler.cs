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
        private int m_series_progress;

        internal override string Name
        {
            get 
            {
                return "Otaku Works";
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info);

            var numbers = doc.DocumentNode.SelectSingleNode("//div[@class='pagenav']").SelectNodes("div/a");
            var number = Int32.Parse(numbers.Reverse().Take(2).Last().InnerText);

            ConcurrentBag<Tuple<int, int, string, string>> series =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            m_series_progress = 0;

            Parallel.For(1, number + 1, (page, state) =>
            {
                try
                {
                    page = number + 1 - page;

                    HtmlDocument page_doc = ConnectionsLimiter.DownloadDocument(a_info, "http://www.otakuworks.com/manga/" + page);

                    var page_series = page_doc.DocumentNode.SelectNodes("//div[@id='subframe']/table/tr/td[@class='box3']/a");

                    int index = 0;

                    foreach (var serie in page_series)
                    {
                        Tuple<int, int, string, string> s =
                            new Tuple<int, int, string, string>(page, index++, serie.InnerText,
                                                                serie.GetAttributeValue("href", ""));

                        series.Add(s);
                    }

                    var result = (from serie in series
                                  orderby serie.Item1, serie.Item2
                                  select new SerieInfo(a_info, serie.Item4, serie.Item3)).ToArray();

                    m_series_progress++;
                    a_progress_callback(m_series_progress * 100 / number, result);
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

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var read = doc.DocumentNode.SelectSingleNode("//div[@id='filelist']/div[3]/a");

            string url = "http://www.otakuworks.com/" + read.GetAttributeValue("href", "");

            doc = ConnectionsLimiter.DownloadDocument(a_info, a_token, url);

            int pages = Int32.Parse(
                doc.DocumentNode.SelectSingleNode("//select[@id='fpage1']").ParentNode.ChildNodes.Reverse().ElementAt(3).InnerText);

            for (int index=1; index<=pages; index++)
            {
                PageInfo pi = new PageInfo(a_info, url + "/" + index, index);

                yield return pi;
            }  
        }

        internal override string GetImageURL(PageInfo a_info, CancellationToken a_token)
        {
            HtmlDocument doc = ConnectionsLimiter.DownloadDocument(a_info, a_token);

            var node = doc.DocumentNode.SelectSingleNode("//div[@id='filelist']/a/img");

            if (node == null)
                node = doc.DocumentNode.SelectSingleNode("//div[@id='filelist']/img");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetChapterURL(ChapterInfo a_info)
        {
            return "http://www.otakuworks.com" + a_info.URLPart;
        }

        internal override string GetServerURL()
        {
            return "http://www.otakuworks.com/manga";
        }
    }
}
