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
        public static ILog MangaCrawler = LogManager.GetLogger("MangaCrawler");
        public static ILog GUI = LogManager.GetLogger("GUI");

        public static bool Log()
        {
            #if DEBUG
            return true;
            #elif LOCAL_SERVERS
            return true;
            #else
            return false;
            #endif
        }
    }
}
