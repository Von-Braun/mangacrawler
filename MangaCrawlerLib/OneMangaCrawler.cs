﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MangaCrawlerLib
{
    internal class OneMangaCrawler : Crawler
    {
        internal override string Name
        {
            get 
            {
                return "OneManga";
            }
        }

        internal override IEnumerable<SerieInfo> DownloadSeries(ServerInfo a_info, Action<int> a_progress_callback)
        {
            HtmlAgilityPack.HtmlDocument doc = doc = new HtmlWeb().Load(a_info.URL);

            var series = doc.DocumentNode.SelectNodes("//table[@class='ch-table']/tr/td[1]/a");

            foreach (var serie in series.Skip(2))
            {
                yield return new SerieInfo()
                {
                    ServerInfo = a_info,
                    Name = serie.InnerText,
                    URLPart = serie.GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1)
                };
            }
            
        }

        internal override IEnumerable<ChapterInfo> DownloadChapters(SerieInfo a_info, Action<int> a_progress_callback)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var chapters = doc.DocumentNode.SelectNodes("//table[@class='ch-table']/tr/td[1]/a");

            if (chapters == null)
                yield break;

            foreach (var chapter in chapters)
            {
                yield return new ChapterInfo()
                {
                    SerieInfo = a_info,
                    Name = chapter.InnerText,
                    URLPart = chapter.GetAttributeValue("href", "").RemoveFromLeft(1).RemoveFromRight(1)
                };
            }
        }

        internal override IEnumerable<PageInfo> DownloadPages(ChapterInfo a_info)
        {
            a_info.DownloadedPages = 0;

            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

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
                    URLPart = page.GetAttributeValue("value", ""),
                    Name = page.NextSibling.InnerText
                };

                yield return pi;
            }
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load(a_info.URL);

            var node = doc.DocumentNode.SelectSingleNode("/html/body/div/div[3]/div/div[4]/a/img");

            return node.GetAttributeValue("src", "");
        }

        internal override string GetServerURL()
        {
            return "http://www.onemanga.com/directory/";
        }

        internal override string GetSerieURL(SerieInfo a_info)
        {
            return "http://www.onemanga.com/" + a_info.URLPart;
        }

        internal override string GetChapterURL(ChapterInfo a_info)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlWeb().Load("http://www.onemanga.com/" + a_info.URLPart + "/");

            var url = doc.DocumentNode.SelectSingleNode("/html/body/div[2]/div[3]/div/ul/li/a");

            return "http://www.onemanga.com/" + url.GetAttributeValue("href", "").RemoveFromLeft(1);
        }

        internal override string GetPageURL(PageInfo a_info)
        {
            return String.Format("http://www.onemanga.com/{0}/{1}", a_info.ChapterInfo.URLPart, a_info.URLPart);
        }
    }
}