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

        [TestCleanup]
        public void CheckError()
        {
            Assert.IsTrue(m_error == false);
        }

        private IEnumerable<SerieInfo> TestServer(ServerInfo a_info, int a_count)
        {
            TestContext.WriteLine("Testing server {0}", a_info.Name);

            a_info.DownloadSeries();
            var series = a_info.Series;

            new HtmlWeb().Load(a_info.URL);

            if (a_count > 0)
            {
                TestContext.WriteLine("Series, expected more than {0}, finded: {1}", 
                    a_count, series.Count());

                if (series.Count() < a_count)
                    m_error = true;
            }
            else if (a_count == 0)
            {
                TestContext.WriteLine("series: {0}", series.Count());
                m_error = true;
            }

            Assert.IsTrue(a_info.Series.All(s => s.Name.Trim() == s.Name));
            Assert.IsTrue(a_info.Series.All(s => !String.IsNullOrWhiteSpace(s.Name)));

            return a_info.Series;
        }

        private IEnumerable<ChapterInfo> TestSerie(SerieInfo a_info, int a_count, 
            bool a_ongoing = false)
        {
            TestContext.WriteLine("  Testing serie {0}", a_info.Name);

            a_info.DownloadChapters();
            var chapters = a_info.Chapters;

            new HtmlWeb().Load(a_info.URL);

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

            Assert.IsTrue(a_info.Chapters.All(s => s.Name.Trim() == s.Name));
            Assert.IsTrue(a_info.Chapters.All(s => !String.IsNullOrWhiteSpace(s.Name)));

            return chapters;
        }

        private IEnumerable<PageInfo> TestChapter(ChapterInfo a_info, int a_count, 
            bool a_ongoing = false)
        {
            if (a_ongoing)
                Assert.IsTrue(a_count == 0);

            TestContext.WriteLine("    Testing chapter {0}", a_info.Name);

            new HtmlWeb().Load(a_info.URL);

            a_info.DownloadPages();
            var pages = a_info.Pages;

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

            Assert.IsTrue(a_info.Pages.All(s => s.Name.Trim() == s.Name));
            Assert.IsTrue(a_info.Pages.All(s => !String.IsNullOrWhiteSpace(s.Name)));

            return pages;
        }

        private void TestPage(PageInfo a_info, string a_hash, bool a_ongoing = false)
        {
            Assert.IsTrue(a_hash != null);

            if (a_ongoing)
                Assert.IsTrue(a_hash == "");
            else
                Assert.IsTrue(a_hash != "");

            TestContext.WriteLine("        Testing page {0}", a_info.Name);

            var stream = a_info.GetImageStream();

            Assert.IsTrue(stream.Length > 0);

            System.Drawing.Image.FromStream(stream);
            stream.Position = 0;

            if (!a_ongoing)
            {
                string hash = GetHash(stream);
                if (a_hash != hash)
                {
                    TestContext.WriteLine("        Hash doestn't match, finded: {0}", hash);
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
            var series = TestServer(ServerInfo.AnimeSource, 53);

            {
                var chapters = TestSerie(series.First(
                    s => s.Name == "Kimagure Orange Road"), 167);

                var pages = TestChapter(chapters.First(), 34);

                TestPage(pages.First(),
                    "38FBB6A5-D0D7B833-A2010C4A-BFC54B7F-286C7FD0-F6FB9C6B-5F5FC35F-16F91CCA");
                TestPage(pages.Last(),
                    "A1E7E121-0CCB6F23-9F29EE6A-72552092-5447D56B-5FCF9784-5F398237-B8DA9C61");

                pages = TestChapter(chapters.Last(), 4);

                TestPage(pages.First(), 
                    "97895BDE-0DE30690-79305713-6CCF2D22-A5B83C1F-586BA24C-AC7DC3F7-8826E214");
                TestPage(pages.Last(), 
                    "25476017-BDD69A52-FAEEC980-8B6D2BB0-F36BD832-8AFA63B9-335FCB0A-C4C6AE50");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "AIKI"), 61, true);

                var pages = TestChapter(chapters.Last(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter(chapters.First(), 36);

                TestPage(pages.First(), 
                    "4D5FA238-A53BE9A3-C14F7599-922A940E-879DBC4A-C2894E12-98C986DA-A7ACBDA2");
                TestPage(pages.Last(), 
                    "A9793054-13436650-7ACCE952-2CDF99D0-014244F4-82FEF5D6-A7A7438D-C5D038C9");
            }
        }

        [TestMethod]
        public void BleachExileTest()
        {
            var series = TestServer(ServerInfo.BleachExile, 1147);

            {
                var chapters = TestSerie(series.First(s => s.Name == "07-Ghost"), 51);

                var pages = TestChapter(chapters.First(), 46);

                TestPage(pages.First(), 
                    "982CACB9-37785E42-210238AC-E6F8C57B-19275887-4195D35D-38743C3B-E4459B46");
                TestPage(pages.Last(), 
                    "917F2F90-74CA9047-8BDCA29E-5D05F5B2-5424EFB7-63F3E434-3585DF71-F68B1F0A");

                pages = TestChapter(chapters.Last(), 26);

                TestPage(pages.First(), 
                    "A06551F8-747B8BE0-EA1E49C3-74D32C8F-39860B65-8EA2DFF6-1A8F7C6E-618C1DEB");
                TestPage(pages.Last(), 
                    "59AC6FA3-E9531AC0-3A3CC33B-8EC6B536-294957CB-B2130B92-EDCB0BDF-BCE63099");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "Fairy Tail"), 224, true);

                var pages = TestChapter(chapters.First(), 73);

                TestPage(pages.First(), 
                    "53F08C59-6EBA8CFA-7C56D7D6-85548233-9B398E8C-27CFD069-71F83F4D-21835235");
                TestPage(pages.Last(), 
                    "BB71EA96-80D001F8-B7244F62-32C002BF-4F911F96-DF6DBEBE-860025D9-D88DC9EE");

                pages = TestChapter(chapters.Last(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }
        }

        [TestMethod]
        public void MangaFoxTest()
        {
            var series = TestServer(ServerInfo.MangaFox, 7070);

            {
                var chapters = TestSerie(series.First(s => s.Name == ".hack//G.U.+"), 26);

                var pages = TestChapter(chapters.Last(), 68);

                TestPage(pages.First(), 
                    "BB93A387-185223CB-8EC50E70-899AA5F4-1B70222B-A39ED542-BAA71897-C5ECB461");
                TestPage(pages.Last(), 
                    "A08602B0-41A27AAD-D870271E-F8AD256A-68D2C903-3C775B39-DF207BB2-95D1C137");

                pages = TestChapter(chapters.First(), 33);

                TestPage(pages.First(), 
                    "454E0B8D-03CA4892-BEE861B4-ABE79154-56FB60F2-8910BE2A-BDC107C0-9388DED0");
                TestPage(pages.Last(), 
                    "DED6595F-377DBE4F-D204100F-4A697979-A717AA9D-E24314C3-4E209759-650680B9");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "(G) Edition"), 3, true);

                var pages = TestChapter(chapters.Last(), 17);

                TestPage(pages.First(), 
                    "6CC9C11F-4E614BFE-CB4AF33F-F4344834-717C52C9-C67672EB-B2CD6178-A3C24814");
                TestPage(pages.Last(), 
                    "0CBD3787-E149EF52-00065BE3-1AD2C925-29D905EC-581835B8-DC637B3D-2ACEC1CD");

                pages = TestChapter(chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "[switch]"), -1);

                Assert.IsTrue(chapters.Count() == 0);
            }
        }

        [TestMethod]
        public void MangaRunTest()
        {
            var series = TestServer(ServerInfo.MangaRun, 374);

            {
                var chapters = TestSerie(series.First(s => s.Name == "666satan"), 78);

                var pages = TestChapter(chapters.Last(), 51);

                TestPage(pages.First(),
                    "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB");
                TestPage(pages.Last(),
                    "0D8475C4-E5D98687-C4DA831B-0D8F7003-6DF7F2EE-3747FC41-1E56B602-AF65CE0A");

                pages = TestChapter(chapters.First(), 25);

                TestPage(pages.First(),
                    "4C1EBC9E-132DC56A-56B47BD6-C567DE7A-9354C577-D2C7E01E-18B7209B-CFDC1D43");
                TestPage(pages.Last(),
                    "C7BBA2D4-579AD7C0-38DE23A8-E7BDC94A-0D1480F3-50D22B2F-F759BF7B-E684F834");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "bleach"), 434, true);

                var pages = TestChapter(chapters.Last(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter(chapters.First(), 57);

                TestPage(pages.First(),
                    "8D78D814-791583E2-19F0FC41-460F600B-982ABBF0-6278B2A9-3D6D5112-ADB86FA6");
                TestPage(pages.Last(),
                    "41838061-F5379AD8-FF615340-5FEF5C07-C9A68FF7-6F643947-7A203C17-5308D8A4");
            }
        }

        [TestMethod]
        public void MangaShareTest()
        {
            var series = TestServer(ServerInfo.MangaShare, 143);

            {
                var chapters = TestSerie(series.First(s => s.Name == "666 Satan"), 77);

                var pages = TestChapter(chapters.Last(), 80);

                TestPage(pages.First(),
                    "D7654CB6-AC4813C0-5B985B91-0E15FEC9-193F9C48-D78B8A77-67A9B4A4-F1C0C337");
                TestPage(pages.Last(),
                    "96EF8686-80AFE4C8-8A149FC6-AA587E14-32F557E5-B65E4FD1-CB49D80E-563E2C97");

                pages = TestChapter(chapters.First(), 51);

                TestPage(pages.First(),
                    "654005EA-44B03AD4-6B7D1004-5CAE6C14-13B1B8C5-BB09114D-C4240FEE-C084D426");
                TestPage(pages.Last(),
                    "B17D5A70-A3723C4C-FE5D6B46-6E66E8C8-ABD57067-14DB3D66-4C373CA1-60EF1D32");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "Bleach"), 452, true);

                var pages = TestChapter(chapters.Last(), 57);

                TestPage(pages.First(),
                    "3CA3E1B7-2DBE8106-419613CB-FF35FF1E-D7DD9B0C-A533F92E-2ECB098F-3E4B5F96");
                TestPage(pages.Last(),
                    "61482BCB-6B9C1FDA-8CCC87BF-7B53CCDB-4F711B76-195745D2-B9DC5217-94F9CB39");

                pages = TestChapter(chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }
        }

        [TestMethod]
        public void MangaToshokanTest()
        {
            var series = TestServer(ServerInfo.MangaToshokan, 1150);

            {
                var chapters = TestSerie(series.First(s => s.Name == "Angel Shop"), 15);

                var pages = TestChapter(chapters.Last(), 56);

                TestPage(pages.First(), 
                    "8A2B473E-BD8752D4-3616D223-8B288E32-E054EE8A-3AE7EACC-E652BC95-2478081D");
                TestPage(pages.Last(), 
                    "B8781498-A9EA2DCA-255D3822-A2F9A8E8-306A1FE0-AA076D04-92707D2D-B43A84C4");

                pages = TestChapter(chapters.First(), 29);

                TestPage(pages.First(), 
                    "6FADD86A-1A648C38-23BC157C-FB48A97F-05A3A918-B9BEC2B2-3AEEDA36-2AE299AD");
                TestPage(pages.Last(), 
                    "BD3117E2-E4B9B41C-7DF13F9B-73C10CCE-62D2B126-07BE5B61-D92280FF-0AE70516");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "1/2 Prince"), 50, true);

                var pages = TestChapter(chapters.Last(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter(chapters.First(), 71);

                TestPage(pages.First(), 
                    "456C6422-8F18E283-344F9FAE-2DAF8AAC-F4ED58BD-48404EF6-1B518ED6-1982DAD3");
                TestPage(pages.Last(), 
                    "B501E1E1-77699CF0-1C577390-C7384159-DCA79666-66F9BA6E-D2E3AE43-4877B517");
            }
        }

        [TestMethod]
        public void MangaVolumeTest()
        {
            var series = TestServer(ServerInfo.MangaVolume, 1076);

            {
                var chapters = TestSerie(series.First(s => s.Name == "3x3 Eyes"), 414, true);

                var pages = TestChapter(chapters.Last(), 15);

                TestPage(pages.First(), 
                    "D7AA1E8F-6D1F2766-40C81052-82223AB4-9D722C24-16429545-4B4635A6-26D497A7");
                TestPage(pages.Last(), 
                    "3B443FB1-A263AE7D-E81A9ADA-5FF3AD75-F3B6CB50-011A31C2-9D214533-C2EB7E8B");

                pages = TestChapter(chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "Bleach"), -1);

                Assert.IsTrue(chapters.Count() == 0);
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "666 Satan"), 76, true);

                var pages = TestChapter(chapters.Last(), 80);

                TestPage(pages.First(),
                    "7007F030-E9172FE1-540F1CC8-8547B281-ED357A63-4F4ED165-1D2D5706-5D040F1B");
                TestPage(pages.Last(),
                    "7180D10B-7B88A853-628DAD1D-016B18E8-1149D959-FA507C37-4132A670-B2E05E3C");

                pages = TestChapter(chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }
        }

        [TestMethod]
        public void OtakuWorksTest()
        {
            var series = TestServer(ServerInfo.OtakuWorks, 4901);

            {
                var chapters = TestSerie(series.First(s => s.Name == "Ai Kora"), 92);

                var pages = TestChapter(chapters.First(), 23);

                TestPage(pages.First(),
                    "0C5EFB64-5E316B49-2119FAB9-E8AFFC17-B379B298-25D0FC18-BD612E89-6DEA005E");
                TestPage(pages.Last(),
                    "2FBEA2C1-6001A412-816B7D2E-586AAFDC-A0832D5F-4DC80437-05BF14D5-F428DBAE");

                pages = TestChapter(chapters.First(), 23);

                TestPage(pages.First(),
                    "0C5EFB64-5E316B49-2119FAB9-E8AFFC17-B379B298-25D0FC18-BD612E89-6DEA005E");
                TestPage(pages.Last(),
                    "2FBEA2C1-6001A412-816B7D2E-586AAFDC-A0832D5F-4DC80437-05BF14D5-F428DBAE");
            }

            {
                var chapters = TestSerie(series.First(
                    s => s.Name == ".hack//G.U. The World"), 0, true);

                Assert.IsTrue(chapters.Count() == 0);
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "Bleach"), 216, true);

                var pages = TestChapter(chapters.First(), 0);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter(chapters.Last(), 187);

                TestPage(pages.First(),
                    "8D78D814-791583E2-19F0FC41-460F600B-982ABBF0-6278B2A9-3D6D5112-ADB86FA6");
                TestPage(pages.Last(),
                    "6FD444E5-354E6D3F-120CF0AD-B9468CBF-D9D02BFB-D4DA860B-56EE5DAD-7FB794CE");
            }
        }

        [TestMethod]
        public void OurMangaTest()
        {
            var series = TestServer(ServerInfo.OurManga, 2140);

            {
                var chapters = TestSerie(series.First(s => s.Name == "090 - Eko To Issho"), 61);

                var pages = TestChapter(chapters.Last(), 25);

                TestPage(pages.First(),
                    "7065E9C9-E1BC872F-3D3D72A7-6D4F8EB6-6C1B2A3F-1D64D446-E37EC166-BC51AA0E");
                TestPage(pages.Last(),
                    "E088EAE7-8D5A8A67-0A35FC77-BE548D2D-F6BBB3CE-02412A6E-E18F65B5-8B050E7E");

                pages = TestChapter(chapters.First(), 14);

                TestPage(pages.First(),
                    "17D80766-814B5B54-3B19921C-F20BA640-D8E81FF7-5F9CAB2A-AF5C85C4-591848ED");
                TestPage(pages.Last(),
                    "BC623819-7E7C4A77-48D825B6-6FB898F1-A5CD0904-2E5E93AB-EE2F4FE4-D1E65FFC");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "Fairy Tail"), 228, true);

                var pages = TestChapter(chapters.Last(), 74);

                TestPage(pages.First(),
                    "6BFDF3BC-3DDED90E-47CF0B60-10FE37E5-EC0EA244-D55922B5-5421E513-51C69338");
                TestPage(pages.Last(),
                    "7CB8D9E4-334CBC77-6091EE52-D2BA5824-5D4CDC0E-594E1EC8-BFEEA60F-04859424");

                pages = TestChapter(chapters.First(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }
        }

        [TestMethod]
        public void SpectrumNexusTest()
        {
            var series = TestServer(ServerInfo.SpectrumNexus, 139);

            {
                var chapters = TestSerie(series.First(s => s.Name == "Bleach"), 65, true);

                var pages = TestChapter(chapters.Last(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter(chapters.First(), 187);

                TestPage(pages.First(),
                    "62A96AF0-2818D19A-0A35DAA7-FD513701-6E6670E0-663EC84E-AAA3579D-CC6B8733");
                TestPage(pages.Last(),
                    "7B541E33-09E0105D-A3B4EEE4-B2E6B819-330AF88E-8FA96087-BCE7DFE4-59A9CA55");
            }

            {
                var chapters = TestSerie(series.First(
                    s => s.Name == "Fullmetal Alchemist"), 27, true);

                var pages = TestChapter(chapters.Last(), 212);

                TestPage(pages.First(), 
                    "7E9DF42D-ED8C16A2-E67248FF-F57702BB-2ED646B3-0C9C60D3-83506320-328A86D4");
                TestPage(pages.Last(), 
                    "2C9D7E26-477ECEB2-F29C4CFC-7F5979B3-84DF750C-D270B456-B72AC73E-9012517F");

                pages = TestChapter(chapters.First(), 177);

                TestPage(pages.First(), 
                    "3EB9F663-30BFA500-B792DC8E-63CB6950-E20635F7-4A7B0069-4E5E8B18-C36321E5");
                TestPage(pages.Last(), 
                    "4465CA14-0C186340-8C3DF8D0-13409713-C81030F1-344510E6-BBFB2A9C-308AC1F9");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "Air Gear"), 50, true);

                var pages = TestChapter(chapters.Last(), 17);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);

                pages = TestChapter(chapters.First(), 188);

                TestPage(pages.First(), 
                    "CBF5F8D4-7619396E-CED74CA5-5897E090-E302C4E5-355977BB-0E971488-F949D0A9");
                TestPage(pages.Last(), 
                    "1AEAE9E8-496A3A3B-23E9F5A0-68444A5A-E4E6B4D1-8885B265-A7D22E92-B2B00CF1");
            }
        }

        [TestMethod]
        public void StopTazmoTest()
        {
            var series = TestServer(ServerInfo.StopTazmo, 1902);

            {
                var chapters = TestSerie(series.First(s => s.Name == "Bleach"), 438, true);

                var pages = TestChapter(chapters.First(), 57);

                TestPage(pages.First(),
                    "8D78D814-791583E2-19F0FC41-460F600B-982ABBF0-6278B2A9-3D6D5112-ADB86FA6");
                TestPage(pages.Last(),
                    "41838061-F5379AD8-FF615340-5FEF5C07-C9A68FF7-6F643947-7A203C17-5308D8A4");

                pages = TestChapter(chapters.Last(), 0, true);

                TestPage(pages.First(), "", true);
                TestPage(pages.Last(), "", true);
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "666 Satan"), 78);

                var pages = TestChapter(chapters.First(), 25);

                TestPage(pages.First(),
                    "4C1EBC9E-132DC56A-56B47BD6-C567DE7A-9354C577-D2C7E01E-18B7209B-CFDC1D43");
                TestPage(pages.Last(),
                    "C7BBA2D4-579AD7C0-38DE23A8-E7BDC94A-0D1480F3-50D22B2F-F759BF7B-E684F834");

                pages = TestChapter(chapters.Last(), 51);

                TestPage(pages.First(),
                    "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB");
                TestPage(pages.Last(),
                    "0D8475C4-E5D98687-C4DA831B-0D8F7003-6DF7F2EE-3747FC41-1E56B602-AF65CE0A");
            }
        }

        [TestMethod]
        public void UnixMangaTest()
        {
            var series = TestServer(ServerInfo.UnixManga, 1532);

            {
                var chapters = TestSerie(series.First(s => s.Name == "Bleach"), 439);

                var pages = TestChapter(chapters.Last(), 8);

                TestPage(pages.First(), 
                    "262ED4DC-7C5B3D1F-B0918BB3-3DACD75A-60D2119B-17A29A97-04E12601-051312D8");
                TestPage(pages.Last(), 
                    "B8177447-C74F7AA8-0521B396-2B7119F8-E173C00B-27F6B894-C997C530-2566F1E6");

                pages = TestChapter(chapters.First(), 15);

                TestPage(pages.First(), 
                    "99285B2F-50490ECA-D0D8A9E4-818032F2-186D0AC4-B75331FD-3818B301-FA0F2CA2");
                TestPage(pages.Last(), 
                    "52B1C84E-03455C85-1C96BA7F-6F4FCF33-B839109A-E6F699D6-E9400BCB-13CC5B90");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "666 Satan"), 80);

                var pages = TestChapter(chapters.Last(), 25);

                TestPage(pages.First(), 
                    "408A4442-0CDE316E-1E41544D-B8DFB08C-402CDC6E-9128EF2E-6B2E1ECE-8496E9D7");
                TestPage(pages.Last(), 
                    "1595E11D-A302BA42-7A33F0E1-4ADEF4BD-C5365FBF-0B89123E-B61C9E8B-E7E08A2D");

                pages = TestChapter(chapters.First(), 30);

                TestPage(pages.First(), 
                    "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB");
                TestPage(pages.Last(), 
                    "AC9DF68B-C07F380B-DBBCF4EA-4A79C0DB-EF00A7DC-5AF91C46-56E1F179-90EDB30F");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "Kamisama no Tsukurikata"), 17);

                var pages = TestChapter(chapters.Last(), 30);

                TestPage(pages.First(), 
                    "BA438114-F68B9CDD-42198F17-8C72B2D3-78882EC9-C9A2A995-BFE19D26-FDD0A9C5");
                TestPage(pages.Last(), 
                    "A95CE474-FBFCFCEE-F98AB70E-C3B341C3-609F8871-7AD9F33C-A0EF7BFB-1BDF5B8D");

                pages = TestChapter(chapters.First(), 30);

                TestPage(pages.First(), 
                    "49C0AACF-CEFCBFD1-626AF6D5-3E3F444A-252DCBCC-CB109F3A-BEB2FCB7-F2C39DA0");
                TestPage(pages.Last(), 
                    "FFC3EBB9-9BDF5759-DE87C6D6-E2634C38-8982EC38-34CF67C6-2336B18D-28F7862E");
            }

            {
                var chapters = TestSerie(series.First(s => s.Name == "16 Sai Kissu Complete"), -1);

                Assert.IsTrue(chapters.Count() == 1);

                var pages = TestChapter(chapters.First(), 30);

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
            Parallel.ForEach(ServerInfo.ServersInfos, server => 
            {
                try
                {
                    server.DownloadSeries();
                }
                catch
                {
                    TestContext.WriteLine("Exception while downloading series from server {0}", 
                        server);
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
                        TestContext.WriteLine(
                            "Exception while downloading chapters from serie {0}", serie);
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
                            TestContext.WriteLine(
                                "Exception while downloading pages from chapter {0}", chapter);
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
                                TestContext.WriteLine(
                                    "Exception while downloading image from page {0}", page);
                                return;
                            }

                            if (stream.Length == 0)
                            {
                                TestContext.WriteLine(
                                    "Image stream has zero size for page {0}", page);
                                return;
                            }

                            try
                            {
                                System.Drawing.Image.FromStream(stream);
                            }
                            catch
                            {
                                TestContext.WriteLine(
                                    "Exception while creating image from stream for page {0}", page);
                                return;
                            }
                        });
                    });
                });
            });
        }
    }
}
