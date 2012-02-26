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

namespace MangaCrawlerLib
{
    public static class DownloadManager
    {
        internal static string UserAgent = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:10.0) Gecko/20100101 Firefox/10.0";

        private static List<Server> s_servers;
        private static List<Chapter> s_works = new List<Chapter>();

        private static string s_settings_dir;

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

            if (!a_server.DownloadRequired)
                return;
            a_server.State = ServerState.Downloading;

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

            if (!a_serie.DownloadRequired)
                return;
            a_serie.State = SerieState.Downloading;

            Task task = new Task(() =>
            {
                a_serie.DownloadChapters();
            }, TaskCreationOptions.LongRunning);

            task.Start(a_serie.Server.Scheduler[Priority.Chapters]);
        }

        public static void DownloadPages(IEnumerable<Chapter> a_chapters)
        {
            foreach (var chapter in a_chapters)
            {
                if (chapter.IsWorking)
                {
                    Loggers.MangaCrawler.InfoFormat(
                        "Already in work, chapter: {0} state: {1}",
                        chapter, chapter.State);
                    continue;
                }

                chapter.State = ChapterState.Downloading;
                
                Loggers.MangaCrawler.InfoFormat(
                    "Chapter: {0} state: {1}",
                    chapter, chapter.State);

                lock (s_works)
                {
                    s_works.Add(chapter);
                }

                Task task = new Task(() =>
                {
                    chapter.DownloadPages(GetMangaRootDir(), UseCBZ());
                }, TaskCreationOptions.LongRunning);

                task.Start(chapter.Serie.Server.Scheduler[Priority.Pages]);
            }
        }

        public static IEnumerable<Chapter> Works
        {
            get
            {
                lock (s_works)
                {
                    return (from chapter in s_works
                            select chapter).ToArray();
                }
            }
        }

        public static IEnumerable<Server> Servers
        {
            get
            {
                return s_servers;
            }
        }

        public static void Load(string a_settings_dir)
        {
            s_settings_dir = a_settings_dir;

            s_servers = ServerList.Servers.ToList();

            NHibernateSetup.DatabaseDir = a_settings_dir;
            NHibernateSetup.Setup(true);
        }
    }
}
