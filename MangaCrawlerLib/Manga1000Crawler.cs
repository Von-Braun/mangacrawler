using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MangaCrawlerLib
{
    internal class Manga1000Crawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "1000Manga";
            }
        }

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var series = doc.DocumentNode.SelectNodes("//table[@class='ch-table']/tr/td[1]/a");

            foreach (var serie in series.Skip(2))
            {
                yield return new SerieInfo()
                {
                    ServerInfo = a_info,
                    Name = serie.InnerText,
                    URLPart = "http://www.1000manga.com/" + serie.GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1)
                };
            }
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var chapters = doc.DocumentNode.SelectNodes("//table[@class='ch-table mb20']/tr/td[1]/a");

            if (chapters == null)
                yield break;

            foreach (var chapter in chapters)
            {
                yield return new ChapterInfo()
                {
                    SerieInfo = a_info,
                    Name = chapter.InnerText,
                    URLPart = "http://www.1000manga.com/" + chapter.GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1)
                };
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var chapter_link = doc.DocumentNode.SelectSingleNode("//div[@id='chapter-link']/a").
                GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1);

            doc = new HtmlWeb().Load("http://www.1000manga.com/" + chapter_link);

            var pages = doc.DocumentNode.SelectNodes("//select[@id='id_page_select']/option");

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
                    URLPart = a_info.URLPart + "/" + page.GetAttributeValue("value", ""),
                    Name = page.NextSibling.InnerText
                };

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var node = doc.DocumentNode.SelectSingleNode("//div[@class='one-page']/a/img");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.1000manga.com/directory/";
        }
    }
}
