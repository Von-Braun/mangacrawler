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
        public static ILog ConLimits = LogManager.GetLogger("ConLimits");
        public static ILog Cancellation = LogManager.GetLogger("Cancellation");
        public static ILog Settings = LogManager.GetLogger("Settings");
        public static ILog NHibernate = LogManager.GetLogger("NHibernate");
        public static ILog NH = LogManager.GetLogger("NH");
        public static ILog MangaCrawler = LogManager.GetLogger("MangaCrawler");
        public static ILog GUI = LogManager.GetLogger("GUI");
    }
}
