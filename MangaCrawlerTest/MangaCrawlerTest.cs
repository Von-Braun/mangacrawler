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

        [TestInitialize]
        public void Initialize()
        {
        }

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

            {
                var chapters = TestSerie(series.First(s => s.Name == "Kimagure Orange Road"), 167);

                var pages = TestChapter(chapters.Last(), 4);

                TestPage(pages.First(), "97895BDE-0DE30690-79305713-6CCF2D22-A5B83C1F-586BA24C-AC7DC3F7-8826E214");
                TestPage(pages.Last(), "25476017-BDD69A52-FAEEC980-8B6D2BB0-F36BD832-8AFA63B9-335FCB0A-C4C6AE50");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "AIKI"), 61);

                var pages = TestChapter(chapters.Last(), 27);

                TestPage(pages.First(), "A0393FC2-BD1F6363-95F28E49-9110C57B-B9B58711-219A9DEB-1F393DC2-19F4AC59");
                TestPage(pages.Last(), "70457F96-F014EE56-B7B1DAC1-49AE9D18-21B5841D-81338B78-A4FCB68E-BCE17D57");

                pages = TestChapter(chapters.First(), 36);

                TestPage(pages.First(), "4D5FA238-A53BE9A3-C14F7599-922A940E-879DBC4A-C2894E12-98C986DA-A7ACBDA2");
                TestPage(pages.Last(), "A9793054-13436650-7ACCE952-2CDF99D0-014244F4-82FEF5D6-A7A7438D-C5D038C9");
            }
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
        public void MangaRunTest()
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

        [TestMethod]
        public void MangaShareTest()
        {
            var series = TestServer(ServerInfo.CreateMangaShare(), 101);

            var chapters = TestSerie(series.First(s => s.Name == "666 Satan"), 77);

            var pages = TestChapter(chapters.Last(), 80);

            TestPage(pages.First(), "D7654CB6-AC4813C0-5B985B91-0E15FEC9-193F9C48-D78B8A77-67A9B4A4-F1C0C337");
            TestPage(pages.Last(), "96EF8686-80AFE4C8-8A149FC6-AA587E14-32F557E5-B65E4FD1-CB49D80E-563E2C97");

            pages = TestChapter(chapters.First(), 51);

            TestPage(pages.First(), "654005EA-44B03AD4-6B7D1004-5CAE6C14-13B1B8C5-BB09114D-C4240FEE-C084D426");
            TestPage(pages.Last(), "B17D5A70-A3723C4C-FE5D6B46-6E66E8C8-ABD57067-14DB3D66-4C373CA1-60EF1D32");
        }

        [TestMethod]
        public void MangaToshokanTest()
        {
            var series = TestServer(ServerInfo.CreateMangaToshokan(), 946);

            {
                var chapters = TestSerie(series.First(s => s.Name == "Angel Shop"), 15);

                var pages = TestChapter(chapters.Last(), 56);

                TestPage(pages.First(), "8A2B473E-BD8752D4-3616D223-8B288E32-E054EE8A-3AE7EACC-E652BC95-2478081D");
                TestPage(pages.Last(), "B8781498-A9EA2DCA-255D3822-A2F9A8E8-306A1FE0-AA076D04-92707D2D-B43A84C4");

                pages = TestChapter(chapters.First(), 29);

                TestPage(pages.First(), "6FADD86A-1A648C38-23BC157C-FB48A97F-05A3A918-B9BEC2B2-3AEEDA36-2AE299AD");
                TestPage(pages.Last(), "BD3117E2-E4B9B41C-7DF13F9B-73C10CCE-62D2B126-07BE5B61-D92280FF-0AE70516");
            }

            {
                HtmlDocument doc = new HtmlWeb().Load(ServerInfo.CreateMangaToshokan().URL);

                var rows = doc.DocumentNode.SelectNodes("/html/body/div/div/div[6]/div[2]/div/table/tr/td/table[2]/tr/td[2]/table/tr");

                bool ongoing = false;

                foreach (var row in rows)
                {
                    if (row.ChildNodes.Count < 8)
                        continue;

                    if (row.ChildNodes[1].InnerText == "1/2 Prince")
                    {
                        Assert.IsTrue(row.ChildNodes[7].InnerText == "ongoing");
                        ongoing = true;
                        break;
                    }
                }

                Assert.IsTrue(ongoing);
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "1/2 Prince"), 47);

                var pages = TestChapter(chapters.Last(), 34);

                TestPage(pages.First(), "15179861-3DEAD8DF-09B49A60-7CB67AC3-DAC29A33-40CDB875-04D0678B-1440D4C7");
                TestPage(pages.Last(), "7CD19E4C-EFA18F64-BEC0B089-82140298-84F0EFAC-E73C39B9-FFA9D500-DB33A61C");

                pages = TestChapter(chapters.First(), 71);

                TestPage(pages.First(), "456C6422-8F18E283-344F9FAE-2DAF8AAC-F4ED58BD-48404EF6-1B518ED6-1982DAD3");
                TestPage(pages.Last(), "B501E1E1-77699CF0-1C577390-C7384159-DCA79666-66F9BA6E-D2E3AE43-4877B517");
            }
        }

        [TestMethod]
        public void MangaVolumeTest()
        {
            var series = TestServer(ServerInfo.CreateMangaVolume(), 882);

            var chapters = TestSerie(series.First(s => s.Name == "A Wolf Warning"), 6);

            var pages = TestChapter(chapters.Last(), 44);

            TestPage(pages.First(), "64998013-8EA1B049-DF6C0922-898DB12B-4F99D6A4-239F1C44-2F57082E-91B99836");
            TestPage(pages.Last(), "954B5B99-AA18837E-96B3E51B-37988EDA-2C6075D5-C3801771-42C825E7-865505AB");

            pages = TestChapter(chapters.First(), 42);

            TestPage(pages.First(), "B5164DED-11BA5489-20E366C3-2A91C900-ABD00AF0-8FBE8B4A-6C8AA9A5-AD3031D8");
            TestPage(pages.Last(), "DB5A3A62-435BFD36-F9496253-A3468DB8-140A33E1-3855C48B-FACC4DF5-5CEF7242");
        }

        [TestMethod]
        public void OtakuWorksTest()
        {
            var series = TestServer(ServerInfo.CreateOtakuWorks(), -1);

            {
                HtmlDocument doc = new HtmlWeb().Load(series.First(s => s.Name == "07 Ghost").URL);
                var pages = doc.DocumentNode.SelectNodes("/html/body/div/div/div[5]/div/div[3]/div[10]/div[27]/div/a").AsEnumerable();
                Assert.IsTrue(pages != null);
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "07 Ghost"), -1);

                var pages = TestChapter(chapters.Last(), -1);

                TestPage(pages.First(), null);
                TestPage(pages.Last(), null);

                pages = TestChapter(chapters.First(), -1);

                TestPage(pages.First(), null);
                TestPage(pages.Last(), null);
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == ".hack//G.U. The World"), -1);

                var pages = TestChapter(chapters.Last(), -1);

                TestPage(pages.First(), null);
                TestPage(pages.Last(), null);

                pages = TestChapter(chapters.First(), -1);

                TestPage(pages.First(), null);
                TestPage(pages.Last(), null);
            }
        }

        [TestMethod]
        public void xxxTest()
        {
            var series = TestServer(ServerInfo.CreateMangaToshokan(), -1);

            var chapters = TestSerie(series.First(s => s.Name == ""), -1);

            var pages = TestChapter(chapters.Last(), -1);

            TestPage(pages.First(), null);
            TestPage(pages.Last(), null);

            pages = TestChapter(chapters.First(), -1);

            TestPage(pages.First(), null);
            TestPage(pages.Last(), null);
        }
    }
}
