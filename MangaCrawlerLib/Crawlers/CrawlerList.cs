using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomanuExtensions;

namespace MangaCrawlerLib.Crawlers
{
    internal static class CrawlerList
    {
        private static Dictionary<string, Crawler> s_map = new Dictionary<string, Crawler>();

        static CrawlerList()
        {
            #if LOCAL_SERVERS
            AddCrawlers(GetTestCrawlers());
            #else
            AddCrawlers(GetRealCrawlers());
            #endif
        }

        public static IEnumerable<Crawler> GetTestCrawlers()
        {
            yield return new TestServerCrawler("normal", 1000, false, false, false, 0);
            yield return new TestServerCrawler("empty", 500, false, false, true, 0);
            yield return new TestServerCrawler("fast", 300, false, false, false, 0);
            yield return new TestServerCrawler("no_delay", 0, false, false, false, 0);
            yield return new TestServerCrawler("fast, max_con", 300, false, false, false, 1);
            yield return new TestServerCrawler("very_slow", 3000, false, false, false, 0);
            yield return new TestServerCrawler("normal, slow series chapters", 1000, true, true, false, 0);
            yield return new TestServerCrawler("fast, slow series chapters", 300, true, true, false, 0);
            yield return new TestServerCrawler("very_slow, slow", 3000, true, true, false, 0);
            yield return new TestServerCrawler("very_slow, max_con, slow", 3000, true, true, false, 1);
            yield return new TestServerCrawler("error series none", 3000, true, true, false, 0);
            yield return new TestServerCrawler("error series few", 3000, true, true, false, 0);
        }

        public static IEnumerable<Crawler> GetRealCrawlers()
        {
            return from c in System.Reflection.Assembly.GetAssembly(typeof(Server)).GetTypes()
                   where c.IsClass
                   where !c.IsAbstract
                   where c.IsDerivedFrom(typeof(Crawler))
                   where c != typeof(TestServerCrawler)
                   orderby c.Name
                   select (Crawler)Activator.CreateInstance(c);
        }

        public static Crawler Get(Server a_server)
        {
            return s_map[a_server.URL];
        }

        private static void AddCrawlers(IEnumerable<Crawler> a_crawlers)
        {
            foreach (var crawler in a_crawlers)
                s_map[crawler.GetServerURL()] = crawler;
        }

        public static IEnumerable<Crawler> Crawlers
        {
            get
            {
                return s_map.Values;
            }
        }
    }
}
