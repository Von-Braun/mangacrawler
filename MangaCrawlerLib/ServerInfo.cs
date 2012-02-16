using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using TomanuExtensions;
using System.Threading;
using MangaCrawlerLib.Crawlers;

namespace MangaCrawlerLib
{
    public class ServerInfo
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string m_url;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<SerieInfo> m_series = new List<SerieInfo>();

        public int DownloadProgress { get; private set; }
        public ServerState State;
        internal CustomTaskScheduler Scheduler { get; private set; }
        internal Crawler Crawler { get; private set; }

        private static readonly ServerInfo[] s_servers_infos;

        static ServerInfo()
        {
            #if TEST_SERVERS
            s_servers_infos = new [] 
            {
                //new ServerInfo(new TestServerCrawler("normal", 1000, false, false, false, 0)), 
                //new ServerInfo(new TestServerCrawler("empty", 500, false, false, true, 0)), 
                new ServerInfo(new TestServerCrawler("fast", 300, false, false, false, 0)), 
                //new ServerInfo(new TestServerCrawler("fast, max_con", 300, false, false, false, 1)), 
                //new ServerInfo(new TestServerCrawler("very_slow", 3000, false, false, false, 0)), 
                //new ServerInfo(new TestServerCrawler("normal, slow series chapters", 1000, true, true, false, 0)), 
                //new ServerInfo(new TestServerCrawler("fast, slow series chapters", 300, true, true, false, 0)), 
                //new ServerInfo(new TestServerCrawler("very_slow, slow series chapters", 3000, true, true, false, 0)), 
                //new ServerInfo(new TestServerCrawler("very_slow, max_con, slow series chapters", 3000, true, true, false, 1)), 
            };
            #else
            s_serversInfos = (from hf in System.Reflection.Assembly.GetAssembly(typeof(ServerInfo)).GetTypes()
                              where hf.IsClass
                              where !hf.IsAbstract
                              where hf.IsDerivedFrom(typeof(Crawler))
                              where hf != typeof(MangaToshokanCrawler)
                              select new ServerInfo((Crawler)Activator.CreateInstance(hf))).ToArray();
            #endif
        }

        internal ServerInfo(Crawler a_crawler)
        {
            Crawler = a_crawler;
            Scheduler = new CustomTaskScheduler(Crawler.MaxConnectionsPerServer, Crawler.Name);
        }

        public static IEnumerable<ServerInfo> ServersInfos
        {
            get
            {
                return s_servers_infos;
            }
        }

        public string URL
        {
            get
            {
                if (m_url == null)
                    m_url = HttpUtility.HtmlDecode(Crawler.GetServerURL());

                return m_url;
            }
        }

        public IEnumerable<SerieInfo> Series
        {
            get
            {
                return m_series;
            }
        }

        internal void DownloadSeries()
        {
            try
            {
                DownloadProgress = 0;

                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    var series = result.ToList();

                    foreach (var serie in m_series)
                    {
                        var el = series.Find(s => (s.Title == serie.Title) && (s.URL == serie.URL));
                        if (el != null)
                            series[series.IndexOf(el)] = serie;
                    }

                    m_series = series;
                    DownloadProgress = progress;
                });

                State = ServerState.Downloaded;
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                State = ServerState.Error;
            }
        }

        public string Name
        {
            get
            {
                return Crawler.Name;
            }
        }

        public static ServerInfo AnimeSource
        {
            get
            {
                return ServersInfos.First(si => si.Name == new AnimeSourceCrawler().Name);
            }
        }

        public static ServerInfo MangaAccess
        {
            get
            {
                return ServersInfos.First(si => si.Name == new MangaAccessCrawler().Name);
            }
        }

        public static ServerInfo MangaFox
        {
            get
            {
                return ServersInfos.First(si => si.Name == new MangaFoxCrawler().Name);
            }
        }

        public static ServerInfo MangaRun
        {
            get
            {
                return ServersInfos.First(si => si.Name == new MangaRunCrawler().Name);
            }
        }

        public static ServerInfo MangaShare
        {
            get
            {
                return ServersInfos.First(si => si.Name == new MangaShareCrawler().Name);
            }
        }

        public static ServerInfo MangaToshokan
        {
            get
            {
                return ServersInfos.First(si => si.Name == new MangaToshokanCrawler().Name);
            }
        }

        public static ServerInfo MangaVolume
        {
            get
            {
                return ServersInfos.First(si => si.Name == new MangaVolumeCrawler().Name);
            }
        }

        public static ServerInfo OtakuWorks
        {
            get
            {
                return ServersInfos.First(si => si.Name == new OtakuWorksCrawler().Name);
            }
        }

        public static ServerInfo OurManga
        {
            get
            {
                return ServersInfos.First(si => si.Name == new OurMangaCrawler().Name);
            }
        }

        public static ServerInfo SpectrumNexus
        {
            get
            {
                return ServersInfos.First(si => si.Name == new SpectrumNexusCrawler().Name);
            }
        }

        public static ServerInfo StopTazmo
        {
            get
            {
                return ServersInfos.First(si => si.Name == new StopTazmoCrawler().Name);
            }
        }

        public static ServerInfo UnixManga
        {
            get
            {
                return ServersInfos.First(si => si.Name == new UnixMangaCrawler().Name);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        internal bool DownloadRequired
        {
            get
            {
                var s = State;
                return (s == ServerState.Error) || (s == ServerState.Initial);
            }
        }
    }
}
