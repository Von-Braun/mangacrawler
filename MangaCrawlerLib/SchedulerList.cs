using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaCrawlerLib
{
    internal static class SchedulerList
    {
        private static Dictionary<string, CustomTaskScheduler> s_schedulers =
            new Dictionary<string, CustomTaskScheduler>();

        public static CustomTaskScheduler Get(ServerInfo a_server)
        {
            CustomTaskScheduler cts;
            if (s_schedulers.TryGetValue(a_server.URL, out cts))
                return cts;

            cts = new CustomTaskScheduler(a_server.Crawler.MaxConnectionsPerServer, a_server.Name);
            s_schedulers.Add(a_server.URL, cts);
            return cts;
        }

        public static IEnumerable<CustomTaskScheduler> Schedulers
        {
            get
            {
                return s_schedulers.Values;
            }
        }
    }
}
