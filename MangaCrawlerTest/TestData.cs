﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MangaCrawlerLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TomanuExtensions;
using TomanuExtensions.Utils;

namespace MangaCrawlerTest
{
    public class PageTestData
    {
        public ChapterTestData ChapterTestData { get; set; }

        public Page Page { get; set; }

        public int Index { get; set; }

        public string Name { get; set; }

        public Image Image { get; set; }

        public string URL { get; set; }

        public string ImageURL { get; set; }

        public string FileName
        {
            get
            {
                var file_name = String.Format("{0} - {1} - {2} - {3}{4}", 
                                    ChapterTestData.SerieTestData.ServerTestData.Name,
                                    ChapterTestData.SerieTestData.Title, 
                                    ChapterTestData.Title, 
                                    Name, 
                                    Path.GetExtension(ImageURL));
                file_name = TomanuExtensions.Utils.FileUtils.RemoveInvalidFileCharacters(file_name);
                file_name = Path.Combine(Helpers.GetTestDataDir(), file_name);
                return file_name;
            }
        }

        public string Hash { get; set; }

        public static PageTestData Load(XElement a_node)
        {
            PageTestData ptd = new PageTestData();

            ptd.Name = a_node.Element("Name").Value;
            ptd.Hash = a_node.Element("Hash").Value;
            ptd.URL = a_node.Element("URL").Value;
            ptd.ImageURL = a_node.Element("ImageURL").Value;
            ptd.Index = Int32.Parse(a_node.Element("Index").Value);

            Assert.IsTrue(!String.IsNullOrWhiteSpace(ptd.Name));
            Assert.IsTrue(!String.IsNullOrWhiteSpace(ptd.Hash));
            Assert.IsTrue(!String.IsNullOrWhiteSpace(ptd.URL));
            Assert.IsTrue(!String.IsNullOrWhiteSpace(ptd.ImageURL));

            return ptd;
        }

        
        public XElement GetAsXml()
        {
            return new XElement("PageTestData",
                new XElement("Name", Name),
                new XElement("Hash", Hash),
                new XElement("URL", URL),
                new XElement("ImageURL", ImageURL),
                new XElement("Index", Index));
        }

        public void Download(ChapterTestData a_chapter_test_data)
        {
            ChapterTestData = a_chapter_test_data;
            Page = ChapterTestData.Chapter.Pages.FirstOrDefault(el => el.Name == Name);
            Index = Page.Index;
            ImageURL = Path.GetExtension(Page.ImageURL);
            URL = Page.URL;

            Name = "";

            Limiter.BeginChapter(Page.Chapter);

            try
            {
                var stream = Page.GetImageStream();

                Image = System.Drawing.Image.FromStream(stream);
                stream.Position = 0;

                Hash = TomanuExtensions.Utils.Hash.CalculateSHA256(stream);
            }
            finally
            {
                Limiter.EndChapter(Page.Chapter);
            }

            Name = Page.Name;
        }

        public bool Compare(PageTestData a_downloaded)
        {
            if (a_downloaded.Hash != Hash)
                return false;
            if (a_downloaded.Name != Name)
                return false;
            if (a_downloaded.ImageURL != ImageURL)
                return false;
            if (a_downloaded.URL != URL)
                return false;

            return true;
        }
    }

    public class ChapterTestData
    {
        public Chapter Chapter { get; set; }

        public SerieTestData SerieTestData { get; set; }

        public string Title { get; set; }

        public int PageCount { get; set; }

        public string URL { get; set; }

        public int Index { get; set; }

        public List<PageTestData> Pages = new List<PageTestData>();

        public static ChapterTestData Load(XElement a_node)
        {
            ChapterTestData ctd = new ChapterTestData();

            ctd.PageCount = Int32.Parse(a_node.Element("PageCount").Value);
            ctd.Index = Int32.Parse(a_node.Element("Index").Value);
            ctd.Title = a_node.Element("Title").Value;
            ctd.URL = a_node.Element("URL").Value;

            Assert.IsTrue(!String.IsNullOrWhiteSpace(ctd.Title));
            Assert.IsTrue(!String.IsNullOrWhiteSpace(ctd.URL));

            foreach (var page_node in a_node.Element("Pages").Elements())
                ctd.Pages.Add(PageTestData.Load(page_node));

            return ctd;
        }

        public XElement GetAsXml()
        {
            return new XElement("ChapterTestData", 
                new XElement("Title", Title), 
                new XElement("PageCount", PageCount),
                new XElement("Index", Index),
                new XElement("URL", URL), 
                new XElement("Pages", 
                    from page in Pages
                    select page.GetAsXml()));
        }

        public void Download(SerieTestData a_serie_test_data)
        {
            SerieTestData = a_serie_test_data;
            Chapter = SerieTestData.Serie.Chapters.FirstOrDefault(el => el.Title == Title);
            Index = Chapter.Serie.Chapters.IndexOf(Chapter);
            URL = Chapter.URL;

            PageCount = -1;
            Title = "";

            Chapter.State = ChapterState.Waiting;
            Limiter.BeginChapter(Chapter);
            try
            {
                Chapter.DownloadPagesList();
            }
            finally
            {
                Limiter.EndChapter(Chapter);
            }

            PageCount = Chapter.Pages.Count;
            Title = Chapter.Title;

            foreach (var page in Pages)
                page.Download(this);
        }

