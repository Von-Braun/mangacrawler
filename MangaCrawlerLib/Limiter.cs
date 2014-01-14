using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MangaCrawlerLib
{
    internal static class Limiter
    {
        #region Priority
        internal enum Priority
        {
            Chapters = 1,
            Series = 2,
            Pages = 3,
            Image = 4,
        }
        #endregion

        #region Limit
        private class Limit : IDisposable
        {
            public Priority Priority { get; private set; }
            public AutoResetEvent Event { get; private set; }
            public Server Server { get; private set; }
            public ulong LimiterOrder { get; private set; }

            public Limit(Priority a_priority, Server a_server, ulong a_limiter_order)
            {
                Event = new AutoResetEvent(false);
                Priority = a_priority;
                Server = a_server;
                LimiterOrder = a_limiter_order;
            }

            public override string ToString()
            {
                return String.Format("Server: {0}, Priority: {1}, LimitOrder: {2}", Server.ID, Priority, LimiterOrder);
            }

            public void Dispose()
            {
                Event.Dispose();
            }
        }
        #endregion

        private static List<Limit> s_limits = new List<Limit>();
        private static AutoResetEvent s_loop_event = new AutoResetEvent(true);

        private const int LOOP_SLEEP_MS = 500;
        private const int WAIT_SLEEP_MS = 500;

        private static Dictionary<Server, int> s_server_connections = new Dictionary<Server, int>();
        private static Dictionary<Server, bool> s_one_chapter_per_server = new Dictionary<Server, bool>();
        private static int s_connections = 0;

        static Limiter()
        {
            foreach (var server in DownloadManager.Instance.Servers)
            {
                s_server_connections[server] = 0;
                s_one_chapter_per_server[server] = false;
            }

            Thread loop_thread = new Thread(Loop);
            loop_thread.Name = "Limiter";
            loop_thread.IsBackground = true;
            loop_thread.Start();

        }

        public static void BeginChapter(Chapter a_chapter)
        {
            Aquire(a_chapter.Server, a_chapter.Token, Priority.Pages, a_chapter.LimiterOrder);
        }

        public static void Aquire(Server a_server)
        {
            Aquire(a_server, CancellationToken.None, Priority.Series, a_server.LimiterOrder);
        }

        public static void Aquire(Serie a_serie)
        {
            Aquire(a_serie.Server, CancellationToken.None, Priority.Chapters, a_serie.LimiterOrder);
        }

        public static void Aquire(Chapter a_chapter)
        {
            Aquire(a_chapter.Server, a_chapter.Token, Priority.Image, a_chapter.LimiterOrder);
        }

        public static void Aquire(Page a_page)
        {
            Aquire(a_page.Server, a_page.Chapter.Token, Priority.Image, a_page.LimiterOrder);
        }

        private static void Aquire(Server a_server, CancellationToken a_token, Priority a_priority, ulong a_limiter_order)
        {
            using (Limit limit = new Limit(a_priority, a_server, a_limiter_order))
            {
                lock (s_limits)
                {
                    s_limits.Add(limit);
                }
                s_loop_event.Set();

                while (!limit.Event.WaitOne(WAIT_SLEEP_MS))
                {
                    lock (s_limits)
                    {
                        if (a_token.IsCancellationRequested)
                        {
                            s_limits.Remove(limit);
                            a_token.ThrowIfCancellationRequested();
                        }
                    }
                }
            }
        }

        private static void Loop()
        {
            for (; ; )
            {
                s_loop_event.WaitOne(LOOP_SLEEP_MS);

                lock (s_limits)
                {
                    for (; ; )
                    {
                        Limit limit = GetLimit();

                        if (limit != null)
                        {
                            if (limit.Priority == Priority.Pages)
                            {
                                Debug.Assert(!s_one_chapter_per_server[limit.Server]);
                                s_one_chapter_per_server[limit.Server] = true;
                            }
                            else
                            {
                                if (limit.Priority == Priority.Image)
                                    Debug.Assert(s_one_chapter_per_server[limit.Server]);
                                
                                s_connections++;
                                s_server_connections[limit.Server]++;

                                Debug.Assert(s_connections <=
                                    DownloadManager.Instance.MangaSettings.MaximumConnections);
                                Debug.Assert(s_server_connections[limit.Server] <=
                                    limit.Server.Crawler.MaxConnectionsPerServer);
                            }

                            limit.Event.Set();

                            continue;
                        }
                        else
                            break;
                    }
                }
            }
        }

        public static void EndChapter(Chapter a_chapter)
        {
            lock (s_limits)
            {
                Debug.Assert(s_one_chapter_per_server[a_chapter.Server]);
                s_one_chapter_per_server[a_chapter.Server] = false;
            }

            s_loop_event.Set();
        }

        public static void Release(Serie a_serie)
        {
            Release(a_serie.Server);
        }

        public static void Release(Page a_page)
        {
            Release(a_page.Server);
        }

        public static void Release(Chapter a_chapter)
        {
            Release(a_chapter.Server);
        }

        public static void Release(Server a_server)   
        {
            lock (s_limits)
            {
                s_connections--;
                s_server_connections[a_server]--;

                Debug.Assert(s_connections >= 0);
                Debug.Assert(s_server_connections[a_server] >= 0);
            }

            s_loop_event.Set();
        }

        private static Limit GetLimit()
        {
            Limit candidate = null;

            var candidates1 = from limit in s_limits
                              where limit.Priority != Priority.Pages
                              where s_server_connections[limit.Server] <
                                 limit.Server.Crawler.MaxConnectionsPerServer
                              orderby limit.Priority, limit.LimiterOrder
                              select limit;

            var candidates2 = from limit in s_limits
                              where limit.Priority == Priority.Pages
                              where !s_one_chapter_per_server[limit.Server]
                              orderby limit.LimiterOrder
                              select limit;

            if (s_connections < DownloadManager.Instance.MangaSettings.MaximumConnections)
                candidate = candidates1.FirstOrDefault();

            if (candidate == null)
                candidate = candidates2.FirstOrDefault();

            if (candidate != null)
                s_limits.Remove(candidate);

            return candidate;
        }
    }
}
