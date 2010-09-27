using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Diagnostics;
using MangaCrawlerLib;
using System.Threading.Tasks;
using System.Reflection;

namespace MangaCrawler
{
    // TODO: wiecej kolorow dla statusow, downloading, error, downloaded
    // TODO: test rozlegly zrobic
    // TODO: cache, ladowanie w cachu, update w tle
    // TODO: bookmarks
    // TODO: wykrywanie zmian w obserwowanych seriach
    // TODO: pobieranie jako archiwum
    // TODO: wpf, silverlight, telefony
    // TODO: wbudowany browser
    // TODO: widok wspolny dla wszystkich serwisow, scalac jakos serie, wykrywac zmiany ? gdzie najlepsza jakosc, gdzie duplikaty

    public partial class MangaCrawlerForm : Form
    {
        public ReprioritizableTaskScheduler m_scheduler = new ReprioritizableTaskScheduler();
        public List<ChapterItem> m_chapters = new List<ChapterItem>();//TODO: bindinglist
        public List<SerieItem> m_series = new List<SerieItem>();
        public List<ServerItem> m_servers = new List<ServerItem>();

        public MangaCrawlerForm()
        {
            InitializeComponent();
        }

        public SerieItem SelectedSerie
        {
            get
            {
                return (SerieItem)seriesListBox.SelectedItem;
            }
        }

        public ServerItem SelectedServer
        {
            get
            {
                return (ServerItem)serversListBox.SelectedItem;
            }
        }

        public ChapterInfo SelectedChapter
        {
            get
            {
                return ((ChapterItem)chaptersListBox.SelectedItem).ChapterInfo;
            }
        }

        private void directoryChooseButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = directoryChooseButton.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                directoryPathTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        private void directoryTextBox_TextChanged(object sender, EventArgs e)
        {
            if (directoryPathTextBox.Text.EndsWith(Path.DirectorySeparatorChar.ToString()))
                directoryPathTextBox.Text = directoryPathTextBox.Text.Remove(directoryPathTextBox.Text.Length - 1);

            Settings.Instance.DirectoryPath = directoryPathTextBox.Text;
        }

        private void MangaShareCrawlerForm_Load(object sender, EventArgs e)
        {
            directoryPathTextBox.Text = Settings.Instance.DirectoryPath;

            m_servers = (from server in ServerInfo.ServersInfos
                         select new ServerItem(server)).ToList();

            serversListBox.Items.AddRange(m_servers.ToArray());

            seriesFilterTextBox.Text = Settings.Instance.SeriesFilter;

            splitContainer.SplitterDistance = Settings.Instance.SplitterDistance;

            tasksGridView.AutoGenerateColumns = false;

        }

        private void UpdateSeries(ServerItem a_info = null)
        {
            if (IsDisposed)
                return;

            serversListBox.RefreshItems();

            if (SelectedServer == null)
                return;
            if ((a_info != null) && (SelectedServer != a_info))
                return;
            if (SelectedServer.ServerInfo.Series == null)
                return;

            foreach (var serie in SelectedServer.ServerInfo.Series)
            {
                if (m_series.Any(s => s.SerieInfo == serie))
                    continue;

                m_series.Add(new SerieItem(serie));
            }

            var filtered_series = from si in m_series
                                  from serie in SelectedServer.ServerInfo.Series
                                  where si.SerieInfo == serie
                                  where serie.Name.ToLower().IndexOf(seriesFilterTextBox.Text.ToLower()) != -1
                                  select si;

            seriesListBox.ReloadItems(filtered_series);
        }

        private void serversListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            seriesListBox.Items.Clear();
            chaptersListBox.Items.Clear();

            if (SelectedServer == null)
                return;

            ServerItem server = SelectedServer;

            if (server.Downloading || (server.Finished && !server.Error))
                UpdateSeries();
            else
            {
                server.Initialize();
                server.Downloading = true;

                Task task = new Task(() => 
                {
                    try
                    {
                        server.ServerInfo.DownloadSeries((progress) => 
                        {
                            server.SetProgress(progress);

                            TryInvoke(() =>
                            {
                                UpdateSeries(server);
                            });
                        });
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (Exception)
                    {
                        TryInvoke(() =>
                        {
                            server.Error = true;
                        });
                    }

                    server.Downloading = false;
                    server.Finished = true;

                    TryInvoke(() =>
                    {
                        UpdateSeries(server);
                    });
                });

                
                task.Start(m_scheduler);
                
                UpdateSeries();
            }
        }

        private void seriesFilterTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.SeriesFilter = seriesFilterTextBox.Text;

            UpdateSeries();
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            chaptersListBox.Items.Clear();

            if (SelectedSerie == null)
                return;

            SerieItem serie = SelectedSerie;

            if (serie.Downloading || (serie.Finished && !serie.Error))
                UpdateChapters();
            else
            {
                serie.Initialize();
                serie.Downloading = true;

                Task task = new Task(() => 
                {
                    try
                    {
                        serie.SerieInfo.DownloadChapters((progress) => 
                        {
                            serie.SetProgress(progress);

                            TryInvoke(() =>
                            {
                                UpdateChapters(serie);
                            });
                        });
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (Exception)
                    {
                        serie.Error = true;
                        
                    }

                    serie.Downloading = false;
                    serie.Finished = true;

                    TryInvoke(() =>
                    {
                        UpdateChapters();
                    });

                });

                task.Start(m_scheduler);
                m_scheduler.Prioritize(task);

                UpdateChapters();
            }
        }

