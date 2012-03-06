using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Ionic.Zip;
using System.Resources;
using System.Diagnostics;
using System.Collections.Concurrent;
using HtmlAgilityPack;
using TomanuExtensions;
using System.Collections.ObjectModel;
using NHibernate.Linq;
using NHibernate;

namespace MangaCrawlerLib
{
    public static class DownloadManager
    {
        internal static string UserAgent = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:10.0) Gecko/20100101 Firefox/10.0";

        public static Func<string> GetMangaRootDir;
        public static Func<bool> UseCBZ;

        private static Server[] s_servers;
        private static List<Chapter> s_works = new List<Chapter>();

        static DownloadManager()
        {
            HtmlWeb.UserAgent_Actual = UserAgent;
        }

        public static void DownloadSeries(Server a_server)
        {
            if (a_server == null)
                return;

            bool download_required = NH.TransactionLockUpdateWithResult(a_server, () => 
            {
                if (!a_server.DownloadRequired)
                    return false;
                a_server.SetState(ServerState.Waiting);
                return true;
            });

            if (!download_required)
            {
                Loggers.MangaCrawler.InfoFormat(
                    "Already in work, server: {0} state: {1}",
                    a_server, a_server.State);
                return;
            }

            Task task = new Task(() =>
            {
                a_server.DownloadSeries();

            }, TaskCreationOptions.LongRunning);

            task.Start(a_server.Scheduler[Priority.Series]);
        }

        public static void DownloadChapters(Serie a_serie)
        {
            if (a_serie == null)
                return;

            bool download_required = NH.TransactionLockUpdateWithResult(a_serie, () =>
            {
                if (!a_serie.DownloadRequired)
                    return false;
                a_serie.SetState(SerieState.Waiting);
                return true;
            });

            if (!download_required)
            {
                Loggers.MangaCrawler.InfoFormat(
                    "Already in work, serie: {0} state: {1}",
                    a_serie, a_serie.State);
                return;
            }

            Task task = new Task(() =>
            {
                a_serie.DownloadChapters();
            }, TaskCreationOptions.LongRunning);

            task.Start(a_serie.Scheduler[Priority.Chapters]);
        }

        public static void DownloadPages(IEnumerable<Chapter> a_chapters)
        {
            foreach (var chapter in a_chapters)
            {
                lock (s_works)
                {
                    if (s_works.Contains(chapter))
                    {
                        Loggers.MangaCrawler.InfoFormat(
                            "Already in work, chapter: {0} state: {1}",
                            chapter, chapter.State);
                        continue;
                    }

                    s_works.Add(chapter);
                }

                Chapter chapter_sync = chapter;

                bool download_required = NH.TransactionLockUpdateWithResult(chapter_sync, () =>
                {
                    if (chapter_sync.IsWorking)
                        return false;
                    chapter_sync.SetState(ChapterState.Waiting);

                    return true;
                });

                if (!download_required)
                {
                    Loggers.MangaCrawler.InfoFormat(
                        "Already in work, chapter: {0} state: {1}",
                        chapter_sync, chapter_sync.State);
                    continue;
                }

                Loggers.MangaCrawler.InfoFormat(
                    "Chapter: {0} state: {1}",
                    chapter_sync, chapter_sync.State);

                Task task = new Task(() =>
                {
                    chapter_sync.DownloadPages(GetMangaRootDir(), UseCBZ());
                }, TaskCreationOptions.LongRunning);

                task.Start(chapter_sync.Scheduler[Priority.Pages]);
            }
        }

        public static IEnumerable<Chapter> Works
        {
            get
            {
                lock (s_works)
                {
                    return s_works.ToArray();
                }
            }
        }

        internal static void Sync<T, K>(IEnumerable<T> a_transient, IList<T> a_persisted,
            Func<T, K> a_key_selector, bool a_remove, out bool a_added, out IList<T> a_removed) where K : IEquatable<K>
        {
            IDictionary<K, T> new_pages_dict = a_transient.ToDictionary<T, K>(a_key_selector);
            IDictionary<K, T> pages_dict = a_persisted.ToDictionary(a_key_selector);

            a_removed = new List<T>();

            if (a_remove)
            {
                List<K> to_remove = new List<K>();

                foreach (var key in pages_dict.Keys)
                {
                    if (!new_pages_dict.Keys.Contains(key))
                        to_remove.Add(key);
                }

                a_removed = (from key in to_remove
                            select pages_dict[key]).ToList();
                a_persisted.RemoveRange(a_removed);
                pages_dict.RemoveRange(to_remove);
            }

            a_added = false;

            int index = 0;
            foreach (var tr in a_transient)
            {
                if (a_persisted.Count <= index)
                {
                    a_persisted.Insert(index, tr);
                    a_added = true;
                }
                else
                {
                    var pr = a_persisted[index];

                    if (!a_key_selector(pr).Equals(a_key_selector(tr)))
                    {
                        a_persisted.Insert(index, tr);
                        a_added = true;
                    }
                }
                index++;
            }
        }

        public static IEnumerable<Server> Servers
        {
            get
            {
                if (s_servers == null)
                    s_servers = NH.TransactionWithResult(session => session.Query<Server>().ToArray());

                return s_servers;
            }
        }
    }
}
