using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace MangaCrawlerLib
{
    public static class Loggers
    {
        public static Logger ConnectionsLimits = LogManager.GetLogger("ConnectionsLimits");
        public static Logger Cancellation = LogManager.GetLogger("Cancellation");
        public static Logger Settings = LogManager.GetLogger("Settings");
        public static Logger MangaCrawler = LogManager.GetLogger("MangaCrawler");
        public static Logger DownloadingTasks = LogManager.GetLogger("DownloadingChapters");
        public static Logger GUI = LogManager.GetLogger("GUI");
    }
}