        private void UpdateChapters(SerieItem a_serie = null)
        {
            if (IsDisposed)
                return;

            seriesListBox.RefreshItems();

            if (SelectedSerie == null)
                return;
            if ((a_serie != null) && (a_serie != SelectedSerie))
                return;
            if (SelectedSerie.SerieInfo.Chapters == null)
                return;

            foreach (var ch in SelectedSerie.SerieInfo.Chapters)
            {
                if (m_chapters.Any(chi => chi.ChapterInfo == ch))
                    continue;
                m_chapters.Add(new ChapterItem(ch));
            }

            var list = from chi in m_chapters
                       from ch in SelectedSerie.SerieInfo.Chapters
                       where chi.ChapterInfo == ch
                       select chi;

            chaptersListBox.ReloadItems(list);
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            DownloadSelectedChapters();
        }

        private void DownloadSelectedChapters()
        {
            if (chaptersListBox.SelectedItems.Count == 0)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            try
            {
                new DirectoryInfo(Settings.Instance.DirectoryPath);
            }
            catch
            {
                MessageBox.Show(String.Format("Directory path is invalid: '{0}'.", Settings.Instance.DirectoryPath), 
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                new DirectoryInfo(Settings.Instance.DirectoryPath).Create();
            }
            catch
            {
                MessageBox.Show(String.Format("Can't create directory: '{0}'.", Settings.Instance.DirectoryPath), 
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (var item in chaptersListBox.SelectedItems)
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

                        TryInvoke(() =>
                        {
                            UpdateTasks();
                        });

                        return;
                    }

                    try
                    {
                        try
                        {
                            chapter_item.Token.ThrowIfCancellationRequested();

                            chapter_item.Waiting = false;
                            chapter_item.Downloading = true;

                            TryInvoke(() =>
                            {
                                UpdateTasks();
                            });

                            chapter_item.ChapterInfo.DownloadPages(chapter_item.Token);

                            chapter_item.Token.ThrowIfCancellationRequested();

                            string dir = chapter_item.GetImageDirectory(Settings.Instance.DirectoryPath);

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

                                TryInvoke(() =>
                                {
                                    UpdateTasks();
                                });
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

                    TryInvoke(() =>
                    {
                        UpdateTasks();
                    });

                }, chapter_item.Token);

                task.Start(m_scheduler);
            }

            UpdateTasks();
        }

        private void UpdateTasks()
        {
            BindingList<ChapterItem> list;

            if (tasksGridView.DataSource == null)
                list = new BindingList<ChapterItem>();
            else
                list = (BindingList<ChapterItem>)tasksGridView.DataSource;

            foreach (var ch in m_chapters)
            {
                if (ch.Waiting || (ch.Finished && ch.Error) || ch.Downloading)
                {
                    if (!list.Contains(ch))
                        list.Add(ch);
                }
                else
                    list.Remove(ch);
            }

            if (tasksGridView.DataSource == null)
                tasksGridView.DataSource = list;
            else
                list.ResetBindings();

            UpdateChapters();
        }

        private void TryInvoke(Action a_action)
        {
            try
            {
                BeginInvoke(a_action);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void MangaCrawlerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var c in m_chapters)
                c.Delete();
        }

        private void MangaCrawlerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_chapters.Any(chi => chi.Downloading))
            {
                if ((e.CloseReason != CloseReason.WindowsShutDown) || (e.CloseReason != CloseReason.TaskManagerClosing))
                {
                    if (MessageBox.Show("Downloading in progress. Exit anyway ?",
                            Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void directoryPathTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.DirectoryPath = directoryPathTextBox.Text;
            directoryPathTextBox.Text = Settings.Instance.DirectoryPath;
        }

        private void chaptersListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                DownloadSelectedChapters();

            if ((e.KeyCode == Keys.A) && (e.Control))
                chaptersListBox.SelectAll();
        }

        private void chaptersListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DownloadSelectedChapters();
        }

        private void serverURLButton_Click(object sender, EventArgs e)
        {
            if (SelectedServer != null)
                System.Diagnostics.Process.Start(SelectedServer.ServerInfo.URL);
            else
                System.Media.SystemSounds.Beep.Play();
        }

        private void seriesURLButton_Click(object sender, EventArgs e)
        {
            if (SelectedSerie != null)
                System.Diagnostics.Process.Start(SelectedSerie.SerieInfo.URL);
            else
                System.Media.SystemSounds.Beep.Play();
        }

        private void chapterURLButton_Click(object sender, EventArgs e)
        {
            if (SelectedChapter != null)
                System.Diagnostics.Process.Start(SelectedChapter.URL);
            else
                System.Media.SystemSounds.Beep.Play();
        }

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Settings.Instance.SplitterDistance = splitContainer.SplitterDistance;
        }

        private void chaptersListBox_DoubleClick(object sender, EventArgs e)
        {
            DownloadSelectedChapters();
        }

        private void tasksGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == 0) && (e.RowIndex >= 0))
            {
                BindingList<ChapterItem> list = (BindingList<ChapterItem>)tasksGridView.DataSource;
                list[e.RowIndex].Delete();
                UpdateTasks();
            }
        }
    }
}
