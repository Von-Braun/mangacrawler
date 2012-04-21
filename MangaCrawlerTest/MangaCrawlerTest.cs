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
using MangaCrawlerLib.Crawlers;
using MangaCrawler;
using TomanuExtensions;

namespace MangaCrawlerTest
{
    [TestClass]
    public class MangaCrawlerTest
    {
        private TestContext m_test_context_instance;

        public TestContext TestContext
        {
            get
            {
                return m_test_context_instance;
            }
            set
            {
                m_test_context_instance = value;
            }
        }

        private bool m_error = false;

        [TestCleanup]
        public void CheckError()
        {
            Assert.IsTrue(m_error == false);
        }

        [TestInitialize]
        public void Setup()
        {
            DownloadManager.Create(
                   new MangaSettings(),
                   Settings.GetSettingsDir());
        }

        private static string FormatHash(string a_hash)
        {
            List<string> ar = new List<string>();
            while (a_hash != "")
            {
                ar.Add(a_hash.Left(2));
                a_hash = a_hash.RemoveFromLeft(2);
            }

            for (int i = 0; i < ar.Count / 4; i++)
            {
                if (i != 0)
                    a_hash += "-";
                a_hash += ar[i * 4] + ar[i * 4 + 1] + ar[i * 4 + 2] + ar[i * 4 + 3];
            }

            return a_hash;
        }

        private IEnumerable<Serie> TestServer(Server a_server, int a_count)
        {
            TestContext.WriteLine("Testing server {0}", a_server.Name);

            a_server.State = ServerState.Waiting;
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

            a_serie.State = SerieState.Waiting;
            a_serie.DownloadChapters();
            var chapters = a_serie.Chapters;

            Crawler.DownloadWithRetry(() => new HtmlWeb().Load(a_serie.URL));

            if (!a_ongoing)
            {
                if (chapters.Count() != a_count)
                {
                    TestContext.WriteLine("  ERROR Chapters, expected {0}, finded: {1}", a_count,
                        chapters.Count());
                    m_error = true;
                }
                else
                {
                    TestContext.WriteLine("  Chapters, expected {0}, finded: {1}", a_count,
                        chapters.Count());
                }
            }
            else
            {
                if (chapters.Count() < a_count)
                {
                    TestContext.WriteLine("  ERROR Chapters (ongoing), expected {0}, finded: {1}",
                        a_count, chapters.Count());
                    m_error = true;
                }
                else
                {
                    TestContext.WriteLine("  Chapters (ongoing), expected {0}, finded: {1}",
                        a_count, chapters.Count());
                }
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
                    TestContext.WriteLine("      ERROR Bad chapter title");
                    TestContext.WriteLine("      Excepcted: >{0}<", a_title);
                    TestContext.WriteLine("      Finded:    >{0}<", a_chapter.Title);

                    m_error = true;
                }
            }

            Crawler.DownloadWithRetry(() => new HtmlWeb().Load(a_chapter.URL));

            a_chapter.State = ChapterState.Waiting;

            Limiter.BeginChapter(a_chapter);

            try
            {
                a_chapter.DownloadPagesList();
            }
            finally
            {
                Limiter.EndChapter(a_chapter);
            }

            var pages = a_chapter.Pages.ToList();

            if (!a_ongoing)
            {
                if (pages.Count() != a_count)
                {
                    TestContext.WriteLine("    ERROR Pages, expected {0}, finded: {1}",
                        a_count, pages.Count());
                    m_error = true;
                }
                else
                {
                    TestContext.WriteLine("    Pages, expected {0}, finded: {1}",
                    a_count, pages.Count());
                }
            }
            else
            {
                TestContext.WriteLine("    Ongoing");
            }

            Assert.IsTrue(a_chapter.Pages.All(s => s.Name.Trim() == s.Name));
            Assert.IsTrue(a_chapter.Pages.All(s => !String.IsNullOrWhiteSpace(s.Name)));

            return pages;
        }

