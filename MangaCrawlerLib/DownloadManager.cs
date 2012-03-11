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

            if (a_server.DownloadRequired)
                a_server.State = ServerState.Waiting;
            else
            {
                Loggers.MangaCrawler.InfoFormat(
                    "Already in work, server: {0} state: {1}",
                    a_server, a_server.State);
                return;
            }

            new Task(() =>
            {
                a_server.DownloadSeries();

            }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
        }

        public static void DownloadChapters(Serie a_serie)
        {
            if (a_serie == null)
                return;

            if (a_serie.DownloadRequired)
                a_serie.State = SerieState.Waiting;
            else
            {
                Loggers.MangaCrawler.InfoFormat(
                    "Already in work, serie: {0} state: {1}",
                    a_serie, a_serie.State);
                return;
            }

            new Task(() =>
            {
                a_serie.DownloadChapters();
            }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
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

                if (!chapter_sync.IsWorking)
                    chapter_sync.State = ChapterState.Waiting;  
                else
                {
                    Loggers.MangaCrawler.InfoFormat(
                        "Already in work, chapter: {0} state: {1}",
                        chapter_sync, chapter_sync.State);
                    continue;
                }

                Loggers.MangaCrawler.InfoFormat(
                    "Chapter: {0} state: {1}",
                    chapter_sync, chapter_sync.State);

                new Task(() =>
                {
                    chapter_sync.DownloadPages(GetMangaRootDir(), UseCBZ());
                }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
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

        public static void Save()
        {
            Catalog.Save();
        }

        public static IEnumerable<Server> Servers
        {
            get
            {
                if (s_servers == null)
                    s_servers = Catalog.Load();

                return s_servers;
            }
        }
    }
}
