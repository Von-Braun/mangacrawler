using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib.Crawlers;

namespace MangaCrawlerLib
{
    public static class ServerList
    {
        private static List<Server> s_list = new List<Server>();

        static ServerList()
        {
            s_list = (from c in CrawlerList.Crawlers
                      select new Server(c.GetServerURL(), c.Name)).ToList();
        }

        public static IEnumerable<Server> Servers
        {
            get
            {
                return s_list;
            }
        }
    }
}
