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

        private static Dictionary<Server, QueuedMutex> s_one_chapter_per_server = 
            new Dictionary<Server, QueuedMutex>();

        private static Dictionary<Server, QueuedSemaphore<Priority>> s_serverConnections =
            new Dictionary<Server, QueuedSemaphore<Priority>>();
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

        public static void BeginDownloadPages(Chapter a_chapter)
        {
            Loggers.ConLimits.InfoFormat("Locking one per server, chapter: {0} state: {1}",
                a_chapter, a_chapter.State);

            s_one_chapter_per_server[a_chapter.Server].WaitOne(a_chapter.Token);

            Loggers.ConLimits.InfoFormat("Locked one per server, chapter: {0} state: {1}",
               a_chapter, a_chapter.State);
        }

        public static void EndDownloadPages(Chapter a_chapter)
        {
            Loggers.ConLimits.InfoFormat("Releasing one per server, chapter: {0} state: {1}",
                a_chapter, a_chapter.State);

            s_one_chapter_per_server[a_chapter.Server].ReleaseMutex();
        }

        public static void Aquire(Server a_server, Priority a_priority)
        {
            Aquire(a_server, CancellationToken.None, a_priority);
        }

        public static void Aquire(Server a_server, CancellationToken a_token, 
            Priority a_priority)
        {
            Loggers.ConLimits.InfoFormat(
                "Aquiring global connection limit, server name: {0}",
                a_server.Name);

            s_connections.WaitOne(a_token, a_priority);

            Loggers.ConLimits.InfoFormat(
                "Aquired global connection limit, server name: {0}",
                a_server.Name);

            Loggers.ConLimits.InfoFormat(
                "Aquiring server connection limit, server name: {0}",
                a_server.Name);

            // Should never block. Scheduler do the job. 
            Debug.Assert(!s_serverConnections[a_server].Saturated);

            s_serverConnections[a_server].WaitOne(a_token, a_priority);

            Loggers.ConLimits.InfoFormat(
                "Aquired server connection limit, server name: {0}",
                a_server.Name);
        }

        public static void Release(Server a_server)
        {
            Loggers.ConLimits.InfoFormat(
                "Releasing global connection limit, server name: {0}",
                a_server.Name);

            s_serverConnections[a_server].Release();

            Loggers.ConLimits.InfoFormat(
                "Releasing server connection limit, server name: {0}",
                a_server.Name);

            s_connections.Release();
        }
    }
}