        private void TestPage(Page a_page, string a_hash, string a_name, bool a_ongoing = false)
        {
            Assert.IsTrue(a_hash != null);

            if (a_ongoing)
                Assert.IsTrue(a_name == "");
            else
            {
                Assert.IsTrue(a_name != null);

                if (a_page.Name != a_name)
                {
                    TestContext.WriteLine("          ERROR Bad page name");
                    TestContext.WriteLine("          Excepcted: >{0}<", a_name);
                    TestContext.WriteLine("          Finded: >{0}<", a_page.Name);

                    m_error = true;
                }
            }

            if (a_ongoing)
                Assert.IsTrue(a_hash == "");
            else
                Assert.IsTrue(a_hash != null);

            TestContext.WriteLine("        Testing page {0}", a_page.Name);

            Limiter.BeginChapter(a_page.Chapter);
            try
            {
                var stream = a_page.GetImageStream();

                Assert.IsTrue(stream.Length > 0);

                System.Drawing.Image.FromStream(stream);
                stream.Position = 0;

                if (!a_ongoing)
                {
                    string hash = Hash.CalculateSHA256(stream);
                    hash = FormatHash(hash);

                    if (a_hash != hash)
                    {
                        TestContext.WriteLine("        ERROR Hash doestn't match");
                        TestContext.WriteLine("        Excepcted: >{0}<", a_hash);
                        TestContext.WriteLine("        Finded: >{0}<", hash);
                        m_error = true;
                    }
                }
            }
            finally
            {
                Limiter.EndChapter(a_page.Chapter);
            }
        }

