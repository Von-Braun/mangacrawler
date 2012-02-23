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
using System.Text.RegularExpressions;
using System.Net;
using TomanuExtensions.Utils;

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

        [TestCleanup]
        public void CheckError()
        {
            Assert.IsTrue(m_error == false);
        }

        private IEnumerable<Serie> TestServer(Server a_server, int a_count)
        {
            TestContext.WriteLine("Testing server {0}", a_server.Name);

            a_server.DownloadSeries();
            var series = a_server.Series;

            Crawler.DownloadWithRetry(() => new HtmlWeb().Load(a_server.URL));

            if (a_count > 0)
            {
                TestContext.WriteLine("Series, expected not less than {0}, finded: {1}", 
                    a_count, series.Count());

                if (series.Count() < a_count)
                    m_error = true;
            }
            else if (a_count == 0)
            {
                TestContext.WriteLine("series: {0}", series.Count());
                m_error = true;
            }

            Assert.IsTrue(a_server.Series.All(s => s.Title.Trim() == s.Title));
            Assert.IsTrue(a_server.Series.All(s => !String.IsNullOrWhiteSpace(s.Title)));

            return a_server.Series;
        }

        private IEnumerable<Chapter> TestSerie(Serie a_serie, int a_count, 
            bool a_ongoing = false)
        {
            TestContext.WriteLine("  Testing serie {0}", a_serie.Title);

            a_serie.DownloadChapters();
            var chapters = a_serie.Chapters;

            Crawler.DownloadWithRetry(() => new HtmlWeb().Load(a_serie.URL));

            if (!a_ongoing)
            {
                TestContext.WriteLine("  Chapters, expected {0}, finded: {1}", a_count,
                    chapters.Count());

                if (chapters.Count() != a_count)
                    m_error = true;
            }
            else
            {
                TestContext.WriteLine("  Chapters (ongoing), expected {0}, finded: {1}", 
                    a_count, chapters.Count());

                if (chapters.Count() < a_count)
                    m_error = true;
            }

            Assert.IsTrue(a_serie.Chapters.All(s => s.Title.Trim() == s.Title));
            Assert.IsTrue(a_serie.Chapters.All(s => !String.IsNullOrWhiteSpace(s.Title)));

            return chapters;
        }

        private List<Page> TestChapter(string a_title, Chapter a_chapter, int a_count, 
            bool a_ongoing = false)
        {
            if (a_ongoing)
                Assert.IsTrue(a_count == 0);

            TestContext.WriteLine("    Testing chapter {0}", a_chapter.Title);

            if (!a_ongoing)
            {
                if (a_title != a_chapter.Title)
                {
                    TestContext.WriteLine("      Bad chapter title");
                    TestContext.WriteLine("      Excepcted: >{0}<", a_title);
                    TestContext.WriteLine("      Is:        >{0}<", a_chapter.Title);
                }
            }

            Crawler.DownloadWithRetry(() => new HtmlWeb().Load(a_chapter.URL));

            a_chapter.CreateWork(".", false);
            a_chapter.AddPages(a_chapter.Serie.Server.Crawler.DownloadPages(a_chapter));

            var pages = a_chapter.Pages;

            if (!a_ongoing)
            {
                TestContext.WriteLine("    Pages, expected {0}, finded: {1}", 
                    a_count, pages.Count());

                if (pages.Count() != a_count)
                    m_error = true;
            }
            else
            {
                TestContext.WriteLine("    Ongoing");
            }

            Assert.IsTrue(a_chapter.Pages.All(s => s.Name.Trim() == s.Name));
            Assert.IsTrue(a_chapter.Pages.All(s => !String.IsNullOrWhiteSpace(s.Name)));

            return pages;
        }

        private void TestPage(Page a_page, string a_hash, bool a_ongoing = false)
        {
            Assert.IsTrue(a_hash != null);

            if (a_ongoing)
                Assert.IsTrue(a_hash == "");
            else
                Assert.IsTrue(a_hash != "");

            TestContext.WriteLine("        Testing page {0}", a_page.Name);

            var stream = a_page.GetImageStream();

            Assert.IsTrue(stream.Length > 0);

            System.Drawing.Image.FromStream(stream);
            stream.Position = 0;

            if (!a_ongoing)
            {
                string hash = Hash.CalculateSHA256(stream, true);
                if (a_hash != hash)
                {
                    TestContext.WriteLine("        Hash doestn't match");
                    TestContext.WriteLine("        Excepcted: >{0}<", a_hash);
                    TestContext.WriteLine("        Is:        >{0}<", hash);
                    m_error = true;
                }
            }
        }

        [TestMethod]
        public void AnimeSourceTest()
        {
            var series = TestServer(ServerList.Servers.First(s => s.Crawler.GetType() == typeof(AnimeSourceCrawler)), 53);

            {
                var chapters = TestSerie(series.First(
                    s => s.Title == "Kimagure Orange Road"), 167);

                var pages = TestChapter("KOR (Volume 18 Chapter 156 part 4 original ending)", chapters.First(), 4);

                TestPage(pages.First(),
                    "97895BDE-0DE30690-79305713-6CCF2D22-A5B83C1F-586BA24C-AC7DC3F7-8826E214");
                TestPage(pages.Last(),
                    "25476017-BDD69A52-FAEEC980-8B6D2BB0-F36BD832-8AFA63B9-335FCB0A-C4C6AE50");

                pages = TestChapter("KOR (Volume 1 Chapter 1)", chapters.Last(), 34);

                TestPage(pages.First(),
                    "D3DBFB1A-2BB55A85-FECE3DF7-766C73D1-F0A8C43C-282046C1-9E181C0A-C709F948");
                TestPage(pages.Last(),
                    "A09BB75E-43DBE864-1DA21C19-6D204131-81C9BDF6-790B8549-9825A636-35F9A95B");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Freezing"), 33, true);

                var pages = TestChapter("Volume 06 - Chapter 33", 
                    chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter("Volume 01 - Chapter 01", chapters.Last(), 40);

                TestPage(pages.First(),
                    "F5C2644E-C4F18D93-D1D6550F-ACFD6A90-B08606FD-C82BC561-5C6EE22E-D15A7CD8");
                TestPage(pages.Last(),
                    "07E027F6-FC08FFF0-B49F310E-6A03A5BE-C4F58938-A48DD527-DA2BED7F-22616F0E");
            }
        }

        [TestMethod]
        public void MangaAccessTest()
        {
            var series = TestServer(ServerList.Servers.First(s => s.Crawler.GetType() == typeof(MangaAccessCrawler)), 4054);

            {
                var chapters = TestSerie(series.First(s => s.Title == "07-Ghost"), 66);

                var pages = TestChapter("07-Ghost chapter 77", chapters.First(), 31);

                TestPage(pages.First(),
                    "0BB04FF3-C33BBF39-97049314-589C07D2-820CB8D7-16346414-C05326F7-574C3331");
                TestPage(pages.Last(),
                    "63982BB5-8F0E233A-C4CA414F-50050B54-442DA223-397E1AEB-01FDA4E9-B19440BC");

                pages = TestChapter("", chapters.Last(), 46);

                TestPage(pages.First(),
                    "CA4F4D23-052C7AA9-196ED72C-E75DE684-F8C5F952-FC46FB35-6D4C9BC3-7BCF44F4");
                TestPage(pages.Last(),
                    "9B9DC441-75C7EBB8-32D37072-19333C96-6CC8A33C-01B7AC94-22DB85C1-0AB772C5");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Fairy Tail"), 272, true);

                var pages = TestChapter("Fairy Tail chapter 271", chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter("Fairy Tail chapter 1", chapters.Last(), 73);

                TestPage(pages.First(),
                    "2485C92C-3EDCBE95-84C5A4A2-6099F511-6926FF1A-16A4E759-E7342F1E-5842833D");
                TestPage(pages.Last(),
                    "BB71EA96-80D001F8-B7244F62-32C002BF-4F911F96-DF6DBEBE-860025D9-D88DC9EE");
            }
        }

        [TestMethod]
        public void MangaFoxTest()
        {
            var series = TestServer(ServerList.Servers.First(s => s.Crawler.GetType() == typeof(MangaFoxCrawler)), 8715);
            
            {
                var chapters = TestSerie(series.First(s => s.Title == ".hack//G.U.+"), 26);
                
                var pages = TestChapter(".hack//G.U.+ 1", chapters.Last(), 68);
            
                TestPage(pages.First(),
                    "B9A430BB-6B5AA874-047CC3AF-FFCD408A-5F51204A-F12E2E31-BE055615-B51E9E82");
                TestPage(pages.Last(), 
                    "A08602B0-41A27AAD-D870271E-F8AD256A-68D2C903-3C775B39-DF207BB2-95D1C137");
            
                pages = TestChapter("", chapters.First(), 33);
            
                TestPage(pages.First(),
                    "C72F3665-DB886AD8-018D7DA5-ADA0CE86-78E41820-DA68487A-07889638-6704026B");
                TestPage(pages.Last(),
                    "B706A9A5-0198C7AD-6A18E4F8-9CF771A2-97E195F3-017D473D-68EBC9C9-308DB705");
            }
            
            {
                var chapters = TestSerie(series.First(s => s.Title == "(G) Edition"), 9, true);

                var pages = TestChapter("(G) Edition 1", chapters.Last(), 17);
            
                TestPage(pages.First(),
                    "0D5F0D20-2C4F785A-257D462C-9E84E429-972ACF39-DC6F3BCD-7CAC14F9-6092495E");
                TestPage(pages.Last(),
                    "49D383E3-432985FF-2EE91FC9-E4E53C48-3737FA32-488494B7-61794E67-49F79ECA");
            
                pages = TestChapter("", chapters.First(), 0, true);
            
                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }
            
            {
                var chapters = TestSerie(series.First(s => s.Title == 
                    "Samayoeru Ookami ni Junai wo"), 1, true);
            
                Assert.IsTrue(chapters.Count() == 1);

                var pages = TestChapter("Samayoeru Ookami ni Junai wo 1", chapters.First(), 0); 
            
                Assert.IsTrue(pages.Count() == 0);
            }

            Assert.IsTrue(series.All(s => s.Title != "[switch]"));
        }

        [TestMethod]
        public void MangaRunTest()
        {
            var series = TestServer(ServerList.Servers.First(s => s.Crawler.GetType() == typeof(MangaRunCrawler)), 323);
            
            {
                var chapters = TestSerie(series.First(s => s.Title == "666satan"), 78);

                var pages = TestChapter("666satan 076", chapters.First(), 51);
            
                TestPage(pages.First(),
                    "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB");
                TestPage(pages.Last(),
                    "0D8475C4-E5D98687-C4DA831B-0D8F7003-6DF7F2EE-3747FC41-1E56B602-AF65CE0A");

                pages = TestChapter("666satan 001a", chapters.Last(), 25);
            
                TestPage(pages.First(),
                    "4C1EBC9E-132DC56A-56B47BD6-C567DE7A-9354C577-D2C7E01E-18B7209B-CFDC1D43");
                TestPage(pages.Last(),
                    "C7BBA2D4-579AD7C0-38DE23A8-E7BDC94A-0D1480F3-50D22B2F-F759BF7B-E684F834");
            }
            
            {
                var chapters = TestSerie(series.First(s => s.Title == "bleach"), 450, true);
            
                var pages = TestChapter("", chapters.First(), 0, true);
            
                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter("bleach 001", chapters.Last(), 57);
            
                TestPage(pages.First(),
                    "8D78D814-791583E2-19F0FC41-460F600B-982ABBF0-6278B2A9-3D6D5112-ADB86FA6");
                TestPage(pages.Last(),
                    "E9B3C85A-C85A9F3B-FB3653FD-599AB0A8-D2B58283-DD48A599-AE5CB86F-2DFDA740");
            }
        }

        [TestMethod]
        public void MangaShareTest()
        {
            var series = TestServer(ServerList.Servers.First(s => s.Crawler.GetType() == typeof(MangaShareCrawler)), 187);

            {
                var chapters = TestSerie(series.First(s => s.Title == "Akumetsu"), 73);

                var pages = TestChapter("001 - Super Elite", chapters.Last(), 50);
            
                TestPage(pages.First(),
                    "E99D18EA-8B4704C3-707E8CF1-C7EB5D28-782DA8AA-3EA987A5-D5063C14-BB079B2A");
                TestPage(pages.Last(),
                    "29A6D2AD-58F2DDB4-70C8118A-C29198AC-0E32A035-D953D134-E7911F08-5EF8E293");
            
                pages = TestChapter("", chapters.First(), 21);
            
                TestPage(pages.First(),
                    "05BA460B-6DBB06B4-3810F860-3A7113A0-3AC7AF80-5021D7E3-6F8D77E8-3A39027A");
                TestPage(pages.Last(),
                    "67C8BF5C-833904D9-1C6AAD79-118BB4BE-DAC43AC7-E4F5B24A-3FD5EF2A-9B2FB526");
            }
            
            {
                var chapters = TestSerie(series.First(s => s.Title == "Fairy Tail"), 263, true);

                var pages = TestChapter("001 - Fairy Tail", chapters.Reverse().Skip(1).First(), 74);
            
                TestPage(pages.First(),
                    "3BBFA539-2392866D-E726FA42-BF766868-893F9BEC-497A9054-2B142CB9-F7D06026");
                TestPage(pages.Last(),
                    "E6EF1C9D-93352A75-5B8BB852-928773E1-81F7828C-36354007-040CF780-33A252CF");
            
                pages = TestChapter("", chapters.Skip(1).First(), 0, true);
            
                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }
        }

        [TestMethod]
        public void MangaVolumeTest()
        {
            var series = TestServer(ServerList.Servers.First(s => s.Crawler.GetType() == typeof(MangaVolumeCrawler)), 1141);

            {
                var chapters = TestSerie(series.First(s => s.Title == "666 Satan"), 76);

                var pages = TestChapter("666 Satan 1", chapters.Last(), 15);

                TestPage(pages.First(), 
                    "D7AA1E8F-6D1F2766-40C81052-82223AB4-9D722C24-16429545-4B4635A6-26D497A7");
                TestPage(pages.Last(), 
                    "3B443FB1-A263AE7D-E81A9ADA-5FF3AD75-F3B6CB50-011A31C2-9D214533-C2EB7E8B");

                pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Bleach"), 0);
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Freezing"), 76, true);

                var pages = TestChapter("", chapters.Last(), 80);

                TestPage(pages.First(),
                    "7007F030-E9172FE1-540F1CC8-8547B281-ED357A63-4F4ED165-1D2D5706-5D040F1B");
                TestPage(pages.Last(),
                    "7180D10B-7B88A853-628DAD1D-016B18E8-1149D959-FA507C37-4132A670-B2E05E3C");

                pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }
        }

        [TestMethod]
        public void OtakuWorksTest()
        {
            var series = TestServer(ServerList.Servers.First(s => s.Crawler.GetType() == typeof(OtakuWorksCrawler )), 4901);

            {
                var chapters = TestSerie(series.First(s => s.Title == "Ai Kora"), 92);

                var pages = TestChapter("", chapters.First(), 23);

                TestPage(pages.First(),
                    "0C5EFB64-5E316B49-2119FAB9-E8AFFC17-B379B298-25D0FC18-BD612E89-6DEA005E");
                TestPage(pages.Last(),
                    "2FBEA2C1-6001A412-816B7D2E-586AAFDC-A0832D5F-4DC80437-05BF14D5-F428DBAE");

                pages = TestChapter("", chapters.First(), 23);

                TestPage(pages.First(),
                    "0C5EFB64-5E316B49-2119FAB9-E8AFFC17-B379B298-25D0FC18-BD612E89-6DEA005E");
                TestPage(pages.Last(),
                    "2FBEA2C1-6001A412-816B7D2E-586AAFDC-A0832D5F-4DC80437-05BF14D5-F428DBAE");
            }

            {
                var chapters = TestSerie(series.First(
                    s => s.Title == ".hack//G.U. The World"), 0);
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Bleach"), 216, true);

                var pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter("", chapters.Last(), 187);

                TestPage(pages.First(),
                    "8D78D814-791583E2-19F0FC41-460F600B-982ABBF0-6278B2A9-3D6D5112-ADB86FA6");
                TestPage(pages.Last(),
                    "6FD444E5-354E6D3F-120CF0AD-B9468CBF-D9D02BFB-D4DA860B-56EE5DAD-7FB794CE");
            }
        }

        [TestMethod]
        public void OurMangaTest()
        {
            var series = TestServer(ServerList.Servers.First(s => s.Crawler.GetType() == typeof(OurMangaCrawler)), 2140);

            {
                var chapters = TestSerie(series.First(s => s.Title == "090 - Eko To Issho"), 61);

                var pages = TestChapter("", chapters.Last(), 25);

                TestPage(pages.First(),
                    "7065E9C9-E1BC872F-3D3D72A7-6D4F8EB6-6C1B2A3F-1D64D446-E37EC166-BC51AA0E");
                TestPage(pages.Last(),
                    "E088EAE7-8D5A8A67-0A35FC77-BE548D2D-F6BBB3CE-02412A6E-E18F65B5-8B050E7E");

                pages = TestChapter("", chapters.First(), 14);

                TestPage(pages.First(),
                    "17D80766-814B5B54-3B19921C-F20BA640-D8E81FF7-5F9CAB2A-AF5C85C4-591848ED");
                TestPage(pages.Last(),
                    "BC623819-7E7C4A77-48D825B6-6FB898F1-A5CD0904-2E5E93AB-EE2F4FE4-D1E65FFC");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Fairy Tail"), 228, true);

                var pages = TestChapter("", chapters.Last(), 74);

                TestPage(pages.First(),
                    "6BFDF3BC-3DDED90E-47CF0B60-10FE37E5-EC0EA244-D55922B5-5421E513-51C69338");
                TestPage(pages.Last(),
                    "7CB8D9E4-334CBC77-6091EE52-D2BA5824-5D4CDC0E-594E1EC8-BFEEA60F-04859424");

                pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }
        }

        [TestMethod]
        public void SpectrumNexusTest()
        {
            var series = TestServer(ServerList.Servers.First(s => s.Crawler.GetType() == typeof(SpectrumNexusCrawler)), 119);

            {
                var chapters = TestSerie(series.First(
                    s => s.Title == "Fullmetal Alchemist"), 27);

                var pages = TestChapter("", chapters.First(), 212);

                TestPage(pages.First(),
                    "7E9DF42D-ED8C16A2-E67248FF-F57702BB-2ED646B3-0C9C60D3-83506320-328A86D4");
                TestPage(pages.Last(),
                    "2C9D7E26-477ECEB2-F29C4CFC-7F5979B3-84DF750C-D270B456-B72AC73E-9012517F");

                pages = TestChapter("", chapters.Last(), 177);

                TestPage(pages.First(),
                    "3EB9F663-30BFA500-B792DC8E-63CB6950-E20635F7-4A7B0069-4E5E8B18-C36321E5");
                TestPage(pages.Last(),
                    "4465CA14-0C186340-8C3DF8D0-13409713-C81030F1-344510E6-BBFB2A9C-308AC1F9");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Bleach"), 65, true);

                var pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter("", chapters.Last(), 187);

                TestPage(pages.First(),
                    "62A96AF0-2818D19A-0A35DAA7-FD513701-6E6670E0-663EC84E-AAA3579D-CC6B8733");
                TestPage(pages.Last(),
                    "7B541E33-09E0105D-A3B4EEE4-B2E6B819-330AF88E-8FA96087-BCE7DFE4-59A9CA55");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Air Gear"), 51, true);

                var pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter("", chapters.Last(), 188);

                TestPage(pages.First(), 
                    "CBF5F8D4-7619396E-CED74CA5-5897E090-E302C4E5-355977BB-0E971488-F949D0A9");
                TestPage(pages.Last(), 
                    "1AEAE9E8-496A3A3B-23E9F5A0-68444A5A-E4E6B4D1-8885B265-A7D22E92-B2B00CF1");
            }
        }

        [TestMethod]
        public void StopTazmoTest()
        {
            var series = TestServer(ServerList.Servers.First(s => s.Crawler.GetType() == typeof(StopTazmoCrawler)), 1902);

            {
                var chapters = TestSerie(series.First(s => s.Title == "666 Satan"), 78);

                var pages = TestChapter("", chapters.First(), 51);

                TestPage(pages.First(),
                    "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB");
                TestPage(pages.Last(),
                    "0D8475C4-E5D98687-C4DA831B-0D8F7003-6DF7F2EE-3747FC41-1E56B602-AF65CE0A");

                pages = TestChapter("", chapters.Last(), 25);

                TestPage(pages.First(),
                    "4C1EBC9E-132DC56A-56B47BD6-C567DE7A-9354C577-D2C7E01E-18B7209B-CFDC1D43");
                TestPage(pages.Last(),
                    "C7BBA2D4-579AD7C0-38DE23A8-E7BDC94A-0D1480F3-50D22B2F-F759BF7B-E684F834");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Bleach"), 438, true);

                var pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter("", chapters.Last(), 57);

                TestPage(pages.First(),
                    "8D78D814-791583E2-19F0FC41-460F600B-982ABBF0-6278B2A9-3D6D5112-ADB86FA6");
                TestPage(pages.Last(),
                    "41838061-F5379AD8-FF615340-5FEF5C07-C9A68FF7-6F643947-7A203C17-5308D8A4");
            }
        }

        [TestMethod]
        public void UnixMangaTest()
        {
            var series = TestServer(ServerList.Servers.First(s => s.Crawler.GetType() == typeof(UnixMangaCrawler)), 1572);

            {
                var chapters = TestSerie(series.First(s => s.Title == "Bleach"), 460, true);

                var pages = TestChapter("", chapters.Last(), 8);

                TestPage(pages.First(), 
                    "262ED4DC-7C5B3D1F-B0918BB3-3DACD75A-60D2119B-17A29A97-04E12601-051312D8");
                TestPage(pages.Last(), 
                    "B8177447-C74F7AA8-0521B396-2B7119F8-E173C00B-27F6B894-C997C530-2566F1E6");

                pages = TestChapter("", chapters.First(), 15);

                TestPage(pages.First(), 
                    "99285B2F-50490ECA-D0D8A9E4-818032F2-186D0AC4-B75331FD-3818B301-FA0F2CA2");
                TestPage(pages.Last(), 
                    "52B1C84E-03455C85-1C96BA7F-6F4FCF33-B839109A-E6F699D6-E9400BCB-13CC5B90");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "666 Satan"), 80);

                var pages = TestChapter("", chapters.Last(), 25);

                TestPage(pages.First(), 
                    "408A4442-0CDE316E-1E41544D-B8DFB08C-402CDC6E-9128EF2E-6B2E1ECE-8496E9D7");
                TestPage(pages.Last(), 
                    "1595E11D-A302BA42-7A33F0E1-4ADEF4BD-C5365FBF-0B89123E-B61C9E8B-E7E08A2D");

                pages = TestChapter("", chapters.First(), 30);

                TestPage(pages.First(), 
                    "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB");
                TestPage(pages.Last(), 
                    "AC9DF68B-C07F380B-DBBCF4EA-4A79C0DB-EF00A7DC-5AF91C46-56E1F179-90EDB30F");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Kamisama no Tsukurikata"), 17);

                var pages = TestChapter("", chapters.Last(), 30);

                TestPage(pages.First(), 
                    "BA438114-F68B9CDD-42198F17-8C72B2D3-78882EC9-C9A2A995-BFE19D26-FDD0A9C5");
                TestPage(pages.Last(), 
                    "A95CE474-FBFCFCEE-F98AB70E-C3B341C3-609F8871-7AD9F33C-A0EF7BFB-1BDF5B8D");

                pages = TestChapter("", chapters.First(), 30);

                TestPage(pages.First(), 
                    "49C0AACF-CEFCBFD1-626AF6D5-3E3F444A-252DCBCC-CB109F3A-BEB2FCB7-F2C39DA0");
                TestPage(pages.Last(), 
                    "FFC3EBB9-9BDF5759-DE87C6D6-E2634C38-8982EC38-34CF67C6-2336B18D-28F7862E");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "16 Sai Kissu Complete"), 1);

                var pages = TestChapter("", chapters.First(), 30);

                TestPage(pages.First(), 
                    "ABC41912-39037D79-66ED232B-E4F0EC7B-907F9E02-18CB640D-FF6E32D2-68750D27");
                TestPage(pages.Last(), 
                    "3048567C-DF4072AC-01C71054-734C7736-BB6CD48E-92E1EA56-BB5DD04A-4C1048EC");
            }
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
            Parallel.ForEach(ServerList.Servers,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = ServerList.Servers.Count(),
                    TaskScheduler = new CustomTaskScheduler.InnerCustomTaskScheduler(ServerList.Servers.Count())

                },
                server => 
            {
                try
                {
                    server.DownloadSeries();
                }
                catch
                {
                    TestContext.WriteLine("{0} - Exception while downloading series from server", 
                        server);
                }

                if (server.Series.Count() == 0)
                {
                    TestContext.WriteLine("{0} - Server have no series", server);
                }
                
                Parallel.ForEach(TakeRandom(server.Series, 0.1),
                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer, 
                        TaskScheduler = server.Scheduler[Priority.Series]
                    },
                    serie =>
                {
                    try
                    {
                        serie.DownloadChapters();
                    }
                    catch
                    {
                        TestContext.WriteLine(
                            "{0} - Exception while downloading chapters from serie", serie);
                    }

                    if (serie.Chapters.Count() == 0)
                    {
                        TestContext.WriteLine("{0} - Serie has no chapters", serie);
                    }

                    Parallel.ForEach(TakeRandom(serie.Chapters, 0.1),
                        new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer,
                            TaskScheduler = serie.Server.Scheduler[Priority.Chapters]
                        },
                        (chapter) => 
                    {
                        chapter.CreateWork(".", false);
                        try
                        {
                            chapter.CreateWork(".", false);
                            chapter.AddPages(chapter.Serie.Server.Crawler.DownloadPages(chapter));
                        }
                        catch
                        {
                            TestContext.WriteLine(
                                "{0} - Exception while downloading pages from chapter", chapter);
                        }

                        if (chapter.Pages.Count() == 0)
                        {
                            TestContext.WriteLine("{0} - Chapter have no pages", chapter);
                        }

                        Parallel.ForEach(TakeRandom(chapter.Pages, 0.1), 
                            new ParallelOptions()
                            {
                                MaxDegreeOfParallelism = chapter.Serie.Server.Crawler.MaxConnectionsPerServer,
                                TaskScheduler = chapter.Serie.Server.Scheduler[Priority.Pages]
                            }, 
                            (page) =>
                        {
                            MemoryStream stream = null;

                            try
                            {
                                stream = page.GetImageStream();
                            }
                            catch
                            {
                                TestContext.WriteLine(
                                    "{0} - Exception while downloading image from page", page);
                            }

                            if (stream.Length == 0)
                            {
                                TestContext.WriteLine(
                                    "{0} - Image stream is zero size for page", page);
                            }

                            try
                            {
                                System.Drawing.Image.FromStream(stream);
                            }
                            catch
                            {
                                TestContext.WriteLine(
                                    "{0} - Exception while creating image from stream for page", page);
                            }
                        });
                    });
                });
            });
        }
    }
}
