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
        private static Dictionary<ServerItem, ListBoxState> m_seriesListBoxState = new Dictionary<ServerItem, ListBoxState>();
        private static Dictionary<SerieItem, ChapterItem> m_selectedChapter = new Dictionary<SerieItem,ChapterItem>();
        private static Dictionary<SerieItem, ListBoxState> m_chaptersListBoxState = new Dictionary<SerieItem, ListBoxState>();

        private static Dictionary<ChapterInfo, ChapterItem> m_chapters = new Dictionary<ChapterInfo, ChapterItem>();
        private static Dictionary<SerieInfo, SerieItem> m_series = new Dictionary<SerieInfo, SerieItem>();
        private static List<ServerItem> m_servers = new List<ServerItem>();
        private static List<ChapterItem> m_tasks = new List<ChapterItem>();

        public static Form Form;

        // TODO: dodac begininvoke
        public static Action<IList<ServerItem>> ServersChanged;
        public static Action<IList<SerieItem>, ListBoxState> SeriesChanged;
        public static Action<IList<ChapterItem>, ListBoxState> ChaptersChanged;
        public static Action<IList<ChapterItem>> TasksChanged;
        public static Func<string> GetSeriesFilter;
        public static Func<string> GetDirectoryPath;

        static DownloadManager()
        {
            foreach (var server in ServerInfo.ServersInfos)
                m_servers.Add(new ServerItem(server));
        }

        public static IDictionary<ChapterInfo, ChapterItem> Chapters
        {
            get
            {
                return m_chapters.AsReadOnly();
            }
        }

        public static ListBoxState SelectedServerSerieListBoxState
        {
            get
            {
                if (SelectedServer == null)
                    return null;
                if (!m_seriesListBoxState.ContainsKey(SelectedServer))
                    return null;

                return m_seriesListBoxState[SelectedServer];
            }
            set
            {
                m_seriesListBoxState[SelectedServer] = value;
            }
        }

        public static ListBoxState SelectedSerieChapterListBoxState
        {
            get
            {
                if (SelectedSerie == null)
                    return null;
                if (!m_chaptersListBoxState.ContainsKey(SelectedSerie))
                    return null;

                return m_chaptersListBoxState[SelectedSerie];
            }
            set
            {
                m_chaptersListBoxState[SelectedSerie] = value;
            }
        }

        public static IList<ChapterItem> Tasks
        {
            get
            {
                return m_tasks.AsReadOnly();
            }
        }

        public static IDictionary<SerieInfo, SerieItem> Series
        {
            get
            {
                return m_series.AsReadOnly();
            }
        }

        public static IList<ServerItem> Servers
        {
            get
            {
                return m_servers.AsReadOnly();
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
                if (Object.ReferenceEquals(value, SelectedSerie))
                    return;
                m_selectedSerie[SelectedServer] = value;

                if (SelectedSerie != null)
                {
                    if (!DownloadChapters(SelectedSerie))
                        OnChaptersChanged();
                }
                else
                    OnChaptersChanged();
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
                if (Object.ReferenceEquals(SelectedServer, value))
                    return;
                m_selectedServer = value;

                if (m_selectedServer != null)
                    if (!DownloadManager.DownloadSeries(m_selectedServer))
                        OnSeriesChanged();
                else
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
            }
        }

        public static bool DownloadSeries(ServerItem a_item)
        {
            if (a_item.Downloading || (a_item.Finished && !a_item.Error))
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

                        a_item.SetProgress(progress);

                        if (progress != 100)
                            OnServersChanged();
                    });
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                    a_item.Error = true;
                }

                a_item.Downloading = false;
                a_item.Finished = true;

                OnServersChanged();
            });

            task.Start(s_scheduler);

            OnServersChanged();

            return true;
        }

        public static bool DownloadChapters(SerieItem a_item)
        {
            if (a_item.Downloading || (a_item.Finished && !a_item.Error))
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

                        a_item.SetProgress(progress);

                        if (progress != 100)
                            OnSeriesChanged();
                    });
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                    a_item.Error = true;
                }

                a_item.Downloading = false;
                a_item.Finished = true;

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

        public static void OnServersChanged()
        {
            if (ServersChanged != null)
            {
                TryInvoke(() =>
                {
                    if (!Form.IsDisposed)
                        ServersChanged(Servers);
                });
            }
        }

        public static void OnSeriesChanged()
        {
            if (SeriesChanged != null)
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
                        SeriesChanged(list.AsReadOnly(), SelectedServerSerieListBoxState);
                });
            }
        }

        public static void OnChaptersChanged()
        {
            if (ChaptersChanged != null)
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
                        ChaptersChanged(list.AsReadOnly(), SelectedSerieChapterListBoxState);
                });
            }

            OnTasksChanged();
        }

        public static void OnTasksChanged()
        {
            if (TasksChanged != null)
            {
                TryInvoke(() =>
                {
                    var all_tasks = (from ch in m_chapters.Values
                                    where (ch.Waiting || (ch.Finished && ch.Error) || ch.Downloading)
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
