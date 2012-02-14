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

        public static void BeginDownloadPages(TaskInfo a_task_info)
        {
            Loggers.ConLimits.InfoFormat("Locking one per server, task: {0} state: {1}",
                a_task_info, a_task_info.State);

            s_serverPages[a_task_info.Server].WaitOne(a_task_info.Token);

            Loggers.ConLimits.InfoFormat("Locked one per server, task: {0} state: {1}",
               a_task_info, a_task_info.State);
        }

        public static void EndDownloadPages(TaskInfo a_task_info)
        {
            Loggers.ConLimits.InfoFormat("Releasing one per server, task: {0} state: {1}",
                a_task_info, a_task_info.State);

            s_serverPages[a_task_info.Server].ReleaseMutex();
        }

        private static void Aquire(ServerInfo a_info, Priority a_priority)
        {
            Loggers.ConLimits.InfoFormat("Aquiring global connection limit, server name: {0}",
                a_info.Name);

            s_connections.WaitOne(a_priority);

            Loggers.ConLimits.InfoFormat("Aquired global connection limit, server name: {0}",
                a_info.Name);

            Loggers.ConLimits.InfoFormat("Aquiring server connection limit, server name: {0}",
                a_info.Name);

            // Should never block. Scheduler do the job. 
            Debug.Assert(!s_serverConnections[a_info].Saturated);

            s_serverConnections[a_info].WaitOne(a_priority);

            Loggers.ConLimits.InfoFormat(
                "Aquired server connection limit, server name: {0}",
                a_info.Name);
        }

        private static void Aquire(ServerInfo a_info, CancellationToken a_token, 
            Priority a_priority)
        {
            Loggers.ConLimits.InfoFormat(
                "Aquiring global connection limit, server name: {0}",
                a_info.Name);

            s_connections.WaitOne(a_token, a_priority);

            Loggers.ConLimits.InfoFormat(
                "Aquired global connection limit, server name: {0}",
                a_info.Name);

            Loggers.ConLimits.InfoFormat(
                "Aquiring server connection limit, server name: {0}",
                a_info.Name);

            // Should never block. Scheduler do the job. 
            Debug.Assert(!s_serverConnections[a_info].Saturated);

            s_serverConnections[a_info].WaitOne(a_token, a_priority);

            Loggers.ConLimits.InfoFormat(
                "Aquired server connection limit, server name: {0}",
                a_info.Name);
        }

        private static void Release(ServerInfo a_info)
        {
            Loggers.ConLimits.InfoFormat(
                "Releasing global connection limit, server name: {0}",
                a_info.Name);

            s_serverConnections[a_info].Release();

            Loggers.ConLimits.InfoFormat(
                "Releasing server connection limit, server name: {0}",
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
                        Loggers.MangaCrawler.InfoFormat(
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
                Aquire(a_info.Server, Priority.Chapters);

                try
                {
                    if (a_url == null)
                        a_url = a_info.URL;

                    var web = new HtmlWeb();
                    var page = web.Load(a_url);

                    if (web.StatusCode == HttpStatusCode.NotFound)
                    {
                        Loggers.MangaCrawler.InfoFormat(
                            "ConnectionsLimiter.DownloadDocument - chapters - page is null, url: {0}",
                            a_url);

                        return null;
                    }

                    return page;
                }
                finally
                {
                    Release(a_info.Server);
                }
            });
        }

        internal static HtmlDocument DownloadDocument(TaskInfo a_task_info, string a_url = null)
        {
            return DownloadWithRetry(() =>
            {
                if (a_task_info.Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "Pages - #1 token cancelled, a_url: {0}",
                        a_url);
                }

                a_task_info.Token.ThrowIfCancellationRequested();

                Aquire(a_task_info.Server, a_task_info.Token, Priority.Pages);

                try
                {
                    if (a_task_info.Token.IsCancellationRequested)
                    {
                        Loggers.Cancellation.InfoFormat(
                            "Pages - #2 token cancelled, a_url: {0}",
                            a_url);
                    }

                    a_task_info.Token.ThrowIfCancellationRequested();

                    if (a_url == null)
                        a_url = a_task_info.URL;

                    var web = new HtmlWeb();
                    var page = web.Load(a_url);

                    if (web.StatusCode == HttpStatusCode.NotFound)
                    {
                        Loggers.MangaCrawler.InfoFormat(
                            "Pages - page is null, url: {0}",
                            a_url);

                        return null;
                    }

                    return page;
                }
                finally
                {
                    Release(a_task_info.Server);
                }
            });
        }

        internal static HtmlDocument DownloadDocument(PageInfo a_page_info, string a_url = null)
        {
            return DownloadWithRetry(() =>
            {
                if (a_page_info.TaskInfo.Token.IsCancellationRequested)
                {
                    Loggers.Cancellation.InfoFormat(
                        "Page - #1 token cancelled, a_url: {0}",
                        a_url);
                }

                a_page_info.TaskInfo.Token.ThrowIfCancellationRequested();

                Aquire(a_page_info.TaskInfo.Server, a_page_info.TaskInfo.Token, Priority.Pages);

                try
                {
                    if (a_page_info.TaskInfo.Token.IsCancellationRequested)
                    {
                        Loggers.Cancellation.InfoFormat(
                            "Page - #2 token cancelled, a_url: {0}",
                            a_url);
                    }

                    a_page_info.TaskInfo.Token.ThrowIfCancellationRequested();

                    if (a_url == null)
                        a_url = a_page_info.URL;

                    var web = new HtmlWeb();
                    var page = web.Load(a_url);

                    if (web.StatusCode == HttpStatusCode.NotFound)
                    {
                        Loggers.MangaCrawler.InfoFormat(
                            "Page - page is null, url: {0}",
                            a_url);

                        return null;
                    }

                    return page;
                }
                finally
                {
                    Release(a_page_info.TaskInfo.Server);
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
                    Loggers.MangaCrawler.Info("exception, {0}", ex);

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
                    Aquire(a_info.TaskInfo.Server, a_info.TaskInfo.Token, Priority.Image);
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
                    Release(a_info.TaskInfo.Server);
                }
            });
        }
    }
}
