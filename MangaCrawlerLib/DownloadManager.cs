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

namespace MangaCrawlerLib
{
    public static class DownloadManager
    {
        private static Dictionary<ServerInfo, CustomTaskScheduler> s_schedulers =
            new Dictionary<ServerInfo, CustomTaskScheduler>();

        private static ServerItem s_selectedServer;
        private static Dictionary<ServerItem, SerieItem> s_selectedSerie = 
            new Dictionary<ServerItem,SerieItem>();
        private static Dictionary<ServerItem, VisualState> s_seriesListBoxState = 
            new Dictionary<ServerItem, VisualState>();
        private static Dictionary<SerieItem, ChapterItem> s_selectedChapter = 
            new Dictionary<SerieItem,ChapterItem>();
        private static Dictionary<SerieItem, VisualState> s_chaptersListBoxState = 
            new Dictionary<SerieItem, VisualState>();

        private static ConcurrentDictionary<ChapterInfo, ChapterItem> s_chapters =
            new ConcurrentDictionary<ChapterInfo, ChapterItem>();
        private static ConcurrentDictionary<SerieInfo, SerieItem> s_series =
            new ConcurrentDictionary<SerieInfo, SerieItem>();
        private static List<ServerItem> s_servers = new List<ServerItem>();
        private static List<ChapterItem> s_tasks = new List<ChapterItem>();

        public static Form Form;

        public static event Action<IEnumerable<ChapterItem>> TasksChanged;

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
                s_servers.Add(new ServerItem(server));
        }

        public static VisualState SeriesVisualState
        {
            get
            {
                if (SelectedServer == null)
                    return GetSeriesVisualState();
                if (!s_seriesListBoxState.ContainsKey(SelectedServer))
                    return GetSeriesVisualState();

                return s_seriesListBoxState[SelectedServer];
            }
            set
            {
                if (SelectedServer != null)
                    s_seriesListBoxState[SelectedServer] = value;
            }
        }

        public static VisualState ChaptersVisualState
        {
            get
            {
                if (SelectedSerie == null)
                    return GetChaptersVisualState();
                if (!s_chaptersListBoxState.ContainsKey(SelectedSerie))
                    return GetChaptersVisualState();

                return s_chaptersListBoxState[SelectedSerie];
            }
            set
            {
                if (SelectedSerie != null)
                    s_chaptersListBoxState[SelectedSerie] = value;
            }
        }

        public static SerieItem SelectedSerie
        {
            get
            {
                if (SelectedServer == null)
                    return null;

                if (!s_selectedSerie.ContainsKey(SelectedServer))
                    return null;

                return s_selectedSerie[SelectedServer];
            }
            set
            {
                ChaptersVisualState = GetChaptersVisualState();

                if (Object.ReferenceEquals(value, SelectedSerie))
                    return;
                s_selectedSerie[SelectedServer] = value;

                if (!DownloadChapters(SelectedSerie))
                    OnChaptersChanged(SelectedSerie);

                SeriesVisualState = GetSeriesVisualState();
            }
        }

        public static ServerItem SelectedServer
        {
            get
            {
                return s_selectedServer;
            }
            set
            {
                SeriesVisualState = GetSeriesVisualState();

                if (Object.ReferenceEquals(SelectedServer, value))
                    return;
                s_selectedServer = value;

                if (!DownloadSeries(s_selectedServer))
                    OnSeriesChanged(s_selectedServer);
            }
        }

        public static ChapterItem SelectedChapter
        {
            get
            {
                if (SelectedSerie == null)
                    return null;

                if (!s_selectedChapter.ContainsKey(SelectedSerie))
                    return null;

                return s_selectedChapter[SelectedSerie];
            }
            set
            {
                if (Object.ReferenceEquals(SelectedChapter, value))
                    return;

                s_selectedChapter[SelectedSerie] = value;

                ChaptersVisualState = GetChaptersVisualState();
            }
        }

