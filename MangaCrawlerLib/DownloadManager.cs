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

        public static Form Form;

        public static event Action<IEnumerable<ChapterInfo>> TasksChanged;

        public static Func<string> GetSeriesFilter;
        public static Func<string> GetDirectoryPath;
        public static Func<bool> UseCBZ;

        public static Func<VisualState> GetServersVisualState;
        public static Func<VisualState> GetSeriesVisualState;
        public static Func<VisualState > GetChaptersVisualState;

        private static RestrictedFrequencyAction SeriesUpdate = 
            new RestrictedFrequencyAction(250);
        private static RestrictedFrequencyAction ChaptersUpdate =
            new RestrictedFrequencyAction(250);
        private static RestrictedFrequencyAction TasksUpdate =
            new RestrictedFrequencyAction(250);
        private static RestrictedFrequencyAction ServersUpdate =
            new RestrictedFrequencyAction(250);

        static DownloadManager()
        {
            HtmlWeb.UserAgent_Actual = HTTPUtils.UserAgent;

            s_servers = ServerInfo.ServersInfos.ToArray();
        }

        public static VisualState SeriesVisualState
        {
            get
            {
                if (SelectedServerInfo == null)
                    return GetSeriesVisualState();
                if (!s_series_visual_states.ContainsKey(SelectedServerInfo))
                    return GetSeriesVisualState();

                return s_series_visual_states[SelectedServerInfo];
            }
            set
            {
                if (SelectedServerInfo != null)
                    s_series_visual_states[SelectedServerInfo] = value;
            }
        }

        public static IEnumerable<ChapterInfo> AllChapters
        {
            get
            {
                foreach (var server in s_servers)
                {
                    foreach (var serie in server.Series)
                    {
                        foreach (var chapter in serie.Chapters)
                        {
                            yield return chapter;
                        }
                    }
                }
            }
        }

        public static VisualState ChaptersVisualState
        {
            get
            {
                if (SelectedSerieInfo == null)
                    return GetChaptersVisualState();
                if (!s_chapters_visual_states.ContainsKey(SelectedSerieInfo))
                    return GetChaptersVisualState();

                return s_chapters_visual_states[SelectedSerieInfo];
            }
            set
            {
                if (SelectedSerieInfo != null)
                    s_chapters_visual_states[SelectedSerieInfo] = value;
            }
        }

        public static SerieInfo SelectedSerieInfo
        {
            get
            {
                if (SelectedServerInfo == null)
                    return null;

                if (!s_selected_series.ContainsKey(SelectedServerInfo))
                    return null;

                return s_selected_series[SelectedServerInfo];
            }
            set
            {
                SeriesVisualState = GetSeriesVisualState();
                s_selected_series[SelectedServerInfo] = value;

                if (!DownloadChapters(SelectedSerieInfo))
                    OnChaptersChanged(SelectedSerieInfo);
            }
        }

        public static ServerInfo SelectedServerInfo
        {
            get
            {
                return s_selected_server_info;
            }
            set
            {
                s_selected_server_info = value;

                if (!DownloadSeries(s_selected_server_info))
                    OnSeriesChanged(s_selected_server_info);
            }
        }

        public static ChapterInfo SelectedChapterInfo
        {
            get
            {
                if (SelectedSerieInfo == null)
                    return null;

                if (!s_selected_chapters.ContainsKey(SelectedSerieInfo))
                    return null;

                return s_selected_chapters[SelectedSerieInfo];
            }
            set
            {
                ChaptersVisualState = GetChaptersVisualState();

                if (SelectedSerieInfo != null)
                    s_selected_chapters[SelectedSerieInfo] = value;
            }
        }

        private static bool DownloadSeries(ServerInfo a_server_info)
        {
            if (a_server_info == null)
                return false;
            if (!a_server_info.State.DownloadRequired)
                return false;

            a_server_info.State.Initialize();
            a_server_info.State.State = ItemState.Downloading;

            Task task = new Task(() =>
            {
                try
                {
                    a_server_info.DownloadSeries((progress) =>
                    {
                        a_server_info.State.Progress = progress;

                        if (progress != 100)
                            OnServersChanged();
                    });

                    a_server_info.State.State = ItemState.Downloaded;
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                    a_server_info.State.State = ItemState.Error;
                }

                OnServersChanged();
            });

            task.Start(a_server_info.State.Scheduler[Priority.Series]);

            OnServersChanged();

            return true;
        }

        private static bool DownloadChapters(SerieInfo a_serie_info)
        {
            if (a_serie_info == null)
                return false;
            if (!a_serie_info.State.DownloadRequired)
                return false;

            a_serie_info.State.Initialize();
            a_serie_info.State.State = ItemState.Downloading;

            Task task = new Task(() =>
            {
                try
                {
                    a_serie_info.DownloadChapters((progress) =>
                    {
                        a_serie_info.State.Progress = progress;

                        if (progress != 100)
                            OnSeriesChanged(a_serie_info.ServerInfo);
                    });

                    a_serie_info.State.State = ItemState.Downloaded;
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                    a_serie_info.State.State = ItemState.Error;
                }

                OnSeriesChanged(a_serie_info.ServerInfo);

            });

            task.Start(a_serie_info.ServerInfo.State.Scheduler[Priority.Chapters]);

            OnSeriesChanged(a_serie_info.ServerInfo);

            return true;
        }

        public static void DownloadPages(IEnumerable<ChapterInfo> a_chapter_infos)
        {
            string baseDir = GetDirectoryPath();

            bool cbz = UseCBZ();

            foreach (var chapter_info in a_chapter_infos)
            {
                if (chapter_info.State.Working)
                    continue;

                chapter_info.State.Initialize();
                chapter_info.State.State = ItemState.Waiting;

                Task task = new Task(() =>
                {
                    try
                    {
                        ConnectionsLimiter.BeginDownloadPages(chapter_info);
                    }
                    catch (OperationCanceledException)
                    {
                        chapter_info.State.Finish(true);
                        OnChaptersChanged(chapter_info.SerieInfo);
                        return;
                    }

                    try
                    {
                        string dir = chapter_info.State.GetImageDirectory(baseDir);

                        new DirectoryInfo(dir).DeleteAll();

                        chapter_info.State.Token.ThrowIfCancellationRequested();

                        chapter_info.State.State = ItemState.Downloading;
                        OnChaptersChanged(chapter_info.SerieInfo);

                        chapter_info.DownloadPages();

                        chapter_info.State.Token.ThrowIfCancellationRequested();

                        Parallel.ForEach(chapter_info.Pages,
                            new ParallelOptions()
                            {
                                MaxDegreeOfParallelism = chapter_info.SerieInfo.ServerInfo.Crawler.MaxConnectionsPerServer,
                                TaskScheduler = chapter_info.SerieInfo.ServerInfo.State.Scheduler[Priority.Pages], 
                            },
                            (page, state) =>
                        {
                            try
                            {
                                if (page.DownloadAndSavePageImage(chapter_info.State.Token, dir))
                                    chapter_info.State.PageDownloaded();
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

                            OnChaptersChanged(chapter_info.SerieInfo);
                        });

                        if (!chapter_info.State.Token.IsCancellationRequested)
                        {
                            if (cbz)
                            {
                                chapter_info.State.State = ItemState.Zipping;
                                OnChaptersChanged(chapter_info.SerieInfo);

                                CreateCBZ(chapter_info);
                            }
                        }

                        chapter_info.State.Finish(a_error: false);
                    }
                    catch
                    {
                        chapter_info.State.Finish(a_error: true);
                    }
                    finally
                    {
                        ConnectionsLimiter.EndDownloadPages(chapter_info);
                    }

                    OnChaptersChanged(chapter_info.SerieInfo);

                });

                task.Start(chapter_info.SerieInfo.ServerInfo.State.Scheduler[Priority.Pages]);

                OnChaptersChanged(chapter_info.SerieInfo);
            }
        }

        private static void CreateCBZ(ChapterInfo a_chapter_info)
        {
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
                    a_chapter_info.State.Token.ThrowIfCancellationRequested();
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

        public static void UpdateVisuals()
        {
            OnServersChanged();
        }

        private static void OnServersChanged()
        {
            Action update = () =>
            {
                TryInvoke(() =>
                {
                    if (!Form.IsDisposed)
                        GetServersVisualState().ReloadItems(s_servers);
                });

                OnSeriesChanged();
            };

            ServersUpdate.Perform(update);
        }

        private static void OnSeriesChanged(ServerInfo a_server_info = null)
        {
            if (a_server_info != null)
            {
                if (SelectedServerInfo != a_server_info)
                    return;
            }

            Action update = () =>
            {
                TryInvoke(() =>
                {
                    SerieInfo[] ar = new SerieInfo[0];

                    if (s_selected_server_info != null)
                    {
                        string filter = GetSeriesFilter().ToLower();
                        ar = (from serie in s_selected_server_info.Series
                              where serie.Title.ToLower().IndexOf(filter) != -1
                              select serie).ToArray();
                    }

                    if (!Form.IsDisposed)
                        SeriesVisualState.ReloadItems(ar);
                });

                OnChaptersChanged();
            };

            SeriesUpdate.Perform(update);
        }

        private static void OnChaptersChanged(SerieInfo a_serie_info = null)
        {
            Action update = () =>
            {
                TryInvoke(() =>
                {
                    ChapterInfo[] ar = new ChapterInfo[0];

                    // TODO: nie da sie jakos prosciej, reloaditems jesli sa, clear jesli nie ma
                    if (SeriesVisualState.ItemSelected)
                    {
                        if (SelectedSerieInfo != null)
                        {
                            ar = (from ch in SelectedSerieInfo.Chapters
                                  select ch).ToArray();
                        }
                    }

                    if (!Form.IsDisposed)
                        ChaptersVisualState.ReloadItems(ar);
                });
            };

            if ((a_serie_info == null) || (SelectedSerieInfo == a_serie_info))
                ChaptersUpdate.Perform(update);

            OnTasksChanged();

        }

        private static void OnTasksChanged()
        {
            if (TasksChanged == null)
                return;

            Action update = () =>
            {
                TryInvoke(() =>
                {
                    if (!Form.IsDisposed)
                    {
                        var tasks = from ch in AllChapters
                                    where ch.State.IsTask
                                    select ch;
                        TasksChanged(tasks);
                    }
                });
            };

            TasksUpdate.Perform(update);
        }

        public static bool DownloadingPages
        {
            get
            {
                return AllChapters.Any(ch => ch.State.State == ItemState.Downloading);
            }
        }

        public static void DeleteTask(ChapterState a_chapterItem)
        {
            a_chapterItem.Delete();
            OnTasksChanged();
        }
    }
}
