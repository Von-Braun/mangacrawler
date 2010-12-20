using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MangaCrawlerLib;
using HtmlAgilityPack;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

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

        private bool m_error = false;

        private IEnumerable<SerieInfo> TestServer(ServerInfo a_info, int a_count)
        {
            TestContext.WriteLine("Testing server {0}", a_info.Name);

            a_info.DownloadSeries();
            var series = a_info.Series;

            new HtmlWeb().Load(a_info.URL);

            if (a_count > 0)
            {
                TestContext.WriteLine("Series, expected more than {0}, finded: {1}", a_count, series.Count());

                if (series.Count() < a_count)
                    m_error = true;
            }
            else if (a_count == 0)
            {
                TestContext.WriteLine("series: {0}", series.Count());
                m_error = true;
            }

            Assert.IsFalse(a_info.Series.Any(s => String.IsNullOrWhiteSpace(s.Name)));

            return a_info.Series;
        }

        private IEnumerable<ChapterInfo> TestSerie(SerieInfo a_info, int a_count)
        {
            TestContext.WriteLine("  Testing serie {0}", a_info.Name);

            a_info.DownloadChapters();
            var chapters = a_info.Chapters;

            new HtmlWeb().Load(a_info.URL);

            if (a_count > 0)
            {
                TestContext.WriteLine("  Chapters, expected {0}, finded: {1}", a_count, chapters.Count());

                if (chapters.Count() < a_count)
                    m_error = true;
            }
            else if (a_count == 0)
            {
                TestContext.WriteLine("  serie: {0}, chapters: {1}", a_info.Name, chapters.Count());
                m_error = true;
            }

            return chapters;
        }

        private IEnumerable<PageInfo> TestChapter(ChapterInfo a_info, int a_count)
        {
            TestContext.WriteLine("    Testing chapter {0}", a_info.Name);

            new HtmlWeb().Load(a_info.URL);

            a_info.DownloadPages();
            var pages = a_info.Pages;

            if (a_count > 0)
            {
                TestContext.WriteLine("    Pages, expected {0}, finded: {1}", a_count, pages.Count());

                if (pages.Count() != a_count)
                    m_error = true;
            }
            else if (a_count == 0)
            {
                TestContext.WriteLine("    serie: {0}, chapter: {1}, pages: {2}",
                    a_info.SerieInfo.Name, a_info.Name, pages.Count());
            }            

            return pages;
        }

        private void TestPage(PageInfo a_info, string a_hash = null)
        {
            TestContext.WriteLine("        Testing page {0}", a_info.Name);

            var stream = a_info.GetImageStream();

            Assert.IsTrue(stream.Length > 0);

            System.Drawing.Image.FromStream(stream);
            stream.Position = 0;

            if (a_hash != null)
            {
                if (a_hash != "")
                {
                    string hash = GetHash(stream);
                    if (a_hash != hash)
                    {
                        TestContext.WriteLine("        Hash doestn't match, finded: {0}", hash);
                        m_error = true;
                    }
                }
                else
                {
                    TestContext.WriteLine("        serie: {0}, chapter: {1}, page: {2}, hash: {3}",
                        a_info.ChapterInfo.SerieInfo.Name, a_info.ChapterInfo.Name, a_info.Index, GetHash(stream));
                    m_error = true;
                }
            }
        }

        private string GetHash(MemoryStream stream)
        {
            return HashLib.HashFactory.Crypto.CreateSHA256().ComputeStream(stream).ToString();
        }

        [TestMethod]
        public void AnimeSourceTest()
        {
            var series = TestServer(ServerInfo.AnimeSource, 52);

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
            var series = TestServer(ServerInfo.MangaFox, 6600);

            {
                var chapters = TestSerie(series.First(s => s.Name == ".hack//G.U.+"), 26);

                var pages = TestChapter(chapters.Last(), 68);

                TestPage(pages.First(), "BB93A387-185223CB-8EC50E70-899AA5F4-1B70222B-A39ED542-BAA71897-C5ECB461");
                TestPage(pages.Last(), "A08602B0-41A27AAD-D870271E-F8AD256A-68D2C903-3C775B39-DF207BB2-95D1C137");

                pages = TestChapter(chapters.First(), 33);

                TestPage(pages.First(), "454E0B8D-03CA4892-BEE861B4-ABE79154-56FB60F2-8910BE2A-BDC107C0-9388DED0");
                TestPage(pages.Last(), "DED6595F-377DBE4F-D204100F-4A697979-A717AA9D-E24314C3-4E209759-650680B9");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "(G) Edition"), 3);

                var pages = TestChapter(chapters.Last(), 17);

                TestPage(pages.First(), "6CC9C11F-4E614BFE-CB4AF33F-F4344834-717C52C9-C67672EB-B2CD6178-A3C24814");
                TestPage(pages.Last(), "0CBD3787-E149EF52-00065BE3-1AD2C925-29D905EC-581835B8-DC637B3D-2ACEC1CD");

                pages = TestChapter(chapters.First(), 17);

                TestPage(pages.First(), "599A16FB-AA9EF0B2-CCA60F9D-3DBB4CA5-223B3C8D-358EC73D-B09616B8-0C39AE04");
                TestPage(pages.Last(), "BB09C661-C715F7CF-FEAAA554-58BB887E-6480410E-7BB6E6E3-EE8D7EF8-07F250A3");

                
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "[switch]"), -1);

                Assert.IsTrue(chapters.Count() == 0);
            }

            Assert.IsFalse(m_error);
        }

        [TestMethod]
        public void MangaRunTest()
        {
            var series = TestServer(ServerInfo.MangaRun, 382);

            var chapters = TestSerie(series.First(s => s.Name == "666satan"), 78);

            var pages = TestChapter(chapters.Last(), 51);

            TestPage(pages.First(), "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB");
            TestPage(pages.Last(), "0D8475C4-E5D98687-C4DA831B-0D8F7003-6DF7F2EE-3747FC41-1E56B602-AF65CE0A");

            pages = TestChapter(chapters.First(), 25);

            TestPage(pages.First(), "4C1EBC9E-132DC56A-56B47BD6-C567DE7A-9354C577-D2C7E01E-18B7209B-CFDC1D43");
            TestPage(pages.Last(), "C7BBA2D4-579AD7C0-38DE23A8-E7BDC94A-0D1480F3-50D22B2F-F759BF7B-E684F834");

            Assert.IsFalse(m_error);
        }

        [TestMethod]
        public void MangaShareTest()
        {
            var series = TestServer(ServerInfo.MangaShare, 120);

            var chapters = TestSerie(series.First(s => s.Name == "666 Satan"), 77);

            var pages = TestChapter(chapters.Last(), 80);

            TestPage(pages.First(), "D7654CB6-AC4813C0-5B985B91-0E15FEC9-193F9C48-D78B8A77-67A9B4A4-F1C0C337");
            TestPage(pages.Last(), "96EF8686-80AFE4C8-8A149FC6-AA587E14-32F557E5-B65E4FD1-CB49D80E-563E2C97");

            pages = TestChapter(chapters.First(), 51);

            TestPage(pages.First(), "654005EA-44B03AD4-6B7D1004-5CAE6C14-13B1B8C5-BB09114D-C4240FEE-C084D426");
            TestPage(pages.Last(), "B17D5A70-A3723C4C-FE5D6B46-6E66E8C8-ABD57067-14DB3D66-4C373CA1-60EF1D32");

            Assert.IsFalse(m_error);
        }

        [TestMethod]
        public void MangaToshokanTest()
        {
            var series = TestServer(ServerInfo.MangaToshokan, 1050);

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
                HtmlDocument doc = new HtmlWeb().Load(ServerInfo.MangaToshokan.URL);

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

                if (!ongoing)
                {
                    TestContext.WriteLine("!!! Not ongoing.");
                    m_error = true;
                }
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "1/2 Prince"), 47);

                var pages = TestChapter(chapters.Last(), 30);

                TestPage(pages.Last(), null);

                pages = TestChapter(chapters.First(), 71);

                TestPage(pages.First(), "456C6422-8F18E283-344F9FAE-2DAF8AAC-F4ED58BD-48404EF6-1B518ED6-1982DAD3");
                TestPage(pages.Last(), "B501E1E1-77699CF0-1C577390-C7384159-DCA79666-66F9BA6E-D2E3AE43-4877B517");
            }

            Assert.IsFalse(m_error);
        }

        [TestMethod]
        public void MangaVolumeTest()
        {
            var series = TestServer(ServerInfo.MangaVolume, 880);

            {
                var chapters = TestSerie(series.First(s => s.Name == "3x3 Eyes"), 406);

                var pages = TestChapter(chapters.Last(), 15);

                TestPage(pages.First(), "D7AA1E8F-6D1F2766-40C81052-82223AB4-9D722C24-16429545-4B4635A6-26D497A7");
                TestPage(pages.Last(), "3B443FB1-A263AE7D-E81A9ADA-5FF3AD75-F3B6CB50-011A31C2-9D214533-C2EB7E8B");

                pages = TestChapter(chapters.First(), 15);

                TestPage(pages.First(), "DF44ECC3-32E962CB-046EC1FC-6FD48EAD-E39F80F2-95C73D8F-A47E99FF-5D7B70D4");
                TestPage(pages.Last(), "3855969B-92F3F9A1-5A15F348-4E71F3D7-645650EC-62EF40A2-4C231610-0A36A445");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "Bleach"), -1);

                Assert.IsTrue(chapters.Count() == 0);
            }

            Assert.IsFalse(m_error);
        }

        [TestMethod]
        public void OtakuWorksTest()
        {
            var series = TestServer(ServerInfo.OtakuWorks, 4300);

            {
                var chapters = TestSerie(series.First(s => s.Name == "07 Ghost"), 64);

                var pages = TestChapter(chapters.First(), -1);

                TestPage(pages.Last(), null);

                pages = TestChapter(chapters.First(), 32);

                TestPage(pages.First(), "612C7ECB-38C987EE-DA982212-BD31E881-47E82A53-D4CB8A00-1FAB9981-790B34FF");
                TestPage(pages.Last(), "2838B57C-B6F10ABF-2FDE2C00-8D19324A-FB1E407D-EDBE2314-1E93AC9E-66704A02");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == ".hack//G.U. The World"), -1);

                Assert.IsTrue(chapters.Count() == 0);
            }

            Assert.IsFalse(m_error);
        }

        [TestMethod]
        public void OurMangaTest()
        {
            var series = TestServer(ServerInfo.OurManga, 1703);

            var chapters = TestSerie(series.First(s => s.Name == "3x3 Eyes"), 558);

            var pages = TestChapter(chapters.Last(), 34);

            TestPage(pages.First(), "B966DB33-56D05253-F325D7DF-2D445889-B39F11D7-0B819587-F3A02925-31E16259");
            TestPage(pages.Last(), "1A0614C0-7DACDA48-F462A919-0FC944EF-DF21684C-1F9207A8-6EB01E62-F1858B57");

            pages = TestChapter(chapters.First(), 15);

            TestPage(pages.First(), "FB75E543-D84B9D8E-F35B7041-41A3D5B4-A5EDD861-D23BE057-46EF2E2A-2951C1A4");
            TestPage(pages.Last(), "DDF19F32-3F35AF13-9EEE5F45-585AC5A7-4B456BE8-B80BB49B-77F047E7-15D2B90F");

            Assert.IsFalse(m_error);
        }

        [TestMethod]
        public void SpectrumNexusTest()
        {
            var series = TestServer(ServerInfo.SpectrumNexus, 134);

            var chapters = TestSerie(series.First(s => s.Name == "Bleach"), 50);

            var pages = TestChapter(chapters.ToArray()[47], 173);

            TestPage(pages.First(), "6F219A82-10B8F7B2-29BA5537-6483DB47-F90F8024-88C51B8F-7B61835E-6E1B7E2B");
            TestPage(pages.Last(), "40AC5FF2-9D76CC6F-38D0A0B0-96C1CDB3-3955085D-5CED08F7-4BD57E1B-30CA46F4");

            pages = TestChapter(chapters.First(), 187);

            TestPage(pages.First(), "62A96AF0-2818D19A-0A35DAA7-FD513701-6E6670E0-663EC84E-AAA3579D-CC6B8733");
            TestPage(pages.Last(), "7B541E33-09E0105D-A3B4EEE4-B2E6B819-330AF88E-8FA96087-BCE7DFE4-59A9CA55");

            pages = TestChapter(chapters.Last(), -1);
            TestPage(pages.Last(), null);

            Assert.IsFalse(m_error);
        }

        [TestMethod]
        public void StopTazmoTest()
        {
            var series = TestServer(ServerInfo.StopTazmo, 1792);

            var chapters = TestSerie(series.First(s => s.Name == "Bleach"), 422);

            var pages = TestChapter(chapters.ToArray()[420], 22);

            TestPage(pages.First(), "C91EF566-472EBA2D-3C527AB5-98DEB6E6-621C59A4-AFB0A26E-D9BDDFAB-983F2F23");
            TestPage(pages.Last(), "8A2A4703-FDF58BF9-0AAEC0A9-93F2876B-DBECD43E-D8DAE263-3F958C29-455D5E9F");

            pages = TestChapter(chapters.First(), 57);

            TestPage(pages.First(), "8D78D814-791583E2-19F0FC41-460F600B-982ABBF0-6278B2A9-3D6D5112-ADB86FA6");
            TestPage(pages.Last(), "41838061-F5379AD8-FF615340-5FEF5C07-C9A68FF7-6F643947-7A203C17-5308D8A4");

            pages = TestChapter(chapters.Last(), -1);

            TestPage(pages.Last(), null);


            Assert.IsFalse(m_error);
        }

        [TestMethod]
        public void UnixMangaTest()
        {
            var series = TestServer(ServerInfo.UnixManga, 1532);

            {
                var chapters = TestSerie(series.First(s => s.Name == "Bleach"), 439);

                var pages = TestChapter(chapters.Last(), 8);

                TestPage(pages.First(), "262ED4DC-7C5B3D1F-B0918BB3-3DACD75A-60D2119B-17A29A97-04E12601-051312D8");
                TestPage(pages.Last(), "B8177447-C74F7AA8-0521B396-2B7119F8-E173C00B-27F6B894-C997C530-2566F1E6");

                pages = TestChapter(chapters.First(), 15);

                TestPage(pages.First(), "99285B2F-50490ECA-D0D8A9E4-818032F2-186D0AC4-B75331FD-3818B301-FA0F2CA2");
                TestPage(pages.Last(), "52B1C84E-03455C85-1C96BA7F-6F4FCF33-B839109A-E6F699D6-E9400BCB-13CC5B90");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "666 Satan"), 80);

                var pages = TestChapter(chapters.Last(), 25);

                TestPage(pages.First(), "408A4442-0CDE316E-1E41544D-B8DFB08C-402CDC6E-9128EF2E-6B2E1ECE-8496E9D7");
                TestPage(pages.Last(), "1595E11D-A302BA42-7A33F0E1-4ADEF4BD-C5365FBF-0B89123E-B61C9E8B-E7E08A2D");

                pages = TestChapter(chapters.First(), 30);

                TestPage(pages.First(), "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB");
                TestPage(pages.Last(), "AC9DF68B-C07F380B-DBBCF4EA-4A79C0DB-EF00A7DC-5AF91C46-56E1F179-90EDB30F");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "Kamisama no Tsukurikata"), 17);

                var pages = TestChapter(chapters.Last(), 30);

                TestPage(pages.First(), "BA438114-F68B9CDD-42198F17-8C72B2D3-78882EC9-C9A2A995-BFE19D26-FDD0A9C5");
                TestPage(pages.Last(), "A95CE474-FBFCFCEE-F98AB70E-C3B341C3-609F8871-7AD9F33C-A0EF7BFB-1BDF5B8D");

                pages = TestChapter(chapters.First(), 30);

                TestPage(pages.First(), "49C0AACF-CEFCBFD1-626AF6D5-3E3F444A-252DCBCC-CB109F3A-BEB2FCB7-F2C39DA0");
                TestPage(pages.Last(), "FFC3EBB9-9BDF5759-DE87C6D6-E2634C38-8982EC38-34CF67C6-2336B18D-28F7862E");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "16 Sai Kissu Complete"), -1);

                Assert.IsTrue(chapters.Count() == 1);

                var pages = TestChapter(chapters.First(), 30);

                TestPage(pages.First(), "ABC41912-39037D79-66ED232B-E4F0EC7B-907F9E02-18CB640D-FF6E32D2-68750D27");
                TestPage(pages.Last(), "3048567C-DF4072AC-01C71054-734C7736-BB6CD48E-92E1EA56-BB5DD04A-4C1048EC");
            }

            Assert.IsFalse(m_error);
        }

        private static IEnumerable<T> TakeRandom<T>(IEnumerable<T> a_enum, double a_percent)
        {
            List<T> list = a_enum.ToList();
            Random random = new Random();

            for (int i = 0; i < list.Count * a_percent; i++)
            {
                int r = random.Next(list.Count);
                T el = list[r];
                list.RemoveAt(r);
                yield return el;
            }
        }

        [TestMethod]
        public void _RandomTestAll()
        {
            Parallel.ForEach(ServerInfo.ServersInfos, server => 
            {
                try
                {
                    server.DownloadSeries();
                }
                catch
                {
                    TestContext.WriteLine("Exception while downloading series from server {0}", server);
                    return;
                }

                if (server.Series.Count() == 0)
                    TestContext.WriteLine("Server {0} has no series", server);
                
                Parallel.ForEach(TakeRandom(server.Series, 0.1), serie =>
                {
                    try
                    {
                        serie.DownloadChapters();
                    }
                    catch
                    {
                        TestContext.WriteLine("Exception while downloading chapters from serie {0}", serie);
                        return;
                    }

                    if (serie.Chapters.Count() == 0)
                        TestContext.WriteLine("Serie {0} has no chapters", serie);

                    Parallel.ForEach(TakeRandom(serie.Chapters, 0.1), chapter => 
                    {
                        try
                        {
                            chapter.DownloadPages();
                        }
                        catch
                        {
                            TestContext.WriteLine("Exception while downloading pages from chapter {0}", chapter);
                            return;
                        }

                        if (chapter.Pages.Count() == 0)
                            TestContext.WriteLine("Chapter {0} has no pages", chapter);

                        Parallel.ForEach(TakeRandom(chapter.Pages, 0.1), page =>
                        {
                            MemoryStream stream = null;

                            try
                            {
                                stream = page.GetImageStream();
                            }
                            catch
                            {
                                TestContext.WriteLine("Exception while downloading image from page {0}", page);
                                return;
                            }

                            if (stream.Length == 0)
                            {
                                TestContext.WriteLine("Image stream has zero size for page {0}", page);
                                return;
                            }

                            try
                            {
                                System.Drawing.Image.FromStream(stream);
                            }
                            catch
                            {
                                TestContext.WriteLine("Exception while creating image from stream for page {0}", page);
                                return;
                            }
                        });
                    });
                });
            });
        }
    }
}
