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

        public static Dictionary<ChapterInfo, ChapterItem> Chapters = new Dictionary<ChapterInfo, ChapterItem>();
        public static Dictionary<SerieInfo, SerieItem> Series = new Dictionary<SerieInfo, SerieItem>();
        public static List<ServerItem> Servers = new List<ServerItem>();

        public static Form Form;

        public static event Action ServersChanged;
        public static event Action SeriesChanged;
        public static event Action ChaptersChanged;

        static DownloadManager()
        {
            foreach (var server in ServerInfo.ServersInfos)
                Servers.Add(new ServerItem(server));
        }

        public static void DownloadSeries(ServerItem a_item)
        {
            if (!(a_item.Downloading || (a_item.Finished && !a_item.Error)))
            {
                a_item.Initialize();
                a_item.Downloading = true;

                Task task = new Task(() =>
                {
                    try
                    {
                        a_item.ServerInfo.DownloadSeries((progress) =>
                        {
                            a_item.SetProgress(progress);

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
            }
        }

        public static void DownloadChapters(SerieItem a_item)
        {
            if (!(a_item.Downloading || (a_item.Finished && !a_item.Error)))
            {
                a_item.Initialize();
                a_item.Downloading = true;

                Task task = new Task(() =>
                {
                    try
                    {
                        a_item.SerieInfo.DownloadChapters((progress) =>
                        {
                            a_item.SetProgress(progress);

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
            }
        }

        public static void DownloadPages(IEnumerable<ChapterItem> a_items, string a_baseDir)
        {
            try
            {
                new DirectoryInfo(a_baseDir);
            }
            catch
            {
                MessageBox.Show(String.Format("Directory path is invalid: '{0}'.", a_baseDir),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                new DirectoryInfo(a_baseDir).Create();
            }
            catch
            {
                MessageBox.Show(String.Format("Can't create directory: '{0}'.", a_baseDir),
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

                            string dir = chapter_item.GetImageDirectory(a_baseDir);

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
                TryInvoke(() => ServersChanged());
        }

        public static void OnSeriesChanged()
        {
            if (ServersChanged != null)
                TryInvoke(() => SeriesChanged());
        }

        public static void OnChaptersChanged()
        {
            if (ServersChanged != null)
                TryInvoke(() => ChaptersChanged());
        }

        public static bool DownloadingPages
        {
            get
            {
                return Chapters.Values.Any(chi => chi.Downloading);
            }
        }

        public static List<ChapterItem> Tasks
        {
            get
            {
                List<ChapterItem> list = new List<ChapterItem>();

                foreach (var ch in Chapters.Values)
                {
                    if (ch.Waiting || (ch.Finished && ch.Error) || ch.Downloading)
                        list.Add(ch);
                }

                return list;
            }
        }
    }
}
