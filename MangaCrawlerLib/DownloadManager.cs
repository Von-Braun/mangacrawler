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

namespace MangaCrawlerLib
{
    public static class DownloadManager
    {
        internal static string UserAgent = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:10.0) Gecko/20100101 Firefox/10.0";

        public static Func<string> GetMangaRootDir;
        public static Func<bool> UseCBZ;

        static DownloadManager()
        {
            HtmlWeb.UserAgent_Actual = UserAgent;
        }

        public static void DownloadSeries(Server a_server)
        {
            if (a_server == null)
                return;

            bool already_downloading = NH.TransactionWithResult(session => 
            {
                a_server = session.Load<Server>(a_server.ID);

                if (!a_server.BeginWaiting())
                    return true;

                session.Update(a_server);

                return false;
            });

            if (already_downloading)
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

            bool already_downloading = NH.TransactionWithResult(session =>
            {
                a_serie = session.Load<Serie>(a_serie.ID);

                if (!a_serie.BeginWaiting())
                    return true;

                session.Update(a_serie);

                return false;
            });

            if (already_downloading)
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
                Chapter chapter_sync = null;

                bool already_downloading = NH.TransactionWithResult(session =>
                {
                    chapter_sync = session.Load<Chapter>(chapter.ID);

                    if (!chapter_sync.BeginWaiting())
                        return true;

                    session.Update(chapter_sync);

                    return false;
                });

                if (already_downloading)
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

        public static Chapter[] Works
        {
            get
            {
                return NH.TransactionWithResult(session =>
                {
                    return (from server in session.Query<Server>().ToList()
                            from serie in server.GetSeries()
                            from chapter in serie.GetChapters()
                            where chapter.State != ChapterState.Initial
                            select chapter).ToArray();
                });
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
    }
}
