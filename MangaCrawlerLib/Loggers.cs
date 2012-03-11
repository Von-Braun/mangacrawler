using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace MangaCrawlerLib
{
    // Sync with MangaCrawler/app.config
    public static class Loggers
    {
        public static ILog Cancellation = LogManager.GetLogger("Cancellation");
        public static ILog Test = LogManager.GetLogger("Test");
        public static ILog MangaCrawler = LogManager.GetLogger("MangaCrawler");
        public static ILog GUI = LogManager.GetLogger("GUI");
    }
}
