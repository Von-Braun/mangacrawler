using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib.Crawlers;

namespace MangaCrawlerLib
{
    public static class ServerList
    {
        private static List<ServerInfo> s_list = new List<ServerInfo>();

        static ServerList()
        {
            s_list = (from c in CrawlerList.Crawlers
                      select new ServerInfo(c.GetServerURL(), c.Name)).ToList();
        }

        public static IEnumerable<ServerInfo> Servers
        {
            get
            {
                return s_list;
            }
        }
    }
}
