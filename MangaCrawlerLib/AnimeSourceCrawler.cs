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

namespace MangaCrawlerLib
{
    internal class AnimeSourceCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "Anime-Source";
            }
        }

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var series = doc.DocumentNode.SelectNodes("/html/body/center/table/tr/td/table[5]/tr/td/table/tr/td/table/tr/td/table/tr/td[2]");

            List<SerieInfo> list = new List<SerieInfo>();

            foreach (var serie in series)
            {
                if (serie.ChildNodes[7].InnerText.Trim() == "2")
                    continue;

                list.Add(new SerieInfo()
                {
                    Name = serie.SelectSingleNode("font").FirstChild.InnerText,
                    URLPart = serie.SelectSingleNode("a[2]").GetAttributeValue("href", ""),
                    ServerInfo = a_info
                });
                
            }

            return list.OrderBy(s => s.Name).ToList();
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var chapters = doc.DocumentNode.SelectNodes("/html/body/center/table/tr/td/table[5]/tr/td/table/tr/td/table/tr/td/blockquote/a");

            // TODO: 
            //if (chapters == null)
            //    yield break;

            foreach (var chapter in chapters.Skip(1))
            {
                yield return new ChapterInfo()
                {
                    URLPart = chapter.GetAttributeValue("href", ""),
                    Name = chapter.InnerText, 
                    SerieInfo = a_info
                };
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var pages = doc.DocumentNode.SelectNodes("//select[@name='pageid']/option");

            if (pages == null)
            {
                string pages_str = doc.DocumentNode.SelectSingleNode(
                    "/html/body/center/table/tr/td/table[5]/tr/td/table/tr/td/table/tr/td/font[2]").ChildNodes[4].InnerText;

                a_info.PagesCount = Int32.Parse(pages_str.Split(new char[] { '/' }).Last());

                for (int page = 1; page <= a_info.PagesCount; page++)
                {
                    PageInfo pi = new PageInfo()
                    {
                        ChapterInfo = a_info,
                        Index = page,
                        URLPart = a_info.URLPart + "&page=" + page
                    };

                    yield return pi;
                }                
            }
            else
            {
                a_info.PagesCount = pages.Count;

                int index = 0;
                foreach (var page in pages)
                {
                    index++;

                    PageInfo pi = new PageInfo()
                    {
                        ChapterInfo = a_info,
                        Index = index,
                        URLPart = page.GetAttributeValue("value", "")
                    };

                    yield return pi;
                }
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            string xpath;
            if (a_info.ChapterInfo.PagesCount == a_info.Index)
                xpath = "/html/body/center/table/tr/td/table[5]/tr/td/div/img";
            else
                xpath = "/html/body/center/table/tr/td/table[5]/tr/td/div/a/img";

            var node = doc.DocumentNode.SelectSingleNode(xpath);

            if (node == null)
            {
                node = doc.DocumentNode.SelectSingleNode("/html/body/center/table/tr/td/table[5]/tr/td/table/tr/td/table/tr/td/font[2]/p[2]/img");

                if (node == null)
                    node = doc.DocumentNode.SelectSingleNode("/html/body/center/table/tr/td/table[5]/tr/td/table/tr/td/table/tr/td/font[2]/p/img");

                return node.GetAttributeValue("src", "");
            }
            else
                return "http://www.anime-source.com/" + node.GetAttributeValue("src", "").RemoveFromLeft(1);
        }

        internal override string GetServerURL()
        {
            return "http://www.anime-source.com/banzai/modules.php?name=Manga";
        }

        internal override string GetSerieURL(SerieInfo a_info)
        {
            return "http://www.anime-source.com/banzai/" + a_info.URLPart;
        }

        internal override string GetChapterURL(ChapterInfo a_info)
        {
            return "http://www.anime-source.com/banzai/" + a_info.URLPart;
        }

        internal override string GetPageURL(PageInfo a_info)
        {
            return "http://www.anime-source.com/banzai/" + a_info.URLPart;
        }
    }
}
