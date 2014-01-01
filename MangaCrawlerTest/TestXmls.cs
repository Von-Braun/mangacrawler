using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaCrawlerLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TomanuExtensions;

namespace MangaCrawlerTest
{
    [TestClass]
    public class TestXmls
    {
        private static string ERROR_SUFFIX = " - downloaded.xml";

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

        private List<ServerTestData> LoadTestDataList()
        {
            return (from test_data_xml in Directory.EnumerateFiles(GetTestDataDir(), "*.xml")
                    select ServerTestData.Load(test_data_xml)).ToList();
        }

        private void Download(List<ServerTestData> a_list)
        {
            Parallel.ForEach(a_list, std => std.Download());
        }

        private void DeleteTestData(string a_server_name)
        {
            foreach (var file in from f in Directory.GetFiles(GetTestDataDir())
                                 let fn = Path.GetFileName(f)
                                 where fn.StartsWith(a_server_name)
                                 select f)
            {
                File.Delete(file);
            }
        }

        private void DeleteErrors()
        {
            foreach (var file in Directory.GetFiles(GetTestDataDir(), ERROR_SUFFIX))
                File.Delete(file);
        }

        private bool Compare(List<ServerTestData> a_from_xml, List<ServerTestData> a_downloaded)
        {
            Assert.AreEqual(a_from_xml.Count, a_downloaded.Count);

            bool result = true;

            for (int i = 0; i < a_from_xml.Count; i++)
                    result &= Compare(a_from_xml[i], a_downloaded[i]);

            return result;
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

        private void CheckOngoing(List<ServerTestData> a_downloaded)
        {
            Assert.IsTrue((from std in a_downloaded
                           select (from serie in std.Series
                                   where serie.Chapters.Count(ch => ch.Index == ch.SerieTestData.ChapterCount - 1) >= 1
                                   where serie.Chapters.Count(ch => ch.Index == 0) >= 1
                                   select serie).Count()).All(c => c >= 2));
        }

        [TestMethod]
        public void TestXmls_()
        {
            DeleteErrors();
            var from_xml = LoadTestDataList();
            var downloaded = LoadTestDataList();
            Download(downloaded);
            Assert.IsTrue(Compare(from_xml, downloaded));
            CheckOngoing(downloaded);
        }
    }
}
