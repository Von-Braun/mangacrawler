using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MangaCrawlerLib
{
    internal class OurMangaCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "OurManga";
            }
        }

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var series = doc.DocumentNode.SelectNodes("//div[@class='m_s_title']/a");

            foreach (var serie in series)
            {
                yield return new SerieInfo()
                {
                    ServerInfo = a_info,
                    Name = serie.InnerText,
                    URLPart = serie.GetAttributeValue("href", "").RemoveFromRight(1)
                };
            }
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var chapters = doc.DocumentNode.SelectNodes("//div[@class='manga_naruto_title']/a").AsEnumerable();

            if (chapters == null)
                yield break;

            chapters = from ch in chapters
                       where ch.ParentNode.ParentNode.ChildNodes[5].InnerText != "Soon!"
                       select ch;

            foreach (var chapter in chapters)
            {
                yield return new ChapterInfo()
                {
                    SerieInfo = a_info,
                    Name = chapter.InnerText,
                    URLPart = chapter.GetAttributeValue("href", "")
                };
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var url = doc.DocumentNode.SelectSingleNode("//div[@id='Summary']/p[2]/a[2]");

            doc = new HtmlWeb().Load(url.GetAttributeValue("href", ""));

            var pages = doc.DocumentNode.SelectNodes("//div[@class='inner_heading_right']/h3/select[2]/option");

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
                    URLPart = page.GetAttributeValue("value", ""),
                    Name = page.NextSibling.InnerText
                };

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.ChapterInfo.URLPart + "/" + a_info.URLPart);

            var node = doc.DocumentNode.SelectSingleNode("//div[@class='inner_full_view']/h3/a/img");

            if (node == null)
                node = doc.DocumentNode.SelectSingleNode("//div[@class='inner_full_view']/h3/img");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.ourmanga.com/directory/";
        }
    }
}
