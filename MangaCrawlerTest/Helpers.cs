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
    public static class Helpers
    {
        private static string DOWNLOADED_SUFFIX = " - downloaded.xml";

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

        public static List<ServerTestData> LoadTestDataList()
        {
            return (from test_data_xml in Directory.EnumerateFiles(GetTestDataDir(), "*.xml")
                    select ServerTestData.Load(test_data_xml)).ToList();
        }

        public static void Download(List<ServerTestData> a_list)
        {
            foreach (var test_data in a_list)
            {
                try
                {
                    test_data.Download();
                }
                catch
                {
                }
            }
        }

        public static void DeleteTestData(string a_server_name)
        {
            foreach (var file in Directory.GetFiles(GetTestDataDir()).Where(el => el.StartsWith(a_server_name)))
                File.Delete(file);
        }

        public static void DeleteDownloaded()
        {
            foreach (var file in Directory.GetFiles(GetTestDataDir(), DOWNLOADED_SUFFIX))
                File.Delete(file);
        }

        public static bool Compare(List<ServerTestData> a_from_xml, List<ServerTestData> a_downloaded)
        {
            Assert.AreEqual(a_from_xml.Count, a_downloaded.Count);

            bool result = true;

            for (int i = 0; i < a_from_xml.Count; i++)
                    result &= Compare(a_from_xml[i], a_downloaded[i]);

            return result;
        }

        private static bool Compare(ServerTestData a_from_xml, ServerTestData a_downloaded)
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

        public static void GenerateInfo(ServerTestData a_server_test_data, bool a_downloaded = true)
        {
            string a_suffix = a_downloaded ? DOWNLOADED_SUFFIX : "";

            a_server_test_data.Save(Path.Combine(GetTestDataDir(), a_server_test_data.Name + a_suffix + ".xml"));

            foreach (var page in from serie in a_server_test_data.Series
                                 from chapter in serie.Chapters
                                 from page in chapter.Pages
                                 select page)
            {
                var image_name = Path.GetFileNameWithoutExtension(page.FileName) + a_suffix + 
                    Path.GetExtension(page.FileName);
                page.Image.Save(image_name);
            }
        }
    }
}
