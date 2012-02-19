using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    internal class MangaShareCrawler : Crawler
    {
        public override string Name
        {
            get
            {
                return "Manga Share";
            }
        }

        public override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var series = doc.DocumentNode.SelectNodes("//table[@class='datalist']/tr[@class='datarow']");

            var result = from serie in series 
                         select new SerieInfo(a_info, 
                                              "http://read.mangashare.com/" + 
                                                  serie.SelectSingleNode("td[@class='datarow-0']/a").
                                                  GetAttributeValue("href", "").Split(new char[] { '/' }).Last(), 
                                              serie.SelectSingleNode("td[@class='datarow-1']/text()").InnerText);

            a_progress_callback(100, result);
        }

        public override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            string url = String.Format("{0}/chapter-001/page001.html", a_info.URL);
            HtmlDocument doc = DownloadDocument(a_info);

            var chapters = doc.DocumentNode.SelectNodes("//table[@class='datalist']/tr/td[4]/a");

            var result = from chapter in chapters 
                         select new ChapterInfo(a_info, chapter.GetAttributeValue("href", ""),
                             chapter.ParentNode.ParentNode.ChildNodes[3].InnerText);

            a_progress_callback(100, result);
        }

        public override IEnumerable<PageInfo> DownloadPages(TaskInfo a_info)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            var pages = doc.DocumentNode.SelectNodes("//select[@name='pagejump']/option");

            int index = 0;
            foreach (var page in pages)
            {
                index++;

                string link = page.GetAttributeValue("Value", "");
                int page_index = link.LastIndexOf("/page");
                link = link.Left(page_index + 5);
                link += a_info.URL + ".html";

                PageInfo pi = new PageInfo(a_info, link, index);

                yield return pi;
            }
        }

        public override string GetImageURL(PageInfo a_info)
        {
            HtmlDocument doc = DownloadDocument(a_info);

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@id='page']/a/img");

            if (node != null)
                return node.GetAttributeValue("src", "");

            return doc.DocumentNode.SelectSingleNode("//div[@id='page']/img").GetAttributeValue("src", "");
        }

        public override string GetServerURL()
        {
            return "http://read.mangashare.com/dir";
        }
    }
}
