using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using HtmlAgilityPack;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    internal static class ConnectionsLimiter
    {
        private const int MAX_CONNECTIONS = 100;
        private const int MAX_CONNECTIONS_PER_SERVER = 4;

        private static Dictionary<ServerInfo, Semaphore> s_dict = new Dictionary<ServerInfo, Semaphore>();
        private static Semaphore m_connections = new Semaphore(MAX_CONNECTIONS, MAX_CONNECTIONS);

        static ConnectionsLimiter()
        {
            foreach (var si in ServerInfo.ServersInfos)
                s_dict.Add(si, new Semaphore(MAX_CONNECTIONS_PER_SERVER, MAX_CONNECTIONS_PER_SERVER));
        }

        public static void Aquire(ServerInfo a_serverInfo)
        {
            m_connections.WaitOne();
            s_dict[a_serverInfo].WaitOne();
        }

        public static void Release(ServerInfo a_serverInfo)
        {
            m_connections.Release();
            s_dict[a_serverInfo].Release();
        }

        public static HtmlDocument DownloadDocument(ServerInfo a_info, string a_url = null)
        {
            Aquire(a_info);

            try
            {
                if (a_url == null)
                    a_url = a_info.URL;

                return new HtmlWeb().Load(a_url);
            }
            finally
            {
                Release(a_info);
            }
        }

        public static HtmlDocument DownloadDocument(SerieInfo a_info, string a_url = null)
        {
            Aquire(a_info.ServerInfo);

            try
            {
                if (a_url == null)
                    a_url = a_info.URL;

                return new HtmlWeb().Load(a_url);
            }
            finally
            {
                Release(a_info.ServerInfo);
            }
        }

        public static HtmlDocument DownloadDocument(ChapterInfo a_info, string a_url = null)
        {
            Aquire(a_info.SerieInfo.ServerInfo);

            try
            {
                if (a_url == null)
                    a_url = a_info.URL;

                return new HtmlWeb().Load(a_url);
            }
            finally
            {
                Release(a_info.SerieInfo.ServerInfo);
            }
        }

        public static HtmlDocument DownloadDocument(PageInfo a_info, string a_url = null)
        {
            Aquire(a_info.ChapterInfo.SerieInfo.ServerInfo);

            try
            {
                if (a_url == null)
                    a_url = a_info.URL;

                return new HtmlWeb().Load(a_url);
            }
            finally
            {
                Release(a_info.ChapterInfo.SerieInfo.ServerInfo);
            }
        }
    }
}
