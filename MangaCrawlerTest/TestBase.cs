using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MangaCrawlerLib;
using MangaCrawler;

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
    }
}
