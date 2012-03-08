using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    internal static class Works
    {
        #region Work
        private class Work
        {
            public Priority Priority { get; private set; }
            public Action Action { get; private set; }
            public Server Server { get; private set; }

            public Work(Action a_action, Priority a_priority, Server a_server)
            {
                Action = a_action;
                Priority = a_priority;
                Server = a_server;
            }
        }
        #endregion

        private static List<Work> s_works = new List<Work>();
        private static AutoResetEvent s_event = new AutoResetEvent(true);

        private const int THREAD_SLEEP_MS = 500;
        public const int MAX_CONNECTIONS = 100;
        public const int MAX_CONNECTIONS_PER_SERVER = 4;

        private static Dictionary<int, int> s_server_connections = new Dictionary<int,int>();
        private static Dictionary<int, bool> s_one_chapter_per_server = new Dictionary<int,bool>();
        private static int s_connections;

        static Works()
        {
            new Thread(Loop).Start();
        }

        public static void AddWork(Action a_action, Priority a_priority, Server a_server)
        {
            lock (s_works)
            {
                s_works.Add(new Work(a_action, a_priority, a_server));
                s_event.Set();
            }
        }

        private static void Loop()
        {
            for (; ; )
            {
                bool signaled = s_event.WaitOne(THREAD_SLEEP_MS);

                lock (s_works)
                {
                    for (; ; )
                    {
                        Work work = GetWork();

                        if (work != null)
                        {
                            Debug.Assert(signaled);
                            FireWork(work);
                            continue;
                        }
                        else
                            break;
                    }
                }
            }
        }

        private static void FireWork(Work a_work)
        {
            if (a_work.Priority == Priority.Pages)
                s_one_chapter_per_server[a_work.Server.ID] = true;
            s_connections++;
            s_server_connections[a_work.Server.ID]++;

            new Thread(() =>
            {
                try
                {
                    a_work.Action();
                }
                finally
                {
                    lock (s_works)
                    {
                        if (a_work.Priority == Priority.Pages)
                            s_one_chapter_per_server[a_work.Server.ID] = false;
                        s_connections--;
                        s_server_connections[a_work.Server.ID]--;
                    }
                }
            }).Start();
        }

        private static Work GetWork()
        {
            if (s_connections == MAX_CONNECTIONS)
                return null;

            Work candidate = null;

            foreach (var work in s_works)
            {
                if (s_server_connections[work.Server.ID] == MAX_CONNECTIONS_PER_SERVER)
                    continue;

                if (work.Priority == Priority.Pages)
                {
                    if (s_one_chapter_per_server[work.Server.ID])
                        continue;
                }

                if (candidate != null)
                {
                    if (candidate.Priority < work.Priority)
                        candidate = work;
                }
                else
                    candidate = work;
            }

            return candidate;
        }
    }
}
