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
        private static Server s_selected_server;
        private static Dictionary<Server, Serie> s_selected_series =
            new Dictionary<Server, Serie>();
        private static Dictionary<Server, VisualState> s_series_visual_states = 
            new Dictionary<Server, VisualState>();
        private static Dictionary<Serie, Chapter> s_selected_chapters =
            new Dictionary<Serie, Chapter>();
        private static Dictionary<Serie, VisualState> s_chapters_visual_states =
            new Dictionary<Serie, VisualState>();
        private static List<Work> s_works = new List<Work>();

        private static string s_settings_dir;

        public static Func<string> GetMangaRootDir;
        public static Func<bool> UseCBZ;

        private static Object s_server_lock = new Object();
        private static Object s_serie_lock = new Object();
        private static Object s_chapter_lock = new Object();

        public static Func<VisualState> GetSeriesVisualState;
        public static Func<VisualState > GetChaptersVisualState;

        static DownloadManager()
        {
            HtmlWeb.UserAgent_Actual = UserAgent;

            s_servers = ServerList.Servers.ToList();
        }

        public static VisualState SeriesVisualState
        {
            get
            {
                if (SelectedServer == null)
                    return GetSeriesVisualState();
                if (!s_series_visual_states.ContainsKey(SelectedServer))
                    return GetSeriesVisualState();

                return s_series_visual_states[SelectedServer];
            }
            set
            {
                if (SelectedServer != null)
                    s_series_visual_states[SelectedServer] = value;
            }
        }

        public static VisualState ChaptersVisualState
        {
            get
            {
                if (SelectedSerie == null)
                    return GetChaptersVisualState();
                if (!s_chapters_visual_states.ContainsKey(SelectedSerie))
                    return GetChaptersVisualState();

                return s_chapters_visual_states[SelectedSerie];
            }
            set
            {
                if (SelectedSerie != null)
                    s_chapters_visual_states[SelectedSerie] = value;
            }
        }

        public static Serie SelectedSerie
        {
            get
            {
                if (SelectedServer == null)
                    return null;

                if (!s_selected_series.ContainsKey(SelectedServer))
                    return null;

                return s_selected_series[SelectedServer];
            }
            set
            {
                SeriesVisualState = GetSeriesVisualState();
                s_selected_series[SelectedServer] = value;

                DownloadChapters(SelectedSerie);
            }
        }

        public static Server SelectedServer
        {
            get
            {
                return s_selected_server;
            }
            set
            {
                s_selected_server = value;
                DownloadSeries(s_selected_server);
            }
        }

        public static Chapter SelectedChapter
        {
            get
            {
                if (SelectedSerie == null)
                    return null;

                if (!s_selected_chapters.ContainsKey(SelectedSerie))
                    return null;

                return s_selected_chapters[SelectedSerie];
            }
            set
            {
                ChaptersVisualState = GetChaptersVisualState();

                if (SelectedSerie != null)
                    s_selected_chapters[SelectedSerie] = value;
            }
        }

        private static void DownloadSeries(Server a_server)
        {
            if (a_server == null)
                return;

            lock (s_server_lock)
            {
                if (!a_server.DownloadRequired)
                    return;
                a_server.State = ServerState.Downloading;
            }

            Task task = new Task(() =>
            {
                a_server.DownloadSeries();
            }, TaskCreationOptions.LongRunning);

            task.Start(a_server.Scheduler[Priority.Series]);
        }

        private static void DownloadChapters(Serie a_serie)
        {
            if (a_serie == null)
                return;

            lock (s_serie_lock)
            {
                if (!a_serie.DownloadRequired)
                    return;
                a_serie.State = SerieState.Downloading;
            }

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
                Work work = null;

                lock (s_chapter_lock)
                {
                    work = chapter.FindWork();

                    if (work != null)
                    {
                        if (work.IsWorking)
                        {
                            Loggers.MangaCrawler.InfoFormat(
                                "Already in work, work: {0} state: {1}",
                                work, work.State);
                            continue;
                        }
                    }
                    else
                    {
                        work = new Work(chapter, GetMangaRootDir(), UseCBZ());
                    }

                    chapter.Work = work;

                    Loggers.MangaCrawler.InfoFormat(
                        "Work: {0} state: {1}",
                        work, work.State);

                }

                lock (s_works)
                {
                    s_works.Add(work);
                }

                Task task = new Task(() =>
                {
                    work.DownloadPages();
                }, TaskCreationOptions.LongRunning);

                task.Start(work.Chapter.Serie.Server.Scheduler[Priority.Pages]);
            }
        }

        public static IEnumerable<Server> Servers
        {
            get
            {
                return s_servers;
            }
        }

        public static IEnumerable<Work> Works
        {
            get
            {
                lock (s_works)
                {
                    return s_works.ToArray();
                }
            }
        }

        public static void Load(string a_settings_dir)
        {
            s_settings_dir = a_settings_dir;
        }
    }
}
