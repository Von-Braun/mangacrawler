using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MangaCrawlerLib;
using MangaCrawler;
using System.Diagnostics;
using System.IO;

namespace MangaCrawlerTest
{
    [TestClass]
    public class TestBase
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


        protected virtual void WriteLine(string a_str, params object[] a_args)
        {
            TestContext.WriteLine(a_str, a_args);
            Debug.WriteLine(a_str, a_args);
        }

        protected void WriteLineError(string a_str, params object[] a_args)
        {
            WriteLine(a_str, a_args);
        }

        protected void WriteLineWarning(string a_str, params object[] a_args)
        {
            WriteLine(a_str, a_args);
        }
    }
}
