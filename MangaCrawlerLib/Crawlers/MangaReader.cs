﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MangaCrawlerLib.Crawlers
{
    internal class MangaReader : Crawler
    {
        public override string Name
        {
            get 
            {
                return "Manga Reader";
            }
        }

        internal override void DownloadSeries(Server a_server, Action<int, IEnumerable<Serie>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_server);

            var series = doc.DocumentNode.SelectNodes(
                "//div[@class='series_alpha']//ul[@class='series_alpha']/li/a");

            var result = from serie in series
                         select new Serie(a_server, serie.GetAttributeValue("href", ""), serie.InnerText);

            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            HtmlDocument doc = DownloadDocument(a_serie);

            var chapters = doc.DocumentNode.SelectNodes("//table[@class='listing']/tr/td/a");

            var result = from chapter in chapters
                         select new Chapter(a_serie, chapter.GetAttributeValue("href", ""), chapter.InnerText);

            a_progress_callback(100, result);
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            HtmlDocument doc = DownloadDocument(a_chapter);

            var pages = doc.DocumentNode.SelectNodes("//div[@class='selectpage']/select/option");

            return from page in pages
                   select new Page(a_chapter, page.GetAttributeValue("value", ""), pages.IndexOf(page) + 1,
                                   page.NextSibling.InnerText);
        }

        public override string GetServerURL()
        {
            return "http://www.mangareader.net/alphabetical";
        }

        internal override string GetImageURL(Page a_page)
        {
            HtmlDocument doc = DownloadDocument(a_page);
            var image = doc.DocumentNode.SelectSingleNode("//div[@id='imgholder']/a/img");
            return image.GetAttributeValue("src", "");
        }
    }
}