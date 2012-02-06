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
            HtmlWeb.UserAgent_Actual = HTTPUtils.UserAgent;

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
                    continue;

                chapter_info.InitializeDownload();
                chapter_info.State = ItemState.Waiting;

                Debug.Assert(!s_tasks.Contains(chapter_info));
                lock (s_tasks)
                {
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
                        chapter_info.FinishDownload(true);
                        return;
                    }

                    try
                    {
                        string dir = chapter_info.GetImageDirectory(baseDir);

                        new DirectoryInfo(dir).DeleteAll();

                        chapter_info.Token.ThrowIfCancellationRequested();

                        chapter_info.State = ItemState.Downloading;

                        chapter_info.DownloadPages();

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
                                page.DownloadAndSavePageImage(chapter_info.Token, dir);
                            }
                            catch (OperationCanceledException)
                            {
                                state.Break();
                            }
                            catch
                            {
                                state.Break();
                                throw;
                            }
                        });

                        chapter_info.Token.ThrowIfCancellationRequested();
                        
                        if (cbz)
                            CreateCBZ(chapter_info);
                        

                        chapter_info.FinishDownload(a_error: false);
                    }
                    catch
                    {
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

        private static void TryInvoke(Action a_action)
        {
            try
            {
                Form.BeginInvoke(a_action);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private static T TryInvoke<T>(Func<T> a_action)
        {
            try
            {
                return (T)Form.Invoke(a_action);
            }
            catch (ObjectDisposedException)
            {
                return default(T);
            }
        }

        public static IEnumerable<ServerInfo> Servers
        {
            get
            {
                return s_servers;
            }
        }

        public static IEnumerable<ChapterInfo> Tasks
        {
            get
            {
                lock (s_tasks)
                {
                    s_tasks.RemoveRange(from ch in s_tasks
                                        where !ch.IsTask
                                        select ch);
                }

                return s_tasks;
            }
        }

        public static bool DownloadingPages
        {
            get
            {
                lock (s_tasks)
                {
                    return s_tasks.Any(ch => ch.State == ItemState.Downloading);
                }
            }
        }
    }
}
