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

        private static Dictionary<ServerInfo, QueuedMutex> s_one_chapter_per_server = 
            new Dictionary<ServerInfo, QueuedMutex>();

        private static Dictionary<ServerInfo, QueuedSemaphore<Priority>> s_serverConnections =
            new Dictionary<ServerInfo, QueuedSemaphore<Priority>>();
        private static QueuedSemaphore<Priority> s_connections =
            new QueuedSemaphore<Priority>(MAX_CONNECTIONS);

        static ConnectionsLimiter()
        {
            foreach (var si in ServerList.Servers)
            {
                s_serverConnections.Add(si,
                    new QueuedSemaphore<Priority>(si.Crawler.MaxConnectionsPerServer));
            }
            foreach (var si in ServerList.Servers)
                s_one_chapter_per_server.Add(si, new QueuedMutex());
        }

        public static void BeginDownloadPages(TaskInfo a_task_info)
        {
            Loggers.ConLimits.InfoFormat("Locking one per server, task: {0} state: {1}",
                a_task_info, a_task_info.State);

            s_one_chapter_per_server[a_task_info.Chapter.Serie.Server].WaitOne(a_task_info.Token);

            Loggers.ConLimits.InfoFormat("Locked one per server, task: {0} state: {1}",
               a_task_info, a_task_info.State);
        }

        public static void EndDownloadPages(TaskInfo a_task_info)
        {
            Loggers.ConLimits.InfoFormat("Releasing one per server, task: {0} state: {1}",
                a_task_info, a_task_info.State);

            s_one_chapter_per_server[a_task_info.Chapter.Serie.Server].ReleaseMutex();
        }

        public static void Aquire(ServerInfo a_info, Priority a_priority)
        {
            Aquire(a_info, CancellationToken.None, a_priority);
        }

        public static void Aquire(ServerInfo a_info, CancellationToken a_token, 
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

        public static void Release(ServerInfo a_info)
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
    }
}
