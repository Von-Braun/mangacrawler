using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaCrawlerLib;
using MangaCrawlerLib.Crawlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TomanuExtensions;

namespace MangaCrawlerTest
{
    [TestClass]
    public class TestXmls : TestBase
    {
        private static string ERROR_SUFFIX = " - error";

        private void DeleteErrors(string a_server_name)
        {
            foreach (var file in Directory.GetFiles(TestBase.GetTestDataDir(), "*" + a_server_name + "*"))
            {
                if (!Path.GetFileNameWithoutExtension(file).EndsWith(ERROR_SUFFIX))
                    continue;
                if (!Path.GetFileName(file).StartsWith("_" + a_server_name))
                    continue;
                File.Delete(file);
            }
        }

        private bool Compare(ServerTestData a_from_xml, ServerTestData a_downloaded)
        {
            if (!a_from_xml.Compare(a_downloaded))
            {
                GenerateInfo(a_downloaded);
                return false;
            }

            return true;
        }

        public static void GenerateInfo(ServerTestData a_server_test_data, bool a_downloaded = true)
        {
            string a_suffix = a_downloaded ? ERROR_SUFFIX : "";

            a_server_test_data.Save(TestBase.GetTestFilePath("_" + a_server_test_data.Name + a_suffix + ".xml"));

            foreach (var page in from serie in a_server_test_data.Series
                                 from chapter in serie.Chapters
                                 from page in chapter.Pages
                                 select page)
            {
                var image_name = Path.Combine(
                    Path.GetDirectoryName(page.FileName),
                    Path.GetFileNameWithoutExtension(page.FileName) + a_suffix + Path.GetExtension(page.FileName));
                page.Image.Save(image_name);
            }
        }

        private void Check(ServerTestData a_server_test_data)
        {
            foreach (var serie_test_data in a_server_test_data.Series)
            {
                foreach (var chapter_test_data in serie_test_data.Chapters)
                {
                    foreach (var page_test_data in chapter_test_data.Pages)
                    {
                        Assert.IsTrue(File.Exists(page_test_data.FileName));
                    }
                }
            };
        }

        private void TestXml(string a_server_name)
        {
            DeleteErrors(a_server_name);
            var from_xml = ServerTestData.Load(TestBase.GetTestFilePath("_" + a_server_name + ".xml"));
            var downloaded = ServerTestData.Load(GetTestFilePath("_" + a_server_name + ".xml"));
            try
            {
                downloaded.Download();

                Assert.IsTrue(Compare(from_xml, downloaded));
                Check(from_xml);
                Check(downloaded);
            }
            catch
            {
                GenerateInfo(downloaded);
                throw;
            }
        }

        [TestMethod]
        public void TestAnimea()
        {
            var server_name = DownloadManager.Instance.Servers.First(
                el => el.Crawler is MangaCrawlerLib.Crawlers.AnimeaCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void TestAnimeSource()
        {
            var server_name = DownloadManager.Instance.Servers.First(
                el => el.Crawler is MangaCrawlerLib.Crawlers.AnimeSourceCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void TestMangaFox()
        {
            var server_name = DownloadManager.Instance.Servers.First(
              el => el.Crawler is MangaCrawlerLib.Crawlers.MangaFoxCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void TestMangaHere()
        {
            var server_name = DownloadManager.Instance.Servers.First(
              el => el.Crawler is MangaCrawlerLib.Crawlers.MangaHereCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void TestMangaReader()
        {
            var server_name = DownloadManager.Instance.Servers.First(
              el => el.Crawler is MangaCrawlerLib.Crawlers.MangaReaderCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void TestMangaShare()
        {
            var server_name = DownloadManager.Instance.Servers.First(
              el => el.Crawler is MangaCrawlerLib.Crawlers.MangaShareCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void TestMangaStream()
        {
            var server_name = DownloadManager.Instance.Servers.First(
              el => el.Crawler is MangaCrawlerLib.Crawlers.MangaStreamCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void TestMangaVolume()
        {
            var server_name = DownloadManager.Instance.Servers.First(
              el => el.Crawler is MangaCrawlerLib.Crawlers.MangaVolumeCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void TestSpectrumNexus()
        {
            var server_name = DownloadManager.Instance.Servers.First(
              el => el.Crawler is MangaCrawlerLib.Crawlers.SpectrumNexusCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void TestStarkana()
        {
            var server_name = DownloadManager.Instance.Servers.First(
              el => el.Crawler is MangaCrawlerLib.Crawlers.StarkanaCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void TestUnixManga()
        {
            var server_name = DownloadManager.Instance.Servers.First(
              el => el.Crawler is MangaCrawlerLib.Crawlers.UnixMangaCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void TestKissManga()
        {
            var server_name = DownloadManager.Instance.Servers.First(
              el => el.Crawler is MangaCrawlerLib.Crawlers.KissMangaCrawler).Name;
            TestXml(server_name);
        }

        [TestMethod]
        public void _DeleteUnusedImages()
        {
            var xmls = Directory.GetFiles(TestBase.GetTestDataDir(), "*.xml");

            List<string> all_used_pages = new List<string>();

            var all_images = (from f in Directory.GetFiles(TestBase.GetTestDataDir())
                              let ext = Path.GetExtension(f).RemoveFromLeft(1).ToLower()
                              where new string[] { "bmp", "jpg", "gif", "png" }.Contains(ext)
                              select f).ToList();

            foreach (var xml in xmls)
            {
                var std = ServerTestData.Load(xml);

                DeleteErrors(Path.GetFileNameWithoutExtension(xml));

                var pages = from serie in std.Series
                            from chapter in serie.Chapters
                            from page in chapter.Pages
                            select page;

                foreach (var page in pages)
                    all_used_pages.Add(page.FileName);
            }

            var unused_images = all_images.Except(all_used_pages);

            foreach (var ui in unused_images)
            {
                WriteLine("Deleting: {0}", ui);
                File.Delete(ui);
            }

            Assert.IsTrue(!unused_images.Any());
        }

        [TestMethod]
        public void _RegeneratedXmlsAndImages()
        {
            DeleteErrors("");

            var xmls = Directory.GetFiles(TestBase.GetTestDataDir(), "*.xml");

            foreach (var xml in xmls)
            {
                WriteLine(xml);

                if (!xml.Contains("Nexus"))
                    continue;

                var std = ServerTestData.Load(xml);

                std.Series = std.Series.OrderBy(s => s.Title).ToList();
                foreach (var serie in std.Series)
                {
                    serie.Chapters = serie.Chapters.OrderBy(ch => ch.Index).ToList();

                    foreach (var chapter in serie.Chapters)
                        chapter.Pages = chapter.Pages.OrderBy(p => p.Index).ToList();
                }

                try
                {
                    std.Download();
                }
                catch
                {
                    GenerateInfo(std);
                    throw;
                }

                GenerateInfo(std, false);
            }
        }
    }
}