        private static bool DownloadSeries(ServerItem a_item)
        {
            if (a_item == null)
                return false;
            if (!a_item.DownloadRequired)
                return false;

            a_item.Initialize();
            a_item.State = ItemState.Downloading;

            Task task = new Task(() =>
            {
                try
                {
                    a_item.ServerInfo.DownloadSeries((progress) =>
                    {
                        foreach (var serie in a_item.ServerInfo.Series)
                        {
                            if (s_series.ContainsKey(serie))
                                continue;

                            s_series[serie] = new SerieItem(serie, a_item);
                        }

                        a_item.Progress = progress;

                        if (progress != 100)
                            OnServersChanged();
                    });

                    a_item.State = ItemState.Downloaded;
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                    a_item.State = ItemState.Error;
                }

                OnServersChanged();
            });

            task.Start(s_schedulers[a_item.ServerInfo].Scheduler(Priority.Series));

            OnServersChanged();

            return true;
        }

        private static bool DownloadChapters(SerieItem a_serieItem)
        {
            if (a_serieItem == null)
                return false;
            if (!a_serieItem.DownloadRequired)
                return false;

            a_serieItem.Initialize();
            a_serieItem.State = ItemState.Downloading;

            Task task = new Task(() =>
            {
                try
                {
                    a_serieItem.SerieInfo.DownloadChapters((progress) =>
                    {
                        foreach (var chapter in a_serieItem.SerieInfo.Chapters)
                        {
                            if (s_chapters.ContainsKey(chapter))
                                continue;

                            s_chapters[chapter] = new ChapterItem(chapter, a_serieItem);
                        }

                        a_serieItem.Progress = progress;

                        if (progress != 100)
                            OnSeriesChanged(a_serieItem.ServerItem);
                    });

                    a_serieItem.State = ItemState.Downloaded;
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                    a_serieItem.State = ItemState.Error;
                }

                OnSeriesChanged(a_serieItem.ServerItem);

            });

            task.Start(
                s_schedulers[a_serieItem.ServerItem.ServerInfo].Scheduler(Priority.Chapters));

            OnSeriesChanged(a_serieItem.ServerItem);

            return true;
        }

