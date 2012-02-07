using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using TomanuExtensions;
using System.Threading;

namespace MangaCrawlerLib
{
    public class ServerInfo
    {
        private string m_url;
        private IEnumerable<SerieInfo> m_series;
        private Object m_lock = new Object();

        public int DownloadProgress { get; private set; }
        public ItemState State { get; private set; }
        internal CustomTaskScheduler Scheduler { get; private set; }
        internal Crawler Crawler { get; private set; }

        private static readonly ServerInfo[] s_serversInfos;

        static ServerInfo()
        {
            s_serversInfos = (from hf in System.Reflection.Assembly.GetAssembly(typeof(ServerInfo)).GetTypes()
                              where hf.IsClass
                              where !hf.IsAbstract
                              where hf.IsDerivedFrom(typeof(Crawler))
                              where hf != typeof(MangaToshokanCrawler)
                              select new ServerInfo((Crawler)Activator.CreateInstance(hf))).ToArray();
        }

        internal ServerInfo(Crawler a_crawler)
        {
            Crawler = a_crawler;
            Scheduler = new CustomTaskScheduler(Crawler.MaxConnectionsPerServer);
        }

        public static IEnumerable<ServerInfo> ServersInfos
        {
            get
            {
                return from si in s_serversInfos
                       select si;
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
            [DebuggerStepThrough]
            get
            {
                if (m_series == null)
                    return new SerieInfo[0];

                return from serie in m_series
                       select serie;
            }
        }

        internal void DownloadSeries()
        {
            try
            {
                DownloadProgress = 0;
                State = ItemState.Downloading;

                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    var series = result.ToList();

                    if (m_series != null)
                    {
                        foreach (var serie in m_series)
                        {
                            var el = series.Find(s => (s.Title == serie.Title) && (s.URL == serie.URL));
                            if (el != null)
                                series[series.IndexOf(el)] = serie;
                        }
                    }

                    m_series = series;
                    DownloadProgress = progress;
                });

                State = ItemState.Downloaded;
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                State = ItemState.Error;
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
                lock (m_lock)
                {
                    return (State == ItemState.Error) || (State == ItemState.Initial);
                }
            }
        }
    }
}
