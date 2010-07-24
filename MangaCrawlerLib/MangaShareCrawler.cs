using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MangaCrawlerLib
{
    internal class MangaShareCrawler : Crawler
    {
        internal override string Name
        {
            get
            {
                return "MangaShare";
            }
        }

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var series = doc.DocumentNode.SelectNodes("//table[@class='datalist']/tr[@class='datarow']");

            foreach (var serie in series)
            {
                yield return new SerieInfo()
                {
                    Name = serie.SelectSingleNode("td[@class='datarow-1']/text()").InnerText,
                    URLPart = serie.SelectSingleNode("td[@class='datarow-0']/a").GetAttributeValue("href", "").Split(new char[] { '/' }).Last(),
                    ServerInfo = a_info
                };
            }
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            string url = String.Format("{0}/chapter-001/page001.html", a_info.URL);
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(url);

            var chapters = doc.DocumentNode.SelectNodes("//select[@name='chapterjump']/option");

            if (chapters == null)
                yield break;

            foreach (var chapter in chapters)
            {
                yield return new ChapterInfo()
                {
                    URLPart = chapter.GetAttributeValue("Value", ""),
                    Name = chapter.NextSibling.InnerText,
                    SerieInfo = a_info
                };
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var pages = doc.DocumentNode.SelectNodes("//select[@name='pagejump']/option");

            if (pages == null)
                yield break;

            a_info.PagesCount = pages.Count;

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                PageInfo pi = new PageInfo()
                {
                    URLPart = page.GetAttributeValue("Value", ""),
                    Index = index,
                    ChapterInfo = a_info
                };

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@id='page']/a/img");

            if (node != null)
                return node.GetAttributeValue("src", "");

            return doc.DocumentNode.SelectSingleNode("//div[@id='page']/img").GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://read.mangashare.com/dir";
        }

        internal override string GetSerieURL(SerieInfo a_info)
        {
            return "http://read.mangashare.com/" + a_info.URLPart;
        }

        internal override string GetChapterURL(ChapterInfo a_info)
        {
            return String.Format("http://read.mangashare.com/{0}/chapter-{1}/page001.html",
                a_info.SerieInfo.URLPart, a_info.URLPart);
        }

        internal override string GetPageURL(PageInfo a_info)
        {
            return String.Format("http://read.mangashare.com/{0}/chapter-{1}/page{2}.html",
                                 a_info.ChapterInfo.SerieInfo.URLPart, a_info.ChapterInfo.URLPart, a_info.URLPart);
        }
    }
}