        [TestMethod]
        public void _RandomTestAll()
        {
            Parallel.ForEach(DownloadManager.Instance.Servers,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = DownloadManager.Instance.Servers.Count(),
                    TaskScheduler = Limiter.Scheduler
                },
                server =>
                {
                    try
                    {
                        server.State = ServerState.Waiting;
                        server.DownloadSeries();
                    }
                    catch
                    {
                        TestContext.WriteLine("{0} - Exception while downloading series from server",
                            server);
                    }

                    if (server.Series.Count == 0)
                    {
                        TestContext.WriteLine("{0} - Server have no series", server);
                    }

                    Parallel.ForEach(TakeRandom(server.Series, 0.1),
                        new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer,
                            TaskScheduler = Limiter.Scheduler
                        },
                        serie =>
                        {
                            try
                            {
                                serie.State = SerieState.Waiting;
                                serie.DownloadChapters();
                            }
                            catch
                            {
                                TestContext.WriteLine(
                                    "{0} - Exception while downloading chapters from serie", serie);
                            }

                            if (serie.Chapters.Count == 0)
                            {
                                TestContext.WriteLine("{0} - Serie has no chapters", serie);
                            }

                            Parallel.ForEach(TakeRandom(serie.Chapters, 0.1),
                                new ParallelOptions()
                                {
                                    MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer,
                                    TaskScheduler = Limiter.Scheduler
                                },
                                (chapter) =>
                                {
                                    try
                                    {
                                        chapter.State = ChapterState.Waiting;

                                        Limiter.BeginChapter(chapter);

                                        try
                                        {
                                            chapter.DownloadPagesList();
                                        }
                                        finally
                                        {
                                            Limiter.EndChapter(chapter);
                                        }
                                    }
                                    catch
                                    {
                                        TestContext.WriteLine(
                                            "{0} - Exception while downloading pages from chapter", chapter);
                                    }

                                    if (chapter.Pages.Count == 0)
                                    {
                                        TestContext.WriteLine("{0} - Chapter have no pages", chapter);
                                    }

                                    Parallel.ForEach(TakeRandom(chapter.Pages, 0.1),
                                        new ParallelOptions()
                                        {
                                            MaxDegreeOfParallelism = chapter.Crawler.MaxConnectionsPerServer,
                                            TaskScheduler = Limiter.Scheduler
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
        public void UnixMangaTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(UnixMangaCrawler)), 1613);

            {
                var chapters = TestSerie(series.First(s => s.Title == "Bleach"), 505, true);

                var pages = TestChapter("Bleach c Calendar", chapters.Last(), 8);

                TestPage(pages.First(),
                    "E018B7DF-C64538D1-95A9B6C9-49A8DBE9-18C11E68-C52EDDFB-B800B51A-FA7E354F", "00");
                TestPage(pages.Last(),
                    "B8177447-C74F7AA8-0521B396-2B7119F8-E173C00B-27F6B894-C997C530-2566F1E6", "xxxhomeunixxxx");

                pages = TestChapter("", chapters.Skip(2).First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "666 Satan"), 80);

                var pages = TestChapter("666Satan-v01-c01a[TW]", chapters.Last(), 25);

                TestPage(pages.First(),
                    "5E06A425-528A10D1-A3B02CCC-69F21166-5EC8C51D-F4F6601E-59247E93-A46DB54A", "666Satan-01-00");
                TestPage(pages.Last(),
                    "6DABC33C-5106B55F-CD3E4C3F-6C4088D6-0BE589FC-FBA4764D-41B27F65-AC88904D", "666Satan-01-24");

                pages = TestChapter("666Satan-76-END-[FH]", chapters.First(), 30);

                TestPage(pages.First(),
                    "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB", "01");
                TestPage(pages.Last(),
                    "AC9DF68B-C07F380B-DBBCF4EA-4A79C0DB-EF00A7DC-5AF91C46-56E1F179-90EDB30F", "31");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Kamisama no Tsukurikata"), 17);

                var pages = TestChapter("kamisama 001", chapters.Last(), 30);

                TestPage(pages.First(),
                    "4EB61381-57D3E2E2-3BCA169A-513A154A-293D39BD-56CF793F-ADD02D7E-A8BDACBC", "0001");
                TestPage(pages.Last(),
                    "A95CE474-FBFCFCEE-F98AB70E-C3B341C3-609F8871-7AD9F33C-A0EF7BFB-1BDF5B8D", "0030");

                pages = TestChapter("kamisama 017", chapters.First(), 30);

                TestPage(pages.First(),
                    "E1D1A575-88D45C6A-8BFEBF16-349CA8BC-FB90D4A4-1AC4F831-EAF1385F-7F55F315", "0001");
                TestPage(pages.Last(),
                    "FFC3EBB9-9BDF5759-DE87C6D6-E2634C38-8982EC38-34CF67C6-2336B18D-28F7862E", "0030");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "16 Sai Kissu Complete"), 1);

                var pages = TestChapter("16 Sai Kissu Complete", chapters.First(), 30);

                TestPage(pages.First(),
                    "83AE7BA3-A1554FA6-0FB46772-FD94DD15-057EAF75-5AF15ED5-A41C1DEF-94FE6F56", "00 _ Nekohana");
                TestPage(pages.Last(),
                    "3048567C-DF4072AC-01C71054-734C7736-BB6CD48E-92E1EA56-BB5DD04A-4C1048EC", "16 Sai Kissu c01 030");
            }
        }

        [TestMethod]
        public void AnimeSourceTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(AnimeSourceCrawler)), 53);

            {
                var chapters = TestSerie(series.First(
                    s => s.Title == "Kimagure Orange Road"), 167);

                var pages = TestChapter("KOR (Volume 18 Chapter 156 part 4 original ending)", chapters.First(), 4);

                TestPage(pages.First(),
                    "97895BDE-0DE30690-79305713-6CCF2D22-A5B83C1F-586BA24C-AC7DC3F7-8826E214", "1");
                TestPage(pages.Last(),
                    "25476017-BDD69A52-FAEEC980-8B6D2BB0-F36BD832-8AFA63B9-335FCB0A-C4C6AE50", "4");

                pages = TestChapter("KOR (Volume 1 Chapter 1)", chapters.Last(), 34);

                TestPage(pages.First(),
                    "D3DBFB1A-2BB55A85-FECE3DF7-766C73D1-F0A8C43C-282046C1-9E181C0A-C709F948", "1");
                TestPage(pages.Last(),
                    "A09BB75E-43DBE864-1DA21C19-6D204131-81C9BDF6-790B8549-9825A636-35F9A95B", "34");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Freezing"), 33, true);

                var pages = TestChapter("Volume 06 - Chapter 33", 
                    chapters.First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);

                pages = TestChapter("Volume 01 - Chapter 01", chapters.Last(), 40);

                TestPage(pages.First(),
                    "F5C2644E-C4F18D93-D1D6550F-ACFD6A90-B08606FD-C82BC561-5C6EE22E-D15A7CD8", "1");
                TestPage(pages.Last(),
                    "07E027F6-FC08FFF0-B49F310E-6A03A5BE-C4F58938-A48DD527-DA2BED7F-22616F0E", "40");
            }
        }

        [TestMethod]
        public void MangaAccessTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(MangaAccessCrawler)), 4240);

            {
                var chapters = TestSerie(series.First(s => s.Title == "090 ~Eko to Issho~"), 59);

                var pages = TestChapter("090 ~Eko to Issho~ chapter 59", chapters.First(), 12);

                TestPage(pages.First(),
                    "C5F62EA7-4EC5CAFB-49592B43-A24C628C-3C45D653-A5040473-C0806A51-6F8ADA85", "1");
                TestPage(pages.Last(),
                    "0BF825A9-B161DFB4-768B4F36-BE24CD17-0CE9B886-4039ED32-441E09DA-00F6A65E", "12");

                pages = TestChapter("090 ~Eko to Issho~ chapter 1", chapters.Last(), 25);

                TestPage(pages.First(),
                    "ACDB7B80-C49DF779-698D8AB3-52BFB11A-5FF1A9A9-3A12CC99-0F77943E-4F9C9E19", "1");
                TestPage(pages.Last(),
                    "AD4A2B5C-FB398235-9860705E-B1CBB811-7E3990C7-E9F9D7A5-31ADF68A-1A10B0D4", "25");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Fairy Tail"), 277, true);

                var pages = TestChapter("Fairy Tail chapter 271", chapters.First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);

                pages = TestChapter("Fairy Tail chapter 1", chapters.Last(), 73);

                TestPage(pages.First(),
                    "2485C92C-3EDCBE95-84C5A4A2-6099F511-6926FF1A-16A4E759-E7342F1E-5842833D", "1");
                TestPage(pages.Last(),
                    "BB71EA96-80D001F8-B7244F62-32C002BF-4F911F96-DF6DBEBE-860025D9-D88DC9EE", "73");
            }
        }

        [TestMethod]
        public void MangaFoxTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(MangaFoxCrawler)), 9167);
            
            {
                var chapters = TestSerie(series.First(s => s.Title == "10, 20, and 30"), 79);

                var pages = TestChapter("10, 20, and 30 1", chapters.Last(), 17);
            
                TestPage(pages.First(),
                    "D9FFCBE8-1072AD51-1E0AA6F6-BE5CCFDD-7CF584AD-E8221713-C49AA2F4-AEA62DC0", "1");
                TestPage(pages.Last(),
                    "1CB42BA2-0B8F29DF-98435B81-9C072CDA-BAF7AE44-486B60E8-92265042-929FE0CE", "17");

                pages = TestChapter("10, 20, and 30 78", chapters.First(), 14);
            
                TestPage(pages.First(),
                    "2BCC333B-6850F08E-CFF72D4C-E8FDACB8-E79D2D3A-FDCE387D-24CC936B-1660CAD6", "1");
                TestPage(pages.Last(),
                    "95383B54-35E02D1D-DF94CA10-8455F7F0-63789ABC-5C3282E2-67BEB278-2BFEC769", "14");
            }
            
            {
                var chapters = TestSerie(series.First(s => s.Title == "1/2 Prince"), 9, true);

                var pages = TestChapter("1/2 Prince 1", chapters.Last(), 73);
            
                TestPage(pages.First(),
                    "8209C173-CED82809-1B4DBB32-BE7487AE-CEA8F7C3-1FA4A608-BDBFA73B-1DF3FA00", "1");
                TestPage(pages.Last(),
                    "924EA537-2BFAE86B-EFE6B628-1B3B3236-4C14D399-54517F07-C121DD08-E7365305", "73");
            
                pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);
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
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(MangaRunCrawler)), 323);
            
            {
                var chapters = TestSerie(series.First(s => s.Title == "666satan"), 78);

                var pages = TestChapter("666satan 076", chapters.First(), 51);
            
                TestPage(pages.First(),
                    "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB", "01");
                TestPage(pages.Last(),
                    "0D8475C4-E5D98687-C4DA831B-0D8F7003-6DF7F2EE-3747FC41-1E56B602-AF65CE0A", "Credits");

                pages = TestChapter("666satan 001a", chapters.Last(), 25);
            
                TestPage(pages.First(),
                    "4C1EBC9E-132DC56A-56B47BD6-C567DE7A-9354C577-D2C7E01E-18B7209B-CFDC1D43", "666Satan-01-00");
                TestPage(pages.Last(),
                    "C7BBA2D4-579AD7C0-38DE23A8-E7BDC94A-0D1480F3-50D22B2F-F759BF7B-E684F834", "666Satan-01-24");
            }
            
            {
                var chapters = TestSerie(series.First(s => s.Title == "bleach"), 450, true);
            
                var pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);

                pages = TestChapter("bleach 001", chapters.Last(), 57);
            
                TestPage(pages.First(),
                    "8D78D814-791583E2-19F0FC41-460F600B-982ABBF0-6278B2A9-3D6D5112-ADB86FA6", "Bleach-01-01-00");
                TestPage(pages.Last(),
                    "E9B3C85A-C85A9F3B-FB3653FD-599AB0A8-D2B58283-DD48A599-AE5CB86F-2DFDA740", "bleach flap 01");
            }
        }

        [TestMethod]
        public void MangaShareTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(MangaShareCrawler)), 187);

            {
                var chapters = TestSerie(series.First(s => s.Title == "Akumetsu"), 73);

                var pages = TestChapter("001 - Super Elite", chapters.Last(), 50);
            
                TestPage(pages.First(),
                    "E99D18EA-8B4704C3-707E8CF1-C7EB5D28-782DA8AA-3EA987A5-D5063C14-BB079B2A", "1");
                TestPage(pages.Last(),
                    "29A6D2AD-58F2DDB4-70C8118A-C29198AC-0E32A035-D953D134-E7911F08-5EF8E293", "50");

                pages = TestChapter("073 - Gutter Scum", chapters.First(), 21);
            
                TestPage(pages.First(),
                    "05BA460B-6DBB06B4-3810F860-3A7113A0-3AC7AF80-5021D7E3-6F8D77E8-3A39027A", "1");
                TestPage(pages.Last(),
                    "67C8BF5C-833904D9-1C6AAD79-118BB4BE-DAC43AC7-E4F5B24A-3FD5EF2A-9B2FB526", "21");
            }
            
            {
                var chapters = TestSerie(series.First(s => s.Title == "Fairy Tail"), 263, true);

                var pages = TestChapter("001 - Fairy Tail", chapters.Reverse().Skip(1).First(), 74);
            
                TestPage(pages.First(),
                    "3BBFA539-2392866D-E726FA42-BF766868-893F9BEC-497A9054-2B142CB9-F7D06026", "1");
                TestPage(pages.Last(),
                    "E6EF1C9D-93352A75-5B8BB852-928773E1-81F7828C-36354007-040CF780-33A252CF", "74");
            
                pages = TestChapter("", chapters.Skip(1).First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);
            }
        }

        [TestMethod]
        public void MangaVolumeTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(MangaVolumeCrawler)), 1141);

            {
                var chapters = TestSerie(series.First(s => s.Title == "666 Satan"), 76);

                var pages = TestChapter("666 Satan 1", chapters.Last(), 80);

                TestPage(pages.First(),
                    "03D423DB-2EA355AB-CD152179-D58C3BE0-0F8ED502-34989FCC-D32BE720-BD687C64", "1");
                TestPage(pages.Last(),
                    "E17484D0-F2D05001-1CD97224-B306B528-A9B2FC87-4D2712BB-46DB2C7B-F5347844", "80");

                pages = TestChapter("666 Satan 76", chapters.First(), 51);

                TestPage(pages.First(), "78FCFE4A-6C6E827A-411E5398-58FDCA1D-3C448073-314051D7-CFE2AD14-B0F114FC", "1");
                TestPage(pages.Last(), "45FFDA0C-43E220F7-6BCC3705-D775E37B-F48B3876-548FDD38-B7D604EA-A2B9A9C6", "51");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Bleach"), 0);
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Freezing"), 56, true);

                var pages = TestChapter("Freezing 1", chapters.Last(), 40);

                TestPage(pages.First(),
                    "E407F903-98205296-23019348-AB0235B6-F414FD7A-63D80052-C29695CD-B7305CD6", "1");
                TestPage(pages.Last(),
                    "1BA1BF69-1BA4DA38-171034AA-B3A22C99-1F300185-1A71DF8E-379F4B12-97C2AA6B", "40");

                pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);
            }
        }

        [TestMethod]
        public void OtakuWorksTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(OtakuWorksCrawler)), 6712);

            {
                var chapters = TestSerie(series.First(s => s.Title == "Ai Kora"), 92);

                var pages = TestChapter("Volume #12, Chapter #119", chapters.First(), 23);

                TestPage(pages.First(),
                    "BD1DFD22-37D56B17-28B6A51C-DC0D06FC-2EE0A9A0-A92DA3CE-B38985CD-49A513E4", "1");
                TestPage(pages.Last(),
                    "B0FA3092-6CF3D2F7-359A41A5-60448E7F-3B0CD405-FDF2DB5B-7EEFB73D-0440B6C4", "23");

                pages = TestChapter("Volume #01", chapters.Last(), 221);

                TestPage(pages.First(),
                    "89A7B818-BF924FD6-BD57A0C2-5B53D89F-C92259F4-63833C51-C4B56EDE-01B659D8", "1");
                TestPage(pages.Last(),
                    "E759AE74-7AFE7B06-D81DC60D-85060402-39DAAF84-095896AC-F06B5795-D93A36DC", "221");
            }

            {
                var chapters = TestSerie(series.First(
                    s => s.Title == ".hack//G.U. The World"), 0);
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Bleach"), 17, true);

                var pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);

                pages = TestChapter("Chapter #470", chapters.Last(), 23);

                TestPage(pages.First(),
                    "89DC0389-667203B1-3923788E-C758931A-8D876123-468CDCE2-B2D5EA25-D4B9D530", "1");
                TestPage(pages.Last(),
                    "91FA4FB9-CFC01346-70C37F18-074AA4F8-CF9B41AC-55AFE15B-F0642B7D-56B75909", "23");
            }
        }

        [TestMethod]
        public void OurMangaTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(OurMangaCrawler)), 2993);

            {
                var chapters = TestSerie(series.First(s => s.Title == "090 - Eko To Issho"), 61);

                var pages = TestChapter("Chapter 1", chapters.Last(), 25);

                TestPage(pages.First(),
                    "ACDB7B80-C49DF779-698D8AB3-52BFB11A-5FF1A9A9-3A12CC99-0F77943E-4F9C9E19", "eko01_000a");
                TestPage(pages.Last(),
                    "AD4A2B5C-FB398235-9860705E-B1CBB811-7E3990C7-E9F9D7A5-31ADF68A-1A10B0D4", "eko01_022");

                pages = TestChapter("Chapter 60.5", chapters.First(), 14);

                TestPage(pages.First(),
                    "3777DCA7-8CC73AA7-03B353A8-88F115D6-F261571F-2EEDE9F0-48736079-62EEB307", "01");
                TestPage(pages.Last(),
                    "73A83F61-D44253FB-F74590F4-D76D9641-969AA440-E28F10C9-17101ACB-AF7357E7", "joinus");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Fairy Tail"), 275, true);

                var pages = TestChapter("Chapter 1", chapters.Last(), 74);

                TestPage(pages.First(),
                    "73F71C80-4E862F33-94640182-850AC5C7-FD0602A6-4F1BD1CB-86AAA785-DF8E03F1", "01");
                TestPage(pages.Last(),
                    "7CB8D9E4-334CBC77-6091EE52-D2BA5824-5D4CDC0E-594E1EC8-BFEEA60F-04859424", "credits");

                pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);
            }
        }

        [TestMethod]
        public void SpectrumNexusTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(SpectrumNexusCrawler)), 121);

            {
                var chapters = TestSerie(series.First(
                    s => s.Title == "Fullmetal Alchemist"), 27);

                var pages = TestChapter("Volume 27", chapters.First(), 212);

                TestPage(pages.First(),
                    "18812B42-DD6F41A6-88F6133C-FF978311-7DDF6D98-0A9E1017-C741E91B-678EBE39", "1");
                TestPage(pages.Last(),
                    "E878290F-6BB23E8D-E0E80036-511E584D-2C73FA3D-8BB78286-DEC5D4D7-70885E20", "212");

                pages = TestChapter("Volume 01", chapters.Last(), 177);

                TestPage(pages.First(),
                    "74DC3FF3-B47172FD-824F95E0-7E62196E-BFE20CFB-3F21EDC2-4D77FEDB-A21C7035", "1");
                TestPage(pages.Last(),
                    "F78693E3-835389EE-C4575D98-516DA069-5226F579-6DE02CD3-7EB350DE-17534749", "177");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Bleach"), 13, true);

                var pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);

                pages = TestChapter("Chapter 470", chapters.Last(), 23);

                TestPage(pages.First(),
                    "40CE9E35-301F9E63-B0FB16B4-264CCCD2-7510FC7A-8C043C04-1EF8A8F3-1DEC06ED", "1");
                TestPage(pages.Last(),
                    "B78E367D-C2CF75B6-99550DB7-CA263562-86D0A3F3-8F97B1A6-D43ABCB7-0451E857", "23");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Battle Angel Alita"), 10);
            }
        }

        [TestMethod]
        public void StopTazmoTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(StopTazmoCrawler)), 2785);

            {
                var chapters = TestSerie(series.First(s => s.Title == "666 Satan"), 78);

                var pages = TestChapter("666satan_076", chapters.First(), 51);

                TestPage(pages.First(),
                    "6DC6CCF8-BB831044-DEDBEB18-D83FB748-C10D7698-FDDB65B8-506D7A06-2F455AAB", "01");
                TestPage(pages.Last(),
                    "0D8475C4-E5D98687-C4DA831B-0D8F7003-6DF7F2EE-3747FC41-1E56B602-AF65CE0A", "Credits");

                pages = TestChapter("666satan_001a", chapters.Last(), 25);

                TestPage(pages.First(),
                    "4C1EBC9E-132DC56A-56B47BD6-C567DE7A-9354C577-D2C7E01E-18B7209B-CFDC1D43", "666Satan-01-00");
                TestPage(pages.Last(),
                    "C7BBA2D4-579AD7C0-38DE23A8-E7BDC94A-0D1480F3-50D22B2F-F759BF7B-E684F834", "666Satan-01-24");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Bleach"), 475, true);

                var pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);

                pages = TestChapter("bleach_001", chapters.Last(), 57);

                TestPage(pages.First(),
                    "8D78D814-791583E2-19F0FC41-460F600B-982ABBF0-6278B2A9-3D6D5112-ADB86FA6", "Bleach-01-01-00");
                TestPage(pages.Last(),
                    "E9B3C85A-C85A9F3B-FB3653FD-599AB0A8-D2B58283-DD48A599-AE5CB86F-2DFDA740", "bleach_flap_01");
            }
        }

        [TestMethod]
        public void MangaHereTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(MangaHere)), 8356);

            {
                var chapters = TestSerie(series.First(s => s.Title == "666 Satan"), 76);

                var pages = TestChapter("666 Satan 1", chapters.Last(), 80);

                TestPage(pages.First(),
                    "F32A16E3-40787969-1321CF99-60E56266-5C193674-8567D887-9D07160E-618BB267", "1");
                TestPage(pages.Last(),
                    "80ED3880-3AEA69C9-8F6ED876-14EF1B8D-B47C7779-8842C9F5-C1A8721A-9B962CFC", "80");

                pages = TestChapter("666 Satan 76", chapters.First(), 51);

                TestPage(pages.First(),
                    "E5104CE4-20487A8B-F661495F-F78C3650-86A123BD-112C766D-FBC1089A-ADDEC52C", "1");
                TestPage(pages.Last(),
                    "E4B30831-9ED853CD-C2E4A0D5-1AEFE087-427D3C55-CC6FAEB2-98DD20BE-802438F4", "51");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Fairy Tail"), 287, true);

                var pages = TestChapter("Fairy Tail 1", chapters.Last(), 74);

                TestPage(pages.First(),
                    "F78B1798-A2FC4CCD-A6773D62-D9803D04-D2B54352-AD25D299-174F97C3-D956F41E", "1");
                TestPage(pages.Last(),
                    "7CC1FA04-490266B7-BC68BC1A-BAD34324-D8CA551F-D3204F46-C4DC052A-D0EDEB8C", "74");

                pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);
            }
        }

        [TestMethod]
        public void MangaReaderTest()
        {
            var series = TestServer(DownloadManager.Instance.Servers.First(
                s => s.Crawler.GetType() == typeof(MangaReader)), 2714);

            {
                var chapters = TestSerie(series.First(s => s.Title == "37 Degrees Kiss"), 5);

                var pages = TestChapter("37 Degrees Kiss 1", chapters.Last(), 50);

                TestPage(pages.First(),
                    "15717C24-557CF677-1571FAA8-37C38700-CCC1C9FA-1772E7BB-F51D2A1B-B865D2CE", "1");
                TestPage(pages.Last(),
                    "FCEF02CC-2890D38A-A036FE5D-8FE39F3F-1301A272-3E14B6A0-0456AC60-237FEAFD", "50");

                pages = TestChapter("37 Degrees Kiss 5", chapters.First(), 14);

                TestPage(pages.First(),
                    "37A7B694-EA71C1B1-7B610A55-A947F2CB-499FF22F-55FE3588-802EDE46-78EAF86B", "1");
                TestPage(pages.Last(),
                    "BDBCD9BD-D3D8F9FA-D3E86FCC-D7CFC4BB-A802B0E6-B835E1C0-2C9930B3-03A538B1", "14");
            }

            {
                var chapters = TestSerie(series.First(s => s.Title == "Fairy Tail"), 279, true);

                var pages = TestChapter("Fairy Tail 1", chapters.Last(), 80);

                TestPage(pages.First(),
                    "D5C09018-E4705F6C-54790423-037DBECF-5829E10B-49A0A64D-DC14E43C-8B2201E2", "1");
                TestPage(pages.Last(),
                    "60D44E6A-862FA83A-2A81E448-C47CCC95-91FD7BB8-55365D75-518C4C1A-C69ECDE9", "80");

                pages = TestChapter("", chapters.First(), 0, true);

                TestPage(pages.First(), "", "", true);
                TestPage(pages.Last(), "", "", true);
            }
        }
    }
}
