using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;

namespace MangaCrawlerLib
{
    internal class MangaRunCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "MangaRun";
            }
        }

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var series = doc.DocumentNode.SelectNodes("/html/body/div[2]/table/tr/td/div/a");

            foreach (var serie in series)
            {
                yield return new SerieInfo()
                {
                    ServerInfo = a_info,
                    Name = serie.InnerText,
                    URLPart = GetServerURL() + serie.GetAttributeValue("href", "").RemoveFromLeft(1)
                };
            }
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var chapters = doc.DocumentNode.SelectNodes("/html/body/div[2]/table/tr/td/div/a");

            foreach (var chapter in chapters)
            {
                yield return new ChapterInfo()
                {
                    SerieInfo = a_info,
                    Name = chapter.InnerText,
                    URLPart = GetServerURL() + chapter.GetAttributeValue("href", "").RemoveFromLeft(1)
                };
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var pages = doc.DocumentNode.SelectNodes("/html/body/div[2]/table/tr/td/div/a");

            a_info.PagesCount = pages.Count;

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo()
                {
                    ChapterInfo = a_info,
                    Index = index,
                    URLPart = GetServerURL() + page.GetAttributeValue("href", "").RemoveFromLeft(1),
                    Name = Path.GetFileNameWithoutExtension(page.InnerText)
                };

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var node = doc.DocumentNode.SelectSingleNode("/html/body/div[2]/table/tr/td[2]/div/img");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.mangarun.com/";
        }
    }
}
