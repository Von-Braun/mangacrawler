using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaCrawler;
using MangaCrawlerLib;
using MangaCrawlerLib.Crawlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TomanuExtensions;

namespace MangaCrawlerTest
{
    [TestClass]
    public class TestXmls
    {
        private static string ERROR_SUFFIX = " - error";

        [TestInitialize]
        public void Setup()
        {
            DownloadManager.Create(
                   new MangaSettings(),
                   Settings.GetSettingsDir());
        }

        public static string GetTestDataDir()
        {
            string dir = new DirectoryInfo(
                System.Reflection.Assembly.GetAssembly(typeof(Crawler)).Location).Parent.Parent.Parent.Parent.FullName +
                Path.DirectorySeparatorChar + "MangaCrawlerTest" + Path.DirectorySeparatorChar + "TestData";

            if (!Directory.Exists(dir))
            {
                dir = new DirectoryInfo(
                    System.Reflection.Assembly.GetAssembly(typeof(Crawler)).Location).Parent.Parent.Parent.Parent.Parent.FullName +
                    Path.DirectorySeparatorChar + "MangaCrawlerTest" + Path.DirectorySeparatorChar + "TestData";
            }

            return dir;
        }

        private void DeleteErrors(string a_server_name)
        {
            foreach (var file in Directory.GetFiles(GetTestDataDir(), "*" + a_server_name + "*"))
            {
                if (!Path.GetFileNameWithoutExtension(file).EndsWith(ERROR_SUFFIX))
                    continue;
                if (!Path.GetFileName(file).StartsWith(a_server_name))
                    continue;
                File.Delete(file);
            }
        }

        private bool Compare(ServerTestData a_from_xml, ServerTestData a_downloaded)
        {
            try
            {
                if (!a_from_xml.Compare(a_downloaded))
                {
                    GenerateInfo(a_downloaded);
                    return false;
                }

                return true;
            }
            catch
            {
                GenerateInfo(a_downloaded);
                return false;
            }
        }

        private void GenerateInfo(ServerTestData a_server_test_data, bool a_downloaded = true)
        {
            string a_suffix = a_downloaded ? ERROR_SUFFIX : "";

            a_server_test_data.Save(Path.Combine(GetTestDataDir(), a_server_test_data.Name + a_suffix + ".xml"));

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

        private void CheckOngoing(ServerTestData a_downloaded)
        {
            Assert.IsTrue((from serie in a_downloaded.Series
                           where serie.Chapters.Count(ch => ch.Index == ch.SerieTestData.ChapterCount - 1) >= 1
                           where serie.Chapters.Count(ch => ch.Index == 0) >= 1
                           select serie).Count() >= 2);
        }

        private void TestXml(string a_server_name)
        {
            DeleteErrors(a_server_name);
            var from_xml = ServerTestData.Load(Path.Combine(GetTestDataDir(), a_server_name + ".xml"));
            var downloaded = ServerTestData.Load(Path.Combine(GetTestDataDir(), a_server_name + ".xml"));
            downloaded.Download();
            Assert.IsTrue(Compare(from_xml, downloaded));
            CheckOngoing(downloaded);
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
    }
}