        public bool Compare(ChapterTestData a_downloaded)
        {
            if (a_downloaded.PageCount != PageCount)
                return false;
            if (a_downloaded.Title != Title)
                return false;
            if (a_downloaded.URL != URL)
                return false;
            if (a_downloaded.Index != Index)
                return false;

            for (int i = 0; i < Pages.Count; i++)
            {
                if (!Pages[i].Compare(a_downloaded.Pages[i]))
                    return false;
            }

            return true;
        }

        public void AddPage(PageTestData a_page)
        {
            Pages.Add(a_page);
            a_page.ChapterTestData = this;
        }
    }

    public class SerieTestData
    {
        public Serie Serie { get; set; }

        public ServerTestData ServerTestData { get; set; }

        public string Title { get; set; }

        public int ChapterCount { get; set; }

        public string URL { get; set; }

        public List<ChapterTestData> Chapters = new List<ChapterTestData>();

        public static SerieTestData Load(XElement a_node)
        {
            SerieTestData std = new SerieTestData();

            std.ChapterCount = Int32.Parse(a_node.Element("ChapterCount").Value);
            std.Title = a_node.Element("Title").Value;
            std.URL = a_node.Element("URL").Value;

            Assert.IsTrue(!String.IsNullOrWhiteSpace(std.Title));
            Assert.IsTrue(!String.IsNullOrWhiteSpace(std.URL));

            foreach (var chapter_node in a_node.Element("Chapters").Elements())
                std.Chapters.Add(ChapterTestData.Load(chapter_node));

            return std;
        }

        public XElement GetAsXml()
        {
            return new XElement("SerieTestData", 
                new XElement("Title", Title), 
                new XElement("ChapterCount", ChapterCount),
                new XElement("URL", URL), 
                new XElement("Chapters", 
                    from chapter in Chapters
                    select chapter.GetAsXml()));
        }

        public void Download(ServerTestData a_server_test_data)
        {
            ServerTestData = a_server_test_data;
            Serie = ServerTestData.Server.Series.FirstOrDefault(el => el.Title == Title);
            URL = Serie.URL;

            ChapterCount = -1;
            Title = "";

            Serie.State = SerieState.Waiting;
            Serie.DownloadChapters();

            ChapterCount = Chapters.Count;
            Title = Serie.Title;

            foreach (var chapter in Chapters)
                chapter.Download(this);
        }

        public bool Compare(SerieTestData a_downloaded)
        {
            if (a_downloaded.ChapterCount != ChapterCount)
                return false;
            if (a_downloaded.Title != Title)
                return false;
            if (a_downloaded.URL != URL)
                return false;

            for (int i = 0; i < Chapters.Count; i++)
            {
                if (!Chapters[i].Compare(a_downloaded.Chapters[i]))
                    return false;
            }

            return true;
        }

        public void AddChapter(ChapterTestData a_chapter)
        {
            Chapters.Add(a_chapter);
            a_chapter.SerieTestData = this;
        }
    }

    public class ServerTestData
    {
        public Server Server { get; set; }

        public string Name { get; set; }

        public int SerieCount { get; set; }

        public List<SerieTestData> Series = new List<SerieTestData>();

        public static ServerTestData Load(string a_file_path)
        {
            ServerTestData std = new ServerTestData();

            XElement root = XElement.Load(a_file_path);

            std.Name = root.Element("Name").Value;
            std.SerieCount = Int32.Parse(root.Element("SerieCount").Value);

            Assert.IsTrue(!String.IsNullOrWhiteSpace(std.Name));

            foreach (var serie_node in root.Element("Series").Elements())
                std.Series.Add(SerieTestData.Load(serie_node));

            return std;
        }

        public void Save(string a_file_name)
        {
            XElement root = new XElement("ServerTestData", 
                new XElement("Name", Name), 
                new XElement("SerieCount", SerieCount), 
                new XElement("Series", 
                    from serie in Series
                    select serie.GetAsXml()));

            root.Save(a_file_name);
        }

        public void Download()
        {
            Server = DownloadManager.Instance.Servers.First(s => s.Name == Name);

            Name = "";
            SerieCount = -1;

            Server.State = ServerState.Waiting;
            Server.DownloadSeries();

            Name = Server.Name;
            SerieCount = Series.Count;

            foreach (var serie in Series)
                serie.Download(this);
        }

        public bool Compare(ServerTestData a_downloaded)
        {
            if (a_downloaded.SerieCount != SerieCount)
                return false;

            if (a_downloaded.Name != Name)
                return false;

            for (int i = 0; i < Series.Count; i++)
            {
                if (!Series[i].Compare(a_downloaded.Series[i]))
                    return false;
            }

            return true;
        }

        public void AddSeries(SerieTestData a_serie)
        {
            Series.Add(a_serie);
            a_serie.ServerTestData = this;
        }
    }
}
