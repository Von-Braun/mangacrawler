using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MangaCrawlerLib;
using HtmlAgilityPack;
using System.IO;

namespace MangaCrawlerTest
{
    [TestClass]
    public class MangaCrawlerTest
    {
        private TestContext m_testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return m_testContextInstance;
            }
            set
            {
                m_testContextInstance = value;
            }
        }
        private List<SerieInfo> TestServer(ServerInfo a_info, int a_count)
        {
            a_info.DownloadSeries();
            var series = a_info.Series;

            new HtmlWeb().Load(a_info.URL);

            Assert.IsTrue(series.Count >= a_count);

            return a_info.Series;
        }

        private List<ChapterInfo> TestSerie(SerieInfo a_serie, int a_count)
        {
            a_serie.DownloadChapters();
            var chapters = a_serie.Chapters;

            new HtmlWeb().Load(a_serie.URL);

            Assert.IsTrue(chapters.Count >= a_count);

            return chapters;
        }

        private List<PageInfo> TestChapter(ChapterInfo a_chapter, int a_count)
        {
            new HtmlWeb().Load(a_chapter.URL);

            var pages = a_chapter.Pages;

            Assert.IsTrue(pages.Count == a_count);

            return pages;
        }

        private void TestPage(PageInfo a_page, string a_hash = null)
        {
            var stream = a_page.ImageStream;

            Assert.IsTrue(stream.Length > 0);

            if (a_hash != null)
            {
                Assert.AreEqual(a_hash, GetHash(stream));
            }
            else
            {
                TestContext.WriteLine("serie: %s, chapter: %s, page: %d, hash: %s",
                    a_page.ChapterInfo.SerieInfo.Name, a_page.ChapterInfo.Name, a_page.Index, GetHash(stream));
            }

        }

        private string GetHash(MemoryStream stream)
        {
            return HashLib.HashFactory.Crypto.CreateSHA512().ComputeStream(stream).ToString();
        }

        [TestMethod]
        public void AnimeSourceTest()
        {
            var series = TestServer(ServerInfo.CreateAnimeSource(), 10);

            var chapters = TestSerie(series.First(s => s.Name == "Kimagure Orange Road"), 10);

            var pages = TestChapter(chapters.Last(), 40);

            TestPage(pages.Last(), null);

            // test na ostatni rozdzial kiedy jest kontynuacja

            // inny test, ktory dla kazdego servera, dla kazdej serii, dla paru rozdzialow, pierwszy i ostatni, dla paru stron, pierwsza i ostatnia, pobiera 
            // wszystko.
        }
    }
}
