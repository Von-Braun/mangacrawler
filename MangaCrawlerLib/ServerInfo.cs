using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    [DebuggerDisplay("ServerInfo, {ToString()}")]
    public class ServerInfo
    {
        private string m_url;
        private IEnumerable<SerieInfo> m_series;

        internal readonly Crawler Crawler;

        private static readonly ServerInfo[] m_serversInfos;

        static ServerInfo()
        {
            m_serversInfos = (from hf in System.Reflection.Assembly.GetAssembly(typeof(ServerInfo)).GetTypes()
                              where hf.IsClass
                              where !hf.IsAbstract
                              where typeof(Crawler).IsAssignableFrom(hf)
                              select new ServerInfo((Crawler)Activator.CreateInstance(hf))).ToArray();
        }

        internal ServerInfo(Crawler a_crawler)
        {
            Crawler = a_crawler;
        }

        public static IEnumerable<ServerInfo> ServersInfos
        {
            get
            {
                return from si in m_serversInfos
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
            get
            {
                if (m_series == null)
                    return new SerieInfo[0];

                return from serie in m_series
                       select serie;
            }
        }

        public void DownloadSeries(Action<int> a_progress_callback = null)
        {
            if (a_progress_callback != null)
                a_progress_callback(0);

            Crawler.DownloadSeries(this, (progress, result) =>
            {
                var series = result.ToList();

                if (m_series != null)
                {
                    foreach (var serie in m_series)
                    {
                        var el = series.Find(s => (s.Name == serie.Name) && (s.URL == serie.URL));
                        if (el != null)
                            series[series.IndexOf(el)] = serie;
                    }
                }

                m_series = series;

                if (a_progress_callback != null)
                    a_progress_callback(progress);
            });
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

        public static ServerInfo BleachExile
        {
            get
            {
                return ServersInfos.First(si => si.Name == new BleachExileCrawler().Name);
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
    }
}
