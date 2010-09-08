using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MangaCrawlerLib
{
    public class ServerInfo
    {
        private List<SerieInfo> m_series;
        private volatile int m_downloadingProgress;
        private string m_url;

        internal Crawler Crawler;

        internal volatile bool DownloadingSeries;

        public string URL
        {
            get
            {
                if (m_url == null)
                    m_url = HttpUtility.HtmlDecode(Crawler.GetServerURL());

                return m_url;
            }
        }

        public List<SerieInfo> Series
        {
            get
            {
                return m_series;
            }
        }

        public void DownloadSeries(Action a_progress_callback = null)
        {
            if (DownloadingSeries)
                return;

            if (m_series == null)
            {
                DownloadingSeries = true;

                try
                {
                    if (a_progress_callback != null)
                        a_progress_callback();

                    m_series = Crawler.DownloadSeries(this, (progress) =>
                    {
                        m_downloadingProgress = progress;
                        if (a_progress_callback != null)
                            a_progress_callback();
                    }).ToList();
                }
                finally
                {
                    DownloadingSeries = false;
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
            if (DownloadingSeries)
                return String.Format("{0} ({1}%)", Crawler.Name, m_downloadingProgress);
            else if (m_series == null)
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

        public static ServerInfo CreateMangaRun()
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
    }
}
