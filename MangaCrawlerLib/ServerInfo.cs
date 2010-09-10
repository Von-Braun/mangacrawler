using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MangaCrawlerLib
{
    public class ServerInfo
    {
        public List<SerieInfo> Series;
        private volatile int m_downloadingProgress;
        private volatile bool m_downloadingSeries;
        private string m_url;
        internal Crawler Crawler;

        public string URL
        {
            get
            {
                if (m_url == null)
                    m_url = HttpUtility.HtmlDecode(Crawler.GetServerURL());

                return m_url;
            }
        }

        public void DownloadSeries(Action a_progress_callback = null)
        {
            if (m_downloadingSeries)
                return;

            if (Series == null)
            {
                m_downloadingSeries = true;

                try
                {
                    if (a_progress_callback != null)
                        a_progress_callback();

                    Series = Crawler.DownloadSeries(this, (progress) =>
                    {
                        m_downloadingProgress = progress;
                        if (a_progress_callback != null)
                            a_progress_callback();
                    }).ToList();
                }
                finally
                {
                    m_downloadingSeries = false;
                }
            }

            if (a_progress_callback != null)
                a_progress_callback();
        }

        public string Name
        {
            get
            {
                return Crawler.Name;
            }
        }

        internal string GetDecoratedName()
        {
            if (m_downloadingSeries)
                return String.Format("{0} ({1}%)", Crawler.Name, m_downloadingProgress);
            else if (Series == null)
                return Crawler.Name;
            else
                return Crawler.Name + "*";
        }

        public override string ToString()
        {
            return GetDecoratedName();
        }

        public static IEnumerable<ServerInfo> CreateServerInfos()
        {
            return from hf in System.Reflection.Assembly.GetAssembly(typeof(ServerInfo)).GetTypes()
                   where hf.IsClass
                   where !hf.IsAbstract
                   where hf != typeof(Manga1000Crawler)
                   where hf != typeof(OneMangaCrawler)
                   where typeof(Crawler).IsAssignableFrom(hf)
                   select new ServerInfo() { Crawler = (Crawler)Activator.CreateInstance(hf) };
        }

        public static ServerInfo CreateAnimeSource()
        {
            return new ServerInfo { Crawler = new AnimeSourceCrawler() };
        }

        public static ServerInfo CreateMangaFox()
        {
            return new ServerInfo { Crawler = new MangaFoxCrawler() };
        }

        public static ServerInfo CreateMangaRunServerInfo()
        {
            return new ServerInfo { Crawler = new MangaRunCrawler() };
        }

        public static ServerInfo CreateMangaShare()
        {
            return new ServerInfo { Crawler = new MangaShareCrawler() };
        }

        public static ServerInfo CreateMangaToshokan()
        {
            return new ServerInfo { Crawler = new MangaToshokanCrawler() };
        }

        public static ServerInfo CreateMangaVolume()
        {
            return new ServerInfo { Crawler = new MangaVolumeCrawler() };
        }

        public static ServerInfo CreateOtakuWorks()
        {
            return new ServerInfo { Crawler = new OtakuWorksCrawler() };
        }

        public static ServerInfo CreateOurManga()
        {
            return new ServerInfo { Crawler = new OurMangaCrawler() };
        }

        public static ServerInfo CreateSpectrumNexus()
        {
            return new ServerInfo { Crawler = new SpectrumNexusCrawler() };
        }

        public static ServerInfo CreateStopTazmo()
        {
            return new ServerInfo { Crawler = new StopTazmoCrawler() };
        }

        public static ServerInfo CreateUnixManga()
        {
            return new ServerInfo { Crawler = new UnixMangaCrawler() };
        }
    }
}
