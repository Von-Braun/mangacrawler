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
using MangaCrawlerLib.Crawlers;

namespace MangaCrawlerLib
{
    public static class DownloadManager
    {
        internal static string UserAgent = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:10.0) Gecko/20100101 Firefox/10.0";

        public static Func<string> GetMangaRootDir;
        public static Func<string> GetSettingsDir;
        public static Func<TimeSpan> GetCheckTimeDelta;
        public static Func<bool> UseCBZ;
        public static Func<PageNamingStrategy> PageNamingStrategy;
        public static Action UpdateGUI;

        private static List<Entity> s_downloading = new List<Entity>();
        private static Server[] s_servers;
        private static ConcurrentBag<Chapter> s_works = new ConcurrentBag<Chapter>();

        static DownloadManager()
        {
            HtmlWeb.UserAgent_Actual = UserAgent;
        }

        public static bool IsDownloading()
        {
            bool result = s_downloading.Any();

            s_downloading = (from entity in s_downloading
                             where entity.IsWorking
                             select entity).ToList();
            return result;
        }

        public static void DownloadSeries(Server a_server)
        {
            if (a_server == null)
                return;

            if (!a_server.DownloadRequired)
                return;

            s_downloading.Add(a_server);
            a_server.State = ServerState.Waiting;
            a_server.LimiterOrder = Catalog.NextID();
            UpdateGUI();

            new Task(() =>
            {
                a_server.DownloadSeries();

            }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
        }

        public static void DownloadChapters(Serie a_serie)
        {
            if (a_serie == null)
                return;

            if (!a_serie.DownloadRequired)
                return;

            s_downloading.Add(a_serie);
            a_serie.State = SerieState.Waiting;
            a_serie.LimiterOrder = Catalog.NextID();
            UpdateGUI();

            new Task(() =>
            {
                a_serie.DownloadChapters();
            }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
        }

        public static void DownloadPages(IEnumerable<Chapter> a_chapters)
        {
            foreach (var chapter in a_chapters)
            {
                if (chapter.IsWorking)
                    continue;

                lock (s_works)
                {
                    if (!s_works.Contains(chapter))
                        s_works.Add(chapter);
                }

                s_downloading.Add(chapter);
                chapter.State = ChapterState.Waiting;
                chapter.LimiterOrder = Catalog.NextID();
                UpdateGUI();

                Chapter chapter_sync = chapter;

                new Task(() =>
                {
                    chapter_sync.DownloadPages();
                }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
            }
        }

        public static IEnumerable<Chapter> Works
        {
            get
            {
                return s_works;
            }
        }

        public static void Save()
        {
            Catalog.SaveCatalog();
        }

        public static IEnumerable<Server> Servers
        {
            get
            {
                return s_servers;
            }
        }

        public static void Initialize()
        {
            s_servers = Catalog.LoadCatalog();

            IEnumerable<Chapter> works = from work in Catalog.LoadWorks(Servers)
                                         where work.State != ChapterState.Downloaded
                                         where work.State != ChapterState.Aborted
                                         orderby work.LimiterOrder
                                         select work;

            DownloadPages(works);
        }

        public static void SaveWorks()
        {
            Catalog.SaveWorks(s_works);
        }
    }
}
