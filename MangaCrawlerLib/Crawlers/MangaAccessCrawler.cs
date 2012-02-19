using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    internal class MangaAccessCrawler : Crawler
    {
        public override string Name
        {
            get 
            {
                return "Manga Access";
            }
        }

        public override void DownloadSeries(ServerInfo a_info, Action<int, 
            IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes(
                "/html/body/div/div[2]/table/tr/td[2]/div[3]/div/div[@class='c_h2b' or @class='c_h2']/a");

            var result = from serie in series
                         select new SerieInfo(a_info,
                                              "http://manga-access.com" + serie.GetAttributeValue("href", ""),
                                              serie.InnerText);

            a_progress_callback(100, result);
        }

        public override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes(
                "/html/body/div/div[2]/table/tr/td[2]/div[3]/div/div[@class='episode c_h2b' or @class='episode c_h2']/div/a");

            var result = from chapter in chapters
                         select new ChapterInfo(a_info, 
                                                "http://manga-access.com" + chapter.GetAttributeValue("href", ""), 
                                                chapter.InnerText);

            a_progress_callback(100, result);
        }

        public override IEnumerable<PageInfo> DownloadPages(TaskInfo a_info)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var pages = doc.DocumentNode.SelectNodes("//select[@id='page_switch']/option");

            return from page in pages
                   select new PageInfo(a_info, 
                                       "http://manga-access.com" + page.GetAttributeValue("value", ""), pages.IndexOf(page) + 1,
                                       page.NextSibling.InnerText);
        }

        public override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = DownloadDocument(a_info);
            
            var image = doc.DocumentNode.SelectSingleNode("//div[@id='pic']/img");

            return image.GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://www.manga-access.com/manga/list";
        }
    }
}
