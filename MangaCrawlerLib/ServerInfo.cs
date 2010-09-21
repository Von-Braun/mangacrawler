using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MangaCrawlerLib
{
    public class ServerInfo
    {
        private volatile int m_downloadingProgress;
        private volatile bool m_downloadingSeries;
        private string m_url;
        private IEnumerable<SerieInfo> m_series;

        internal readonly Crawler Crawler;

        private static readonly ServerInfo[] m_serversInfos;

        static ServerInfo()
        {
            m_serversInfos = (from hf in System.Reflection.Assembly.GetAssembly(typeof(ServerInfo)).GetTypes()
                              where hf.IsClass
                              where !hf.IsAbstract
                              where hf != typeof(Manga1000Crawler)
                              where hf != typeof(OneMangaCrawler)
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
                    return null;

                return from serie in m_series
                       select serie;
            }
        }

        public void DownloadSeries(Action a_progress_callback = null)
        {
            if (m_downloadingSeries)
                return;

            if (m_series != null)
                return;

            m_downloadingSeries = true;

            try
            {
                if (a_progress_callback != null)
                    a_progress_callback();

                Crawler.DownloadSeries(this, (progress, result) =>
                {
                    var series = result.ToList();

                    if (m_series != null)
                    {
                        foreach (var serie in m_series)
                        {
                            var el = series.Find(s => s.URL == serie.URL);
                            if (el != null)
                                series[series.IndexOf(el)] = serie;
                        }
                    }

                    m_series = series;

                    m_downloadingProgress = progress;

                    if (a_progress_callback != null)
                        a_progress_callback();
                });
            }
            finally
            {
                m_downloadingSeries = false;
            }
        }

        public string Name
        {
            get
            {
                return Crawler.Name;
            }
        }

        internal string DecoratedName
        {
            get
            {
                if (m_downloadingSeries)
                    return String.Format("{0} ({1}%)", Crawler.Name, m_downloadingProgress);
                else if (m_series == null)
                    return Crawler.Name;
                else
                    return Crawler.Name + "*";
            }
        }

        public override string ToString()
        {
            return DecoratedName;
        }

        public static ServerInfo AnimeSource
        {
            get
            {
                return ServersInfos.First(si => si.Name == new AnimeSourceCrawler().Name);
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
    }
}
