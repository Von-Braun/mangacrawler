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

        private static ServerInfo[] s_servers;
        private static ServerInfo s_selected_server_info;
        private static Dictionary<ServerInfo, SerieInfo> s_selected_series =
            new Dictionary<ServerInfo, SerieInfo>();
        private static Dictionary<ServerInfo, VisualState> s_series_visual_states = 
            new Dictionary<ServerInfo, VisualState>();
        private static Dictionary<SerieInfo, ChapterInfo> s_selected_chapters =
            new Dictionary<SerieInfo, ChapterInfo>();
        private static Dictionary<SerieInfo, VisualState> s_chapters_visual_states =
            new Dictionary<SerieInfo, VisualState>();
        private static List<TaskInfo> s_tasks = new List<TaskInfo>();

        private static string s_settings_dir;
        internal static DownloadedChapters Downloaded;
        internal static DownloadingTasks Downloading;

        public static Func<string> GetImagesBaseDir;
        public static Func<bool> UseCBZ;

        private static Object s_server_lock = new Object();
        private static Object s_serie_lock = new Object();
        private static Object s_chapter_lock = new Object();

        public static Func<VisualState> GetSeriesVisualState;
        public static Func<VisualState > GetChaptersVisualState;

        static DownloadManager()
        {
            HtmlWeb.UserAgent_Actual = UserAgent;

            s_servers = ServerInfo.ServersInfos.ToArray();
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

        public static SerieInfo SelectedSerie
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

        public static ServerInfo SelectedServer
        {
            get
            {
                return s_selected_server_info;
            }
            set
            {
                s_selected_server_info = value;
                DownloadSeries(s_selected_server_info);
            }
        }

        public static ChapterInfo SelectedChapter
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

        private static void DownloadSeries(ServerInfo a_server_info)
        {
            if (a_server_info == null)
                return;

            lock (s_server_lock)
            {
                if (!a_server_info.DownloadRequired)
                    return;
                a_server_info.State = ServerState.Downloading;
            }

            Task task = new Task(() =>
            {
                a_server_info.DownloadSeries();
            }, TaskCreationOptions.LongRunning);

            task.Start(a_server_info.Scheduler[Priority.Series]);
        }

        private static void DownloadChapters(SerieInfo a_serie_info)
        {
            if (a_serie_info == null)
                return;

            lock (s_serie_lock)
            {
                if (!a_serie_info.DownloadRequired)
                    return;
                a_serie_info.State = SerieState.Downloading;
            }

            Task task = new Task(() =>
            {
                a_serie_info.DownloadChapters();
            }, TaskCreationOptions.LongRunning);

            task.Start(a_serie_info.Server.Scheduler[Priority.Chapters]);
        }

        public static void DownloadPages(IEnumerable<ChapterInfo> a_chapter_infos)
        {
            string baseDir = GetImagesBaseDir();

            foreach (var chapter_info in a_chapter_infos)
            {
                TaskInfo task_info = null;

                lock (s_chapter_lock)
                {
                    task_info = chapter_info.FindTask();

                    if (task_info != null)
                    {
                        Loggers.MangaCrawler.InfoFormat(
                            "Already in work, task: {0} state: {1}",
                            task_info, task_info.State);
                        continue;
                    }

                    task_info = new TaskInfo(chapter_info, GetImagesBaseDir(), UseCBZ());
                    chapter_info.Task = task_info;

                    Loggers.MangaCrawler.InfoFormat(
                        "Task: {0} state: {1}",
                        task_info, task_info.State);

                }

                StartTask(task_info);
            }
        }

        public static void StartTask(TaskInfo a_task_info)
        {
            lock (s_tasks)
            {
                s_tasks.Add(a_task_info);
            }

            Task task = new Task(() =>
            {
                a_task_info.DownloadPages();
            }, TaskCreationOptions.LongRunning);

            task.Start(a_task_info.Server.Scheduler[Priority.Pages]);
        }

        public static IEnumerable<ServerInfo> Servers
        {
            get
            {
                return s_servers;
            }
        }

        public static IEnumerable<TaskInfo> Tasks
        {
            get
            {
                lock (s_tasks)
                {
                    return s_tasks.ToArray();
                }
            }
        }

        public static void Load(string a_settings_dir)
        {
            s_settings_dir = a_settings_dir;
            Downloaded = new DownloadedChapters(a_settings_dir);
            Downloading = DownloadingTasks.Load(a_settings_dir);
            Downloading.Restore();
        }

        public static void Save()
        {
            Downloading.Save();
        }
    }
}
