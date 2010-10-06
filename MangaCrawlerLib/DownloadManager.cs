using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace MangaCrawlerLib
{
    public static class DownloadManager
    {
        private static ReprioritizableTaskScheduler s_scheduler = new ReprioritizableTaskScheduler();

        private static ServerItem m_selectedServer;
        private static Dictionary<ServerItem, SerieItem> m_selectedSerie = new Dictionary<ServerItem,SerieItem>();
        private static Dictionary<ServerItem, VisualState> m_seriesListBoxState = new Dictionary<ServerItem, VisualState>();
        private static Dictionary<SerieItem, ChapterItem> m_selectedChapter = new Dictionary<SerieItem,ChapterItem>();
        private static Dictionary<SerieItem, VisualState> m_chaptersListBoxState = new Dictionary<SerieItem, VisualState>();

        private static Dictionary<ChapterInfo, ChapterItem> m_chapters = new Dictionary<ChapterInfo, ChapterItem>();
        private static Dictionary<SerieInfo, SerieItem> m_series = new Dictionary<SerieInfo, SerieItem>();
        private static List<ServerItem> m_servers = new List<ServerItem>();
        private static List<ChapterItem> m_tasks = new List<ChapterItem>();


        public static Form Form;

        public static event Action<IEnumerable<ChapterItem>> TasksChanged;

        public static Func<string> GetSeriesFilter;
        public static Func<string> GetDirectoryPath;

        public static Func<VisualState> GetServersVisualState;
        public static Func<VisualState> GetSeriesVisualState;
        public static Func<VisualState> GetChaptersVisualState;

        static DownloadManager()
        {
            foreach (var server in ServerInfo.ServersInfos)
                m_servers.Add(new ServerItem(server));
        }

        public static VisualState SeriesVisualState
        {
            get
            {
                if (SelectedServer == null)
                    return GetSeriesVisualState();
                if (!m_seriesListBoxState.ContainsKey(SelectedServer))
                    return GetSeriesVisualState();

                return m_seriesListBoxState[SelectedServer];
            }
            set
            {
                if (SelectedServer != null)
                    m_seriesListBoxState[SelectedServer] = value;
            }
        }

        public static VisualState ChaptersVisualState
        {
            get
            {
                if (SelectedSerie == null)
                    return GetChaptersVisualState();
                if (!m_chaptersListBoxState.ContainsKey(SelectedSerie))
                    return GetChaptersVisualState();

                return m_chaptersListBoxState[SelectedSerie];
            }
            set
            {
                if (SelectedSerie != null)
                    m_chaptersListBoxState[SelectedSerie] = value;
            }
        }

        public static SerieItem SelectedSerie
        {
            get
            {
                if (SelectedServer == null)
                    return null;

                if (!m_selectedSerie.ContainsKey(SelectedServer))
                    return null;

                return m_selectedSerie[SelectedServer];
            }
            set
            {
                ChaptersVisualState = GetChaptersVisualState();

                if (Object.ReferenceEquals(value, SelectedSerie))
                    return;
                m_selectedSerie[SelectedServer] = value;

                if (!DownloadChapters(SelectedSerie))
                    OnChaptersChanged();

                SeriesVisualState = GetSeriesVisualState();
            }
        }

        public static ServerItem SelectedServer
        {
            get
            {
                return m_selectedServer;
            }
            set
            {
                SeriesVisualState = GetSeriesVisualState();

                if (Object.ReferenceEquals(SelectedServer, value))
                    return;
                m_selectedServer = value;

                if (!DownloadSeries(m_selectedServer))
                    OnSeriesChanged();
            }
        }

        public static ChapterItem SelectedChapter
        {
            get
            {
                if (SelectedSerie == null)
                    return null;

                if (!m_selectedChapter.ContainsKey(SelectedSerie))
                    return null;

                return m_selectedChapter[SelectedSerie];
            }
            set
            {
                if (Object.ReferenceEquals(SelectedChapter, value))
                    return;

                m_selectedChapter[SelectedSerie] = value;

                ChaptersVisualState = GetChaptersVisualState();
            }
        }

        private static bool DownloadSeries(ServerItem a_item)
        {
            if (a_item == null)
                return false;
            if (a_item.Downloading || a_item.Downloaded)
                return false;

            a_item.Initialize();
            a_item.Downloading = true;

            Task task = new Task(() =>
            {
                try
                {
                    a_item.ServerInfo.DownloadSeries((progress) =>
                    {
                        foreach (var serie in a_item.ServerInfo.Series)
                        {
                            if (m_series.ContainsKey(serie))
                                continue;

                            m_series[serie] = new SerieItem(serie);
                        }

                        a_item.Progress = progress;

                        if (progress != 100)
                            OnServersChanged();
                    });

                    a_item.Downloaded = true;
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                    a_item.Error = true;
                }

                a_item.Downloading = false;

                OnServersChanged();
            });

            task.Start(s_scheduler);

            OnServersChanged();

            return true;
        }

        private static bool DownloadChapters(SerieItem a_item)
        {
            if (a_item == null)
                return false;
            if (a_item.Downloading || a_item.Downloaded)
                return false;

            a_item.Initialize();
            a_item.Downloading = true;

            Task task = new Task(() =>
            {
                try
                {
                    a_item.SerieInfo.DownloadChapters((progress) =>
                    {
                        foreach (var chapter in a_item.SerieInfo.Chapters)
                        {
                            if (m_chapters.ContainsKey(chapter))
                                continue;

                            m_chapters[chapter] = new ChapterItem(chapter);
                        }

                        a_item.Progress = progress;

                        if (progress != 100)
                            OnSeriesChanged();
                    });

                    a_item.Downloaded = true;
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                    a_item.Error = true;
                }

                a_item.Downloading = false;

                OnSeriesChanged();

            });

            task.Start(s_scheduler);
            s_scheduler.Prioritize(task);

            OnSeriesChanged();

            return true;
        }

        public static void DownloadPages(IEnumerable<ChapterItem> a_items)
        {
            string baseDir = GetDirectoryPath();

            try
            {
                new DirectoryInfo(baseDir);
            }
            catch
            {
                MessageBox.Show(String.Format("Directory path is invalid: '{0}'.", baseDir),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                new DirectoryInfo(baseDir).Create();
            }
            catch
            {
                MessageBox.Show(String.Format("Can't create directory: '{0}'.", baseDir),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (var item in a_items)
            {
                ChapterItem chapter_item = (ChapterItem)item;

                if (chapter_item.Waiting || chapter_item.Downloading)
                    continue;

                chapter_item.Initialize();
                chapter_item.Waiting = true;

                Task task = new Task(() =>
                {
                    try
                    {
                        ConnectionsLimiter.BeginDownloadPages(chapter_item.ChapterInfo, chapter_item.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        chapter_item.Finish(true);
                        OnChaptersChanged();
                        return;
                    }

                    try
                    {
                        try
                        {
                            chapter_item.Token.ThrowIfCancellationRequested();

                            chapter_item.Waiting = false;
                            chapter_item.Downloading = true;

                            OnChaptersChanged();

                            chapter_item.ChapterInfo.DownloadPages(chapter_item.Token);

                            chapter_item.Token.ThrowIfCancellationRequested();

                            string dir = chapter_item.GetImageDirectory(baseDir);

                            if (new DirectoryInfo(dir).Exists)
                                new DirectoryInfo(dir).Delete(true);
                            
                            Parallel.ForEach(chapter_item.ChapterInfo.Pages, (page, state) =>
                            {
                                try
                                {
                                    page.DownloadAndSavePageImage(chapter_item.Token, dir);
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

                                chapter_item.PageDownloaded();

                                OnChaptersChanged();
                            });

                            chapter_item.Finish(false);
                        }
                        catch
                        {
                            chapter_item.Finish(true);
                        }
                    }
                    finally
                    {
                        ConnectionsLimiter.EndDownloadPages(chapter_item.ChapterInfo);
                    }

                    OnChaptersChanged();

                });

                task.Start(s_scheduler);

                OnChaptersChanged();
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
            TryInvoke(() =>
            {
                if (!Form.IsDisposed)
                    GetServersVisualState().ReloadItems(m_servers.AsReadOnly());
            });

            OnSeriesChanged();
        }

        private static void OnSeriesChanged()
        {
            List<SerieItem> list = new List<SerieItem>();

            if (m_selectedServer != null)
            {
                string filter = GetSeriesFilter().ToLower();
                list = (from serie in m_selectedServer.ServerInfo.Series
                        where serie.Name.ToLower().IndexOf(filter) != -1
                        select m_series[serie]).ToList();
            }

            TryInvoke(() =>
            {
                if (!Form.IsDisposed)
                    SeriesVisualState.ReloadItems(list.AsReadOnly());
            });

            OnChaptersChanged();
        }

        private static void OnChaptersChanged()
        {
            List<ChapterItem> list = new List<ChapterItem>();

            if (SelectedSerie != null)
            {
                list = (from ch in SelectedSerie.SerieInfo.Chapters
                        select m_chapters[ch]).ToList();
            }

            TryInvoke(() =>
            {
                if (!Form.IsDisposed)
                    ChaptersVisualState.ReloadItems(list.AsReadOnly());
            });

            OnTasksChanged();
        }

        private static void OnTasksChanged()
        {
            if (TasksChanged != null)
            {
                TryInvoke(() =>
                {
                    var all_tasks = (from ch in m_chapters.Values
                                    where (ch.Waiting || ch.Error || ch.Downloading)
                                    select ch).ToList();

                    var add = (from task in all_tasks
                               where !m_tasks.Contains(task)
                               select task).ToList();

                    var remove = (from task in m_tasks
                                  where !all_tasks.Contains(task)
                                  select task).ToList();

                    foreach (var el in add)
                        m_tasks.Add(el);
                    foreach (var el in remove)
                        m_tasks.Remove(el);

                    if (!Form.IsDisposed)
                        TasksChanged(m_tasks.AsReadOnly());
                });
            }
        }

        public static bool DownloadingPages
        {
            get
            {
                return m_chapters.Values.Any(chi => chi.Downloading);
            }
        }

        public static void DeleteTask(ChapterItem a_chapterItem)
        {
            a_chapterItem.Delete();
            OnTasksChanged();
        }
    }
}
