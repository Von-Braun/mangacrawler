using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using HtmlAgilityPack;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Collections.Concurrent;

namespace MangaCrawlerLib
{
    public static class ConnectionsLimiter
    {
        public const int MAX_CONNECTIONS = 100;
        public const int MAX_CONNECTIONS_PER_SERVER = 4;

        /// <summary>
        /// To prevent from downloading from one server more than one chapter simultaneously.
        /// </summary>
        private static Dictionary<ServerInfo, QueuedMutex> s_serverPages = 
            new Dictionary<ServerInfo, QueuedMutex>();

        private static Dictionary<ServerInfo, QueuedSemaphore<Priority>> s_serverConnections =
            new Dictionary<ServerInfo, QueuedSemaphore<Priority>>();
        private static QueuedSemaphore<Priority> s_connections =
            new QueuedSemaphore<Priority>(MAX_CONNECTIONS);

        static ConnectionsLimiter()
        {
            foreach (var si in ServerInfo.ServersInfos)
                s_serverConnections.Add(si,
                    new QueuedSemaphore<Priority>(MAX_CONNECTIONS_PER_SERVER));
            foreach (var si in ServerInfo.ServersInfos)
                s_serverPages.Add(si, new QueuedMutex());
        }

        public static void BeginDownloadPages(ChapterInfo a_info, CancellationToken a_token)
        {
            s_serverPages[a_info.SerieInfo.ServerInfo].WaitOne(a_token);
        }

        public static void EndDownloadPages(ChapterInfo a_info)
        {
            s_serverPages[a_info.SerieInfo.ServerInfo].ReleaseMutex();
        }

        private static void Aquire(ServerInfo a_info, Priority a_priority)
        {
            s_connections.WaitOne(a_priority);
            s_serverConnections[a_info].WaitOne(a_priority);
        }

        private static void Aquire(ServerInfo a_info, CancellationToken a_token, 
            Priority a_priority)
        {
            s_connections.WaitOne(a_token, a_priority);
            s_serverConnections[a_info].WaitOne(a_token, a_priority);
        }

        private static void Release(ServerInfo a_info)
        {
            s_serverConnections[a_info].Release();
            s_connections.Release();
        }

        internal static HtmlDocument DownloadDocument(ServerInfo a_info, string a_url = null)
        {
            return DownloadWithRetry(() =>
            {
                Aquire(a_info, Priority.Series);

                try
                {
                    if (a_url == null)
                        a_url = a_info.URL;

                    var web = new HtmlWeb();
                    var page = web.Load(a_url);

                    if (web.StatusCode == HttpStatusCode.NotFound)
                        return null;

                    return page;
                }
                finally
                {
                    Release(a_info);
                }
            });
        }

        internal static HtmlDocument DownloadDocument(SerieInfo a_info, string a_url = null)
        {
            return DownloadWithRetry(() =>
            {
                Aquire(a_info.ServerInfo, Priority.Chapters);

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
            });
        }

        internal static HtmlDocument DownloadDocument(ChapterInfo a_info, 
            CancellationToken a_token, string a_url = null)
        {
            return DownloadWithRetry(() =>
            {
                a_token.ThrowIfCancellationRequested();

                Aquire(a_info.SerieInfo.ServerInfo, a_token, Priority.Pages);

                try
                {
                    a_token.ThrowIfCancellationRequested();

                    if (a_url == null)
                        a_url = a_info.URL;

                    return new HtmlWeb().Load(a_url);
                }
                finally
                {
                    Release(a_info.SerieInfo.ServerInfo);
                }
            });
        }

        internal static HtmlDocument DownloadDocument(PageInfo a_info, 
            CancellationToken a_token, string a_url = null)
        {
            return DownloadWithRetry(() =>
            {
                a_token.ThrowIfCancellationRequested();

                Aquire(a_info.ChapterInfo.SerieInfo.ServerInfo, a_token, Priority.Pages);

                try
                {
                    a_token.ThrowIfCancellationRequested();

                    if (a_url == null)
                        a_url = a_info.URL;

                    return new HtmlWeb().Load(a_url);
                }
                finally
                {
                    Release(a_info.ChapterInfo.SerieInfo.ServerInfo);
                }
            });
        }

        internal static T DownloadWithRetry<T>(Func<T> a_func)
        {
            WebException ex1 = null;

            for (int i = 0; i<3; i++)
            {
                try
                {
                    return a_func();
                }
                catch (WebException ex)
                {
                    ex1 = ex;
                    continue;
                }
            }

            throw ex1;
        }

        internal static HtmlDocument Submit(PageInfo a_info, CancellationToken a_token, 
            string a_url, Dictionary<string, string> a_parameters)
        {
            return DownloadWithRetry(() =>
            {
                a_token.ThrowIfCancellationRequested();

                Aquire(a_info.ChapterInfo.SerieInfo.ServerInfo, a_token, Priority.Pages);

                try
                {
                    a_token.ThrowIfCancellationRequested();

                    return HTTPUtils.Submit(a_url, a_parameters);
                }
                finally
                {
                    Release(a_info.ChapterInfo.SerieInfo.ServerInfo);
                }
            });
        }

        public static MemoryStream GetImageStream(PageInfo a_info, CancellationToken a_token)
        {
            return DownloadWithRetry(() =>
            {
                try
                {
                    Aquire(a_info.ChapterInfo.SerieInfo.ServerInfo, a_token, Priority.Image);

                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(
                        a_info.GetImageURL(a_token));

                    myReq.UserAgent = HTTPUtils.UserAgent;
                    myReq.Referer = a_info.URL;

                    using (Stream image_stream = myReq.GetResponse().GetResponseStream())
                    {
                        MemoryStream mem_stream = new MemoryStream();
                        image_stream.CopyTo(mem_stream);
                        mem_stream.Position = 0;
                        return mem_stream;
                    }
                }
                finally
                {
                    Release(a_info.ChapterInfo.SerieInfo.ServerInfo);
                }
            });
        }
    }
}
