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
        private static List<ChapterInfo> s_tasks = new List<ChapterInfo>();

        public static Form Form;

        public static Func<string> GetDirectoryPath;
        public static Func<bool> UseCBZ;

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
            if (!a_server_info.DownloadRequired)
                return;

            Task task = new Task(() =>
            {
                a_server_info.DownloadSeries();
            });

            task.Start(a_server_info.Scheduler[Priority.Series]);
        }

        private static void DownloadChapters(SerieInfo a_serie_info)
        {
            if (a_serie_info == null)
                return;
            if (!a_serie_info.DownloadRequired)
                return;

            Task task = new Task(() =>
            {
                a_serie_info.DownloadChapters();
            });

            task.Start(a_serie_info.ServerInfo.Scheduler[Priority.Chapters]);
        }

        public static void DownloadPages(IEnumerable<ChapterInfo> a_chapter_infos)
        {
            string baseDir = GetDirectoryPath();

            bool cbz = UseCBZ();

            foreach (var chapter_info in a_chapter_infos)
            {
                if (chapter_info.Working)
                {
                    System.Diagnostics.Debug.WriteLine("DownloadManager.DownloadPages - already in work, title: {0} state: {1}",
                        chapter_info.Title, chapter_info.State);
                    continue;
                }

                System.Diagnostics.Debug.WriteLine(
                    "DownloadManager.DownloadPages - title: {0} state: {1}",
                        chapter_info.Title, chapter_info.State);

                chapter_info.InitializeDownload();
                chapter_info.State = ItemState.Waiting;

                lock (s_tasks)
                {
                    Debug.Assert(!s_tasks.Contains(chapter_info));
                    s_tasks.Add(chapter_info);
                }

                Task task = new Task(() =>
                {
                    try
                    {
                        ConnectionsLimiter.BeginDownloadPages(chapter_info);
                    }
                    catch (OperationCanceledException)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "DownloadManager.DownloadPages - #1 operation cancelled, title: {0} state: {1}",
                            chapter_info.Title, chapter_info.State);

                        chapter_info.FinishDownload(true);
                        return;
                    }

                    try
                    {
                        string dir = chapter_info.GetImageDirectory(baseDir);

                        new DirectoryInfo(dir).DeleteAll();

                        if (chapter_info.Token.IsCancellationRequested)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                "DownloadManager.DownloadPages - #1 cancellation requested, title: {0} state: {1}",
                                chapter_info.Title, chapter_info.State);
                        }

                        chapter_info.Token.ThrowIfCancellationRequested();

                        chapter_info.DownloadPages();

                        if (chapter_info.Token.IsCancellationRequested)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                "DownloadManager.DownloadPages - #2 cancellation requested, title: {0} state: {1}",
                                chapter_info.Title, chapter_info.State);
                        }

                        chapter_info.Token.ThrowIfCancellationRequested();

                        Parallel.ForEach(chapter_info.Pages,
                            new ParallelOptions()
                            {
                                MaxDegreeOfParallelism = chapter_info.SerieInfo.ServerInfo.Crawler.MaxConnectionsPerServer,
                                TaskScheduler = chapter_info.SerieInfo.ServerInfo.Scheduler[Priority.Pages], 
                            },
                            (page, state) =>
                        {
                            try
                            {
                                page.DownloadAndSavePageImage(dir);
                            }
                            catch (OperationCanceledException ex1)
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    "DownloadManager.DownloadPages - OperationCanceledException, title: {0} state: {1}, {2}",
                                    chapter_info.Title, chapter_info.State, ex1);

                                state.Break();
                            }
                            catch (Exception ex2)
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    "DownloadManager.DownloadPages - #1 Exception, title: {0} state: {1}, {2}",
                                    chapter_info.Title, chapter_info.State, ex2);

                                state.Break();
                                throw;
                            }
                        });

                        if (chapter_info.Token.IsCancellationRequested)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                "DownloadManager.DownloadPages - #3 cancellation requested, title: {0} state: {1}",
                                chapter_info.Title, chapter_info.State);
                        }

                        chapter_info.Token.ThrowIfCancellationRequested();
                        
                        if (cbz)
                            CreateCBZ(chapter_info);
                        

                        chapter_info.FinishDownload(a_error: false);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                                    "DownloadManager.DownloadPages - #2 Exception, title: {0} state: {1}, {2}",
                                    chapter_info.Title, chapter_info.State, ex);

                        chapter_info.FinishDownload(a_error: true);
                    }
                    finally
                    {
                        ConnectionsLimiter.EndDownloadPages(chapter_info);
                    }
                });

                task.Start(chapter_info.SerieInfo.ServerInfo.Scheduler[Priority.Pages]);
            }
        }

        private static void CreateCBZ(ChapterInfo a_chapter_info)
        {
            System.Diagnostics.Debug.WriteLine(
                "DownloadManager.CreateCBZ - title: {0} state: {1}",
                a_chapter_info.Title, a_chapter_info.State);

            a_chapter_info.State = ItemState.Zipping;

            var dir = new DirectoryInfo(a_chapter_info.Pages.First().GetImageFilePath()).Parent;

            var zip_file = dir.FullName + ".cbz";

            int counter = 1;
            while (new FileInfo(zip_file).Exists)
            {
                zip_file = String.Format("{0} ({1}).cbz", dir.FullName, counter);
                counter++;
            }

            using (ZipFile zip = new ZipFile())
            {
                zip.UseUnicodeAsNecessary = true;

                foreach (var page in a_chapter_info.Pages)
                {
                    zip.AddFile(page.GetImageFilePath(), "");
                    a_chapter_info.Token.ThrowIfCancellationRequested();
                }

                zip.Save(zip_file);
            }

            try
            {
                foreach (var page in a_chapter_info.Pages)
                    new FileInfo(page.GetImageFilePath()).Delete();

                if ((dir.GetFiles().Count() == 0) && (dir.GetDirectories().Count() == 0))
                    dir.Delete();
            }
            catch
            {
            }

        }

        public static IEnumerable<ServerInfo> Servers
        {
            get
            {
                return s_servers;
            }
        }

        public static IEnumerable<ChapterInfo> GetTasks()
        {
            lock (s_tasks)
            {
                var toremove = (from ch in s_tasks
                                where !ch.IsTask
                                select ch).ToArray();

                foreach (var task in toremove)
                {
                    System.Diagnostics.Debug.WriteLine("DownloadManager.GetTasks - removing {0} task, state: {1}", 
                        task.Title, task.State);
                    s_tasks.Remove(task);
                }

                return s_tasks.ToArray();
            }
        }
    }
}
