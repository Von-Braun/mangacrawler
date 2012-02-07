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
    internal static class ConnectionsLimiter
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
            {
                s_serverConnections.Add(si,
                    new QueuedSemaphore<Priority>(si.Crawler.MaxConnectionsPerServer));
            }
            foreach (var si in ServerInfo.ServersInfos)
                s_serverPages.Add(si, new QueuedMutex());
        }

        public static void BeginDownloadPages(ChapterInfo a_chapter_info)
        {
            System.Diagnostics.Debug.WriteLine("ConnectionsLimiter.BeginDownloadPages - locking one per server, title: {0} state: {1}",
                a_chapter_info.Title, a_chapter_info.State);

            s_serverPages[a_chapter_info.SerieInfo.ServerInfo].WaitOne(a_chapter_info.Token);

            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.BeginDownloadPages - locked one per server, title: {0} state: {1}",
                a_chapter_info.Title, a_chapter_info.State);
        }

        public static void EndDownloadPages(ChapterInfo a_chapter_info)
        {
            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.EndDownloadPages - releasing one per server, title: {0} state: {1}",
                a_chapter_info.Title, a_chapter_info.State);

            s_serverPages[a_chapter_info.SerieInfo.ServerInfo].ReleaseMutex();
        }

        private static void Aquire(ServerInfo a_info, Priority a_priority)
        {
            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.Aquire1 - aquiring global connection limit, server name: {0}",
                a_info.Name);

            s_connections.WaitOne(a_priority);

            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.Aquire1 - aquired global connection limit, server name: {0}",
                a_info.Name);

            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.Aquire1 - aquiring server connection limit, server name: {0}",
                a_info.Name);

            // Should never block. Scheduler do the job. 
            Debug.Assert(!s_serverConnections[a_info].Saturated);

            s_serverConnections[a_info].WaitOne(a_priority);

            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.Aquire1 - aquired server connection limit, server name: {0}",
                a_info.Name);
        }

        private static void Aquire(ServerInfo a_info, CancellationToken a_token, 
            Priority a_priority)
        {
            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.Aquire2 - aquiring global connection limit, server name: {0}",
                a_info.Name);

            s_connections.WaitOne(a_token, a_priority);

            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.Aquire2 - aquired global connection limit, server name: {0}",
                a_info.Name);

            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.Aquire2 - aquiring server connection limit, server name: {0}",
                a_info.Name);

            // Should never block. Scheduler do the job. 
            Debug.Assert(!s_serverConnections[a_info].Saturated);

            s_serverConnections[a_info].WaitOne(a_token, a_priority);

            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.Aquire2 - aquired server connection limit, server name: {0}",
                a_info.Name);
        }

        private static void Release(ServerInfo a_info)
        {
            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.Release - releasing global connection limit, server name: {0}",
                a_info.Name);

            s_serverConnections[a_info].Release();

            System.Diagnostics.Debug.WriteLine(
                "ConnectionsLimiter.Release - releasing server connection limit, server name: {0}",
                a_info.Name);

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
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "ConnectionsLimiter.DownloadDocument - series - page is null, url: {0}", 
                            a_url);

                        return null;
                    }

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

                    var web = new HtmlWeb();
                    var page = web.Load(a_url);

                    if (web.StatusCode == HttpStatusCode.NotFound)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "ConnectionsLimiter.DownloadDocument - chapters - page is null, url: {0}",
                            a_url);

                        return null;
                    }

                    return page;
                }
                finally
                {
                    Release(a_info.ServerInfo);
                }
            });
        }

        internal static HtmlDocument DownloadDocument(ChapterInfo a_info, string a_url = null)
        {
            return DownloadWithRetry(() =>
            {
                if (a_info.Token.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "ConnectionsLimiter.DownloadDocument - pages - #1 token cancelled, a_url: {0}",
                        a_url);
                }

                a_info.Token.ThrowIfCancellationRequested();

                Aquire(a_info.SerieInfo.ServerInfo, a_info.Token, Priority.Pages);

                try
                {
                    if (a_info.Token.IsCancellationRequested)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "ConnectionsLimiter.DownloadDocument - pages - #2 token cancelled, a_url: {0}",
                            a_url);
                    }

                    a_info.Token.ThrowIfCancellationRequested();

                    if (a_url == null)
                        a_url = a_info.URL;

                    var web = new HtmlWeb();
                    var page = web.Load(a_url);

                    if (web.StatusCode == HttpStatusCode.NotFound)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "ConnectionsLimiter.DownloadDocument - pages - page is null, url: {0}",
                            a_url);

                        return null;
                    }

                    return page;
                }
                finally
                {
                    Release(a_info.SerieInfo.ServerInfo);
                }
            });
        }

        internal static HtmlDocument DownloadDocument(PageInfo a_info, string a_url = null)
        {
            return DownloadWithRetry(() =>
            {
                if (a_info.ChapterInfo.Token.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "ConnectionsLimiter.DownloadDocument - page - #1 token cancelled, a_url: {0}",
                        a_url);
                }

                a_info.ChapterInfo.Token.ThrowIfCancellationRequested();

                Aquire(a_info.ChapterInfo.SerieInfo.ServerInfo, a_info.ChapterInfo.Token, Priority.Pages);

                try
                {
                    if (a_info.ChapterInfo.Token.IsCancellationRequested)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "ConnectionsLimiter.DownloadDocument - page - #2 token cancelled, a_url: {0}",
                            a_url);
                    }

                    a_info.ChapterInfo.Token.ThrowIfCancellationRequested();

                    if (a_url == null)
                        a_url = a_info.URL;

                    var web = new HtmlWeb();
                    var page = web.Load(a_url);

                    if (web.StatusCode == HttpStatusCode.NotFound)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "ConnectionsLimiter.DownloadDocument - page - page is null, url: {0}",
                            a_url);

                        return null;
                    }

                    return page;
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
                    System.Diagnostics.Debug.WriteLine(
                            "ConnectionsLimiter.DownloadWithRetry - exception, {0}",
                            ex);

                    ex1 = ex;
                    continue;
                }
            }

            throw ex1;
        }

        public static MemoryStream GetImageStream(PageInfo a_info)
        {
            return DownloadWithRetry(() =>
            {
                try
                {
                    Aquire(a_info.ChapterInfo.SerieInfo.ServerInfo, a_info.ChapterInfo.Token, Priority.Image);
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(
                        a_info.GetImageURL());

                    myReq.UserAgent = DownloadManager.UserAgent;
                    myReq.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
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
