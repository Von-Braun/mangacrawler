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
        private static Dictionary<ServerInfo, CustomTaskScheduler> s_schedulers =
            new Dictionary<ServerInfo, CustomTaskScheduler>();

        private static ServerState s_selectedServerState;
        private static Dictionary<ServerState, SerieState> s_selectedSeries = 
            new Dictionary<ServerState,SerieState>();
        private static Dictionary<ServerState, VisualState> s_seriesVisualStates = 
            new Dictionary<ServerState, VisualState>();
        private static Dictionary<SerieState, ChapterState> s_selectedChapters = 
            new Dictionary<SerieState,ChapterState>();
        private static Dictionary<SerieState, VisualState> s_chaptersVisualStates = 
            new Dictionary<SerieState, VisualState>();

        private static ConcurrentDictionary<ChapterInfo, ChapterState> s_chaptersMap =
            new ConcurrentDictionary<ChapterInfo, ChapterState>();
        private static ConcurrentDictionary<SerieInfo, SerieState> s_seriesMap =
            new ConcurrentDictionary<SerieInfo, SerieState>();
        private static List<ServerState> s_servers = new List<ServerState>();

        public static Form Form;

        public static event Action<ReadOnlyCollection<ChapterState>> TasksChanged;

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

            foreach (var si in ServerInfo.ServersInfos)
            {
                s_schedulers[si] = new CustomTaskScheduler(
                    ConnectionsLimiter.MAX_CONNECTIONS_PER_SERVER * 4 / 3);
            }

            foreach (var server in ServerInfo.ServersInfos)
                s_servers.Add(new ServerState(server));
        }

        public static VisualState SeriesVisualState
        {
            get
            {
                if (SelectedServerState == null)
                    return GetSeriesVisualState();
                if (!s_seriesVisualStates.ContainsKey(SelectedServerState))
                    return GetSeriesVisualState();

                return s_seriesVisualStates[SelectedServerState];
            }
            set
            {
                if (SelectedServerState != null)
                    s_seriesVisualStates[SelectedServerState] = value;
            }
        }

        public static VisualState ChaptersVisualState
        {
            get
            {
                if (SelectedSerieState == null)
                    return GetChaptersVisualState();
                if (!s_chaptersVisualStates.ContainsKey(SelectedSerieState))
                    return GetChaptersVisualState();

                return s_chaptersVisualStates[SelectedSerieState];
            }
            set
            {
                if (SelectedSerieState != null)
                    s_chaptersVisualStates[SelectedSerieState] = value;
            }
        }

        public static SerieState SelectedSerieState
        {
            get
            {
                if (SelectedServerState == null)
                    return null;

                if (!s_selectedSeries.ContainsKey(SelectedServerState))
                    return null;

                return s_selectedSeries[SelectedServerState];
            }
            set
            {
                SeriesVisualState = GetSeriesVisualState();
                s_selectedSeries[SelectedServerState] = value;

                if (!DownloadChapters(SelectedSerieState))
                    OnChaptersChanged(SelectedSerieState);
            }
        }

        public static ServerState SelectedServerState
        {
            get
            {
                return s_selectedServerState;
            }
            set
            {
                s_selectedServerState = value;

                if (!DownloadSeries(s_selectedServerState))
                    OnSeriesChanged(s_selectedServerState);
            }
        }

        public static ChapterState SelectedChapterState
        {
            get
            {
                if (SelectedSerieState == null)
                    return null;

                if (!s_selectedChapters.ContainsKey(SelectedSerieState))
                    return null;

                return s_selectedChapters[SelectedSerieState];
            }
            set
            {
                ChaptersVisualState = GetChaptersVisualState();

                if (SelectedSerieState != null)
                    s_selectedChapters[SelectedSerieState] = value;
            }
        }

        private static bool DownloadSeries(ServerState a_serverState)
        {
            if (a_serverState == null)
                return false;
            if (!a_serverState.DownloadRequired)
                return false;

            a_serverState.Initialize();
            a_serverState.State = ItemState.Downloading;

            Task task = new Task(() =>
            {
                try
                {
                    a_serverState.ServerInfo.DownloadSeries((progress) =>
                    {
                        foreach (var serie in a_serverState.ServerInfo.Series)
                        {
                            if (s_seriesMap.ContainsKey(serie))
                                continue;

                            s_seriesMap[serie] = new SerieState(serie, a_serverState);
                        }

                        a_serverState.Progress = progress;

                        if (progress != 100)
                            OnServersChanged();
                    });

                    a_serverState.State = ItemState.Downloaded;
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                    a_serverState.State = ItemState.Error;
                }

                OnServersChanged();
            });

            task.Start(s_schedulers[a_serverState.ServerInfo].Scheduler(Priority.Series));

            OnServersChanged();

            return true;
        }

        private static bool DownloadChapters(SerieState a_serieState)
        {
            if (a_serieState == null)
                return false;
            if (!a_serieState.DownloadRequired)
                return false;

            a_serieState.Initialize();
            a_serieState.State = ItemState.Downloading;

            Task task = new Task(() =>
            {
                try
                {
                    a_serieState.SerieInfo.DownloadChapters((progress) =>
                    {
                        foreach (var chapter in a_serieState.SerieInfo.Chapters)
                        {
                            if (s_chaptersMap.ContainsKey(chapter))
                                continue;

                            s_chaptersMap[chapter] = new ChapterState(chapter, a_serieState);
                        }

                        a_serieState.Progress = progress;

                        if (progress != 100)
                            OnSeriesChanged(a_serieState.ServerState);
                    });

                    a_serieState.State = ItemState.Downloaded;
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                    a_serieState.State = ItemState.Error;
                }

                OnSeriesChanged(a_serieState.ServerState);

            });

            task.Start(
                s_schedulers[a_serieState.ServerState.ServerInfo].Scheduler(Priority.Chapters));

            OnSeriesChanged(a_serieState.ServerState);

            return true;
        }

        public static void DownloadPages(IEnumerable<ChapterState> a_chapter_states)
        {
            string baseDir = GetDirectoryPath();

            bool cbz = UseCBZ();

            foreach (var chapter_state in a_chapter_states)
            {
                if (chapter_state.Working)
                    continue;

                chapter_state.Initialize();
                chapter_state.State = ItemState.Waiting;

                Task task = new Task(() =>
                {
                    try
                    {
                        ConnectionsLimiter.BeginDownloadPages(chapter_state.ChapterInfo, 
                            chapter_state.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        chapter_state.Finish(true);
                        OnChaptersChanged(chapter_state.SerieState);
                        return;
                    }

                    try
                    {
                        string dir = chapter_state.GetImageDirectory(baseDir);

                        new DirectoryInfo(dir).DeleteAll();

                        chapter_state.Token.ThrowIfCancellationRequested();

                        chapter_state.State = ItemState.Downloading;
                        OnChaptersChanged(chapter_state.SerieState);

                        chapter_state.ChapterInfo.DownloadPages(chapter_state.Token);

                        chapter_state.Token.ThrowIfCancellationRequested();

                        Parallel.ForEach(chapter_state.ChapterInfo.Pages,
                            new ParallelOptions()
                            {
                                MaxDegreeOfParallelism =
                                    ConnectionsLimiter.MAX_CONNECTIONS_PER_SERVER
                            },
                            (page, state) =>
                        {
                            try
                            {
                                if (page.DownloadAndSavePageImage(chapter_state.Token, dir))
                                    chapter_state.PageDownloaded();
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

                            OnChaptersChanged(chapter_state.SerieState);
                        });

                        if (!chapter_state.Token.IsCancellationRequested)
                        {
                            if (cbz)
                            {
                                chapter_state.State = ItemState.Zipping;
                                OnChaptersChanged(chapter_state.SerieState);

                                CreateCBZ(chapter_state);
                            }
                        }

                        chapter_state.Finish(a_error: false);
                    }
                    catch
                    {
                        chapter_state.Finish(a_error: true);
                    }
                    finally
                    {
                        ConnectionsLimiter.EndDownloadPages(chapter_state.ChapterInfo);
                    }

                    OnChaptersChanged(chapter_state.SerieState);

                });

                task.Start(s_schedulers[chapter_state.SerieState.ServerState.ServerInfo].Scheduler(
                    Priority.Pages));

                OnChaptersChanged(chapter_state.SerieState);
            }
        }

        private static void CreateCBZ(ChapterState a_chapter_state)
        {
            var dir = new DirectoryInfo(a_chapter_state.ChapterInfo.Pages.First().GetImageFilePath()).Parent;

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

                foreach (var page in a_chapter_state.ChapterInfo.Pages)
                {
                    zip.AddFile(page.GetImageFilePath(), "");
                    a_chapter_state.Token.ThrowIfCancellationRequested();
                }

                zip.Save(zip_file);
            }

            try
            {
                foreach (var page in a_chapter_state.ChapterInfo.Pages)
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
                        GetServersVisualState().ReloadItems(s_servers.AsReadOnly());
                });

                OnSeriesChanged();
            };

            ServersUpdate.Perform(update);
        }

        private static void OnSeriesChanged(ServerState a_serverItem = null)
        {
            if (a_serverItem != null)
            {
                if (SelectedServerState != a_serverItem)
                    return;
            }

            Action update = () =>
            {
                TryInvoke(() =>
                {
                    List<SerieState> list = new List<SerieState>();

                    if (s_selectedServerState != null)
                    {
                        string filter = GetSeriesFilter().ToLower();
                        list = (from serie in s_selectedServerState.ServerInfo.Series
                                where serie.Title.ToLower().IndexOf(filter) != -1
                                where s_seriesMap.ContainsKey(serie) // moze zostac wywolane poprzez s_seriesUpdateLimiter, moga istniec serie w Series nie dodane do s_series.
                                select s_seriesMap[serie]).ToList();
                    }

                    if (!Form.IsDisposed)
                        SeriesVisualState.ReloadItems(list.AsReadOnly());
                });

                OnChaptersChanged();
            };

            SeriesUpdate.Perform(update);
        }

        private static void OnChaptersChanged(SerieState a_serieStates = null)
        {
            Action update = () =>
            {
                TryInvoke(() =>
                {
                    List<ChapterState> list = new List<ChapterState>();

                    if (SeriesVisualState.ItemSelected)
                    {
                        if (SelectedSerieState != null)
                        {
                            list = (from ch in SelectedSerieState.SerieInfo.Chapters
                                    select s_chaptersMap[ch]).ToList();
                        }
                    }

                    if (!Form.IsDisposed)
                        ChaptersVisualState.ReloadItems(list.AsReadOnly());
                });
            };

            if ((a_serieStates == null) || (SelectedSerieState == a_serieStates))
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
                        var tasks = from ch in s_chaptersMap.Values
                                    where ch.IsTask
                                    select ch;
                        TasksChanged(tasks.ToList().AsReadOnly());
                    }
                });
            };

            TasksUpdate.Perform(update);
        }

        public static bool DownloadingPages
        {
            get
            {
                return s_chaptersMap.Values.Any(chi => chi.State == ItemState.Downloading);
            }
        }

        public static void DeleteTask(ChapterState a_chapterItem)
        {
            a_chapterItem.Delete();
            OnTasksChanged();
        }
    }
}
