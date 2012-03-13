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
            Image = 1,
            Pages = 2,
            Chapters = 3,
            Series = 4,
        }
        #endregion

        #region Limit
        private class Limit
        {
            public Priority Priority { get; private set; }
            public AutoResetEvent Event { get; private set; }
            public Server Server { get; private set; }

            public Limit(Priority a_priority, Server a_server)
            {
                Event = new AutoResetEvent(false);
                Priority = a_priority;
                Server = a_server;
            }
        }
        #endregion

        private static List<Limit> s_limits = new List<Limit>();
        private static AutoResetEvent s_loop_event = new AutoResetEvent(true);

        private const int LOOP_SLEEP_MS = 500;
        private const int WAIT_SLEEP_MS = 500;

        //// Sync with MangaCrawler/app.config
        public const int MAX_CONNECTIONS = 100;

        public const int MAX_CONNECTIONS_PER_SERVER = 4;

        private static Dictionary<Server, int> s_server_connections = new Dictionary<Server, int>();
        private static Dictionary<Server, bool> s_one_chapter_per_server = new Dictionary<Server, bool>();
        private static int s_connections = 0;

        public static TaskScheduler Scheduler = TaskScheduler.Current;

        static Limiter()
        {
            foreach (var server in DownloadManager.Servers)
            {
                s_server_connections[server] = 0;
                s_one_chapter_per_server[server] = false;
            }

            Thread loop_thread = new Thread(Loop);
            loop_thread.IsBackground = true;
            loop_thread.Start();

        }

        public static void BeginChapter(Chapter a_chapter)
        {
            Aquire(a_chapter.Server, a_chapter.Token, Priority.Pages);
        }

        public static void Aquire(Server a_server)
        {
            Aquire(a_server, CancellationToken.None, Priority.Series);
        }

        public static void Aquire(Serie a_serie)
        {
            Aquire(a_serie.Server, CancellationToken.None, Priority.Chapters);
        }

        public static void Aquire(Chapter a_chapter)
        {
            Aquire(a_chapter.Server, a_chapter.Token, Priority.Image);
        }

        public static void Aquire(Page a_page)
        {
            Aquire(a_page.Server, a_page.Chapter.Token, Priority.Image);
        }

        private static void Aquire(Server a_server, CancellationToken a_token, Priority a_priority)
        {
            Limit limit = new Limit(a_priority, a_server);
            lock (s_limits)
            {
                s_limits.Add(limit);
            }
            s_loop_event.Set();

            while (!limit.Event.WaitOne(WAIT_SLEEP_MS))
            {
                if (a_token != CancellationToken.None)
                {
                    if (a_token.IsCancellationRequested)
                    {
                        Loggers.Cancellation.Info("Cancellation requested");
                        a_token.ThrowIfCancellationRequested();
                    }
                }
            }
        }

        private static void Log()
        {
            Debug.Write(s_connections.ToString() + ", ");
            foreach (var el in s_server_connections)
                Debug.Write(el.Value + ", ");
            Debug.WriteLine("");
        }

        // TODO: odswiezac jesli cos sie zmienilo

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

                                Debug.Assert(s_connections <= MAX_CONNECTIONS);
                                Debug.Assert(s_server_connections[limit.Server] <= MAX_CONNECTIONS_PER_SERVER);
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

            foreach (var limit in s_limits)
            {
                if (limit.Priority == Priority.Pages)
                {
                    if (!s_one_chapter_per_server[limit.Server])
                    {
                        candidate = limit;
                        break;
                    }
                }
                else
                {
                    if (s_connections == MAX_CONNECTIONS)
                        return null;

                    if (s_server_connections[limit.Server] == MAX_CONNECTIONS_PER_SERVER)
                        continue;

                    if (candidate != null)
                    {
                        if (candidate.Priority < limit.Priority)
                            candidate = limit;
                    }
                    else
                        candidate = limit;
                }
            }

            if (candidate != null)
                s_limits.Remove(candidate);

            return candidate;
        }
    }
}
