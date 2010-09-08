using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using HtmlAgilityPack;
using System.Threading.Tasks;

namespace MangaCrawlerLib
{
    internal class OtakuWorksCrawler : Crawler
    {
        private int m_progress;

        internal override string Name
        {
            get 
            {
                return "Otaku Works";
            }
        }

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var numbers = doc.DocumentNode.SelectNodes("/html/body/div/div/div[5]/div[3]/div/a");
            var number = Int32.Parse(numbers.Reverse().Take(2).Last().InnerText);

            ConcurrentBag<Tuple<int, int, string, string>> series =
                new ConcurrentBag<Tuple<int, int, string, string>>();

            m_progress = 0;

            Parallel.For(1, number + 1, (page) =>
            {
                HtmlDocument page_doc = new HtmlWeb().Load("http://www.otakuworks.com/manga/" + page);

                var page_series = page_doc.DocumentNode.SelectNodes("/html/body/div/div/div[5]/table/tr/td[@class='box3']/a");

                int index = 0;
                foreach (var serie in page_series)
                {
                    Tuple<int, int, string, string> s =
                        new Tuple<int, int, string, string>(page, index++, serie.InnerText,
                                                            serie.GetAttributeValue("href", ""));

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
                yield return new SerieInfo()
                {
                    Name = serie.Item3,
                    URLPart = serie.Item4,
                    ServerInfo = a_info
                };
            }
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var pages = doc.DocumentNode.SelectNodes("/html/body/div/div/div[5]/div/div[3]/div[10]/div[27]/div/a").AsEnumerable();

            if (pages == null)
            {
                HtmlDocument page_doc = new HtmlWeb().Load(a_info.URL);

                var page_chapters = page_doc.DocumentNode.SelectNodes("/html/body/div/div/div[5]/div/div[3]/div/div[@class='sbox3']/a[1]");

                foreach (var chapter in page_chapters)
                {
                    yield return new ChapterInfo()
                    {
                        Name = chapter.InnerText,
                        URLPart = chapter.GetAttributeValue("href", ""),
                        SerieInfo = a_info
                    };
                }
            }
            else
            {
                pages = pages.Reverse().Skip(1).Reverse();

                ConcurrentBag<Tuple<int, int, string, string>> chapters =
                    new ConcurrentBag<Tuple<int, int, string, string>>();

                m_progress = 0;

                Parallel.ForEach(pages, (page) =>
                {
                    int page_num = Int32.Parse(page.InnerText);

                    String url = "http://www.otakuworks.com/" + page.GetAttributeValue("href", "").RemoveFromLeft(1);

                    if (page_num == 1)
                        url = a_info.URL;

                    HtmlDocument page_doc = new HtmlWeb().Load(url);

                    var page_chapters = page_doc.DocumentNode.SelectNodes("/html/body/div/div/div[5]/div/div[3]/div/div[@class='sbox3']/a[1]");

                    int index = 0;
                    foreach (var chapter in page_chapters)
                    {
                        Tuple<int, int, string, string> s =
                            new Tuple<int, int, string, string>(page_num, index++, chapter.InnerText,
                                                                chapter.GetAttributeValue("href", ""));

                        chapters.Add(s);
                    }

                    m_progress++;
                    a_progress_callback(m_progress * 100 / pages.Count());
                });

                var sorted_chapters = from chapter in chapters
                                      orderby chapter.Item1, chapter.Item2
                                      select chapter;

                foreach (var serie in sorted_chapters)
                {
                    yield return new ChapterInfo()
                    {
                        Name = serie.Item3,
                        URLPart = serie.Item4,
                        SerieInfo = a_info
                    };
                }
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            int pages = Int32.Parse(
                doc.DocumentNode.SelectSingleNode("//select[@id='fpage1']").ParentNode.ChildNodes.Reverse().ElementAt(3).InnerText);

            a_info.PagesCount = pages;

            for (int index=1; index<=pages; index++)
            {
                PageInfo pi = new PageInfo()
                {
                    ChapterInfo = a_info,
                    Index = index,
                    URLPart = a_info.URL + "/" + index
                };

                yield return pi;
            }  
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var node = doc.DocumentNode.SelectSingleNode("//div[@id='filelist']/a/img");

            if (node != null)
                return node.GetAttributeValue("src", "");

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
