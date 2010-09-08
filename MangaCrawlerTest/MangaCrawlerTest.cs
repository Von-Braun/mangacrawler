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

            if (a_count != -1)
                Assert.IsTrue(series.Count >= a_count);
            else
                TestContext.WriteLine("series: {0}", series.Count);

            return a_info.Series;
        }

        private List<ChapterInfo> TestSerie(SerieInfo a_serie, int a_count)
        {
            a_serie.DownloadChapters();
            var chapters = a_serie.Chapters;

            new HtmlWeb().Load(a_serie.URL);

            if (a_count != -1)
                Assert.IsTrue(chapters.Count >= a_count);
            else
                TestContext.WriteLine("serie: {0}, chapters: {1}", a_serie.Name, chapters.Count);

            return chapters;
        }

        private List<PageInfo> TestChapter(ChapterInfo a_chapter, int a_count)
        {
            new HtmlWeb().Load(a_chapter.URL);

            var pages = a_chapter.Pages;

            if (a_count != -1)
                Assert.IsTrue(pages.Count == a_count);
            else
            {
                TestContext.WriteLine("serie: {0}, chapter: {1}, pages: {2}", 
                    a_chapter.SerieInfo.Name, a_chapter.Name, pages.Count);
            }

            return pages;
        }

        private void TestPage(PageInfo a_page, string a_hash = null)
        {
            var stream = a_page.ImageStream;

            Assert.IsTrue(stream.Length > 0);

            System.Drawing.Image.FromStream(stream);
            stream.Position = 0;

            if (a_hash != null)
            {
                Assert.AreEqual(a_hash, GetHash(stream));
            }
            else
            {
                TestContext.WriteLine("serie: {0}, chapter: {1}, page: {2}, hash: {3}",
                    a_page.ChapterInfo.SerieInfo.Name, a_page.ChapterInfo.Name, a_page.Index, GetHash(stream));
            }
        }

        private string GetHash(MemoryStream stream)
        {
            return HashLib.HashFactory.Crypto.CreateSHA256().ComputeStream(stream).ToString();
        }

        [TestMethod]
        public void AnimeSourceTest()
        {
            var series = TestServer(ServerInfo.CreateAnimeSource(), 52);

            var chapters = TestSerie(series.First(s => s.Name == "Kimagure Orange Road"), 167);

            var pages = TestChapter(chapters.Last(), 4);

            TestPage(pages.First(), "97895BDE-0DE30690-79305713-6CCF2D22-A5B83C1F-586BA24C-AC7DC3F7-8826E214");
            TestPage(pages.Last(), "25476017-BDD69A52-FAEEC980-8B6D2BB0-F36BD832-8AFA63B9-335FCB0A-C4C6AE50");

            chapters = TestSerie(series.First(s => s.Name == "AIKI"), 61);

            pages = TestChapter(chapters.Last(), 27);

            TestPage(pages.First(), "A0393FC2-BD1F6363-95F28E49-9110C57B-B9B58711-219A9DEB-1F393DC2-19F4AC59");
            TestPage(pages.Last(), "70457F96-F014EE56-B7B1DAC1-49AE9D18-21B5841D-81338B78-A4FCB68E-BCE17D57");

            pages = TestChapter(chapters.First(), 36);

            TestPage(pages.First(), "4D5FA238-A53BE9A3-C14F7599-922A940E-879DBC4A-C2894E12-98C986DA-A7ACBDA2");
            TestPage(pages.Last(), "A9793054-13436650-7ACCE952-2CDF99D0-014244F4-82FEF5D6-A7A7438D-C5D038C9");
        }

        [TestMethod]
        public void MangaFoxTest()
        {
            var series = TestServer(ServerInfo.CreateMangaFox(), 6768);

            var chapters = TestSerie(series.First(s => s.Name == ".hack//G.U.+"), 26);

            var pages = TestChapter(chapters.Last(), 68);

            TestPage(pages.First(), "BB93A387-185223CB-8EC50E70-899AA5F4-1B70222B-A39ED542-BAA71897-C5ECB461");
            TestPage(pages.Last(), "A08602B0-41A27AAD-D870271E-F8AD256A-68D2C903-3C775B39-DF207BB2-95D1C137");

            pages = TestChapter(chapters.First(), 33);

            TestPage(pages.First(), "454E0B8D-03CA4892-BEE861B4-ABE79154-56FB60F2-8910BE2A-BDC107C0-9388DED0");
            TestPage(pages.Last(), "DED6595F-377DBE4F-D204100F-4A697979-A717AA9D-E24314C3-4E209759-650680B9");
        }

        [TestMethod]
        public void MangaRun()
        {
            var series = TestServer(ServerInfo.CreateMangaRun(), 382);

            var chapters = TestSerie(series.First(s => s.Name == "666satan"), 78);

            var pages = TestChapter(chapters.Last(), 51);

            TestPage(pages.First(), "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB");
            TestPage(pages.Last(), "0D8475C4-E5D98687-C4DA831B-0D8F7003-6DF7F2EE-3747FC41-1E56B602-AF65CE0A");

            pages = TestChapter(chapters.First(), 25);

            TestPage(pages.First(), "4C1EBC9E-132DC56A-56B47BD6-C567DE7A-9354C577-D2C7E01E-18B7209B-CFDC1D43");
            TestPage(pages.Last(), "C7BBA2D4-579AD7C0-38DE23A8-E7BDC94A-0D1480F3-50D22B2F-F759BF7B-E684F834");
        }
    }
}