        public static void DownloadPages(IEnumerable<ChapterItem> a_items)
        {
            string baseDir = GetDirectoryPath();

            bool cbz = UseCBZ();

            foreach (var item in a_items)
            {
                ChapterItem chapter_item = (ChapterItem)item;

                if (chapter_item.Working)
                    continue;

                chapter_item.Initialize();
                chapter_item.State = ItemState.Waiting;

                Task task = new Task(() =>
                {
                    try
                    {
                        ConnectionsLimiter.BeginDownloadPages(chapter_item.ChapterInfo, 
                            chapter_item.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        chapter_item.Finish(true);
                        OnChaptersChanged(chapter_item.SerieItem);
                        return;
                    }

                    try
                    {
                        string dir = chapter_item.GetImageDirectory(baseDir);

                        new DirectoryInfo(dir).DeleteAll();

                        chapter_item.Token.ThrowIfCancellationRequested();

                        chapter_item.State = ItemState.Downloading;
                        OnChaptersChanged(chapter_item.SerieItem);

                        chapter_item.ChapterInfo.DownloadPages(chapter_item.Token);

                        chapter_item.Token.ThrowIfCancellationRequested();

                        Parallel.ForEach(chapter_item.ChapterInfo.Pages,
                            new ParallelOptions()
                            {
                                MaxDegreeOfParallelism =
                                    ConnectionsLimiter.MAX_CONNECTIONS_PER_SERVER
                            },
                            (page, state) =>
                        {
                            try
                            {
                                if (page.DownloadAndSavePageImage(chapter_item.Token, dir))
                                    chapter_item.PageDownloaded();
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

                            OnChaptersChanged(chapter_item.SerieItem);
                        });

                        if (!chapter_item.Token.IsCancellationRequested)
                        {
                            if (cbz)
                            {
                                chapter_item.State = ItemState.Zipping;
                                OnChaptersChanged(chapter_item.SerieItem);

                                CreateCBZ(chapter_item);
                            }
                        }

                        chapter_item.Finish(a_error: false);
                    }
                    catch
                    {
                        chapter_item.Finish(a_error: true);
                    }
                    finally
                    {
                        ConnectionsLimiter.EndDownloadPages(chapter_item.ChapterInfo);
                    }

                    OnChaptersChanged(chapter_item.SerieItem);

                });

                task.Start(s_schedulers[chapter_item.SerieItem.ServerItem.ServerInfo].Scheduler(
                    Priority.Pages));

                OnChaptersChanged(chapter_item.SerieItem);
            }
        }

        private static void CreateCBZ(ChapterItem a_chapter)
        {
            var dir = new DirectoryInfo(a_chapter.ChapterInfo.Pages.First().GetImageFilePath()).Parent;

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

                foreach (var page in a_chapter.ChapterInfo.Pages)
                {
                    zip.AddFile(page.GetImageFilePath(), "");
                    a_chapter.Token.ThrowIfCancellationRequested();
                }

                zip.Save(zip_file);
            }

            try
            {
                foreach (var page in a_chapter.ChapterInfo.Pages)
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
            };

            ServersUpdate.Perform(update);

            OnSeriesChanged();

        }

        private static void OnSeriesChanged(ServerItem a_serverItem = null)
        {
            if (a_serverItem != null)
            {
                if (SelectedServer != a_serverItem)
                    return;
            }

            Action update = () =>
            {
                TryInvoke(() =>
                {
                    List<SerieItem> list = new List<SerieItem>();

                    if (s_selectedServer != null)
                    {
                        string filter = GetSeriesFilter().ToLower();
                        list = (from serie in s_selectedServer.ServerInfo.Series
                                where serie.Name.ToLower().IndexOf(filter) != -1
                                where s_series.ContainsKey(serie) // moze zostac wywolane poprzez s_seriesUpdateLimiter, moga istniec serie w Series nie dodane do s_series.
                                select s_series[serie]).ToList();
                    }

                    if (!Form.IsDisposed)
                        SeriesVisualState.ReloadItems(list.AsReadOnly());

                    if (SelectedSerie == null)
                        OnChaptersChanged();
                });
            };

            SeriesUpdate.Perform(update);

            OnChaptersChanged();
        }

        private static void OnChaptersChanged(SerieItem a_serieItem = null)
        {
            if (a_serieItem != null)
            {
                if (SelectedSerie != a_serieItem)
                    return;
            }

            Action update = () =>
            {
                List<ChapterItem> list = new List<ChapterItem>();

                if (SelectedSerie != null)
                {
                    list = (from ch in SelectedSerie.SerieInfo.Chapters
                            select s_chapters[ch]).ToList();
                }

                TryInvoke(() =>
                {
                    if (!Form.IsDisposed)
                        ChaptersVisualState.ReloadItems(list.AsReadOnly());
                });
            };

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
                    var all_tasks = (from ch in s_chapters.Values
                                        where ch.IsTask
                                        select ch).ToList();

                    var add = (from task in all_tasks
                                where !s_tasks.Contains(task)
                                select task).ToList();

                    var remove = (from task in s_tasks
                                    where !all_tasks.Contains(task)
                                    select task).ToList();

                    foreach (var el in add)
                        s_tasks.Add(el);
                    foreach (var el in remove)
                        s_tasks.Remove(el);

                    if (!Form.IsDisposed)
                        TasksChanged(s_tasks.AsReadOnly());
                });
            };

            TasksUpdate.Perform(update);
        }

        public static bool DownloadingPages
        {
            get
            {
                return s_chapters.Values.Any(chi => chi.State == ItemState.Downloading);
            }
        }

        public static void DeleteTask(ChapterItem a_chapterItem)
        {
            a_chapterItem.Delete();
            OnTasksChanged();
        }
    }
}
