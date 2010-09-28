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
    // TODO: cache, ladowanie w cachu, update w tle, pamietanie co sie sciaglo, jakie hashe, podczas ponownego uruchomienia weryfikacja tego, 
    //       pamietanie urli obrazkow, dat modyfikacji zdalnych, szybka weryfikacja
    // TODO: bookmarks,
    // TODO: wykrywanie zmian w obserwowanych seriach, praca w tle
    // TODO: pobieranie jako archiwum
    // TODO: wpf, silverlight, telefony
    // TODO: wbudowany browser
    // TODO: widok wspolny dla wszystkich serwisow, scalac jakos serie, wykrywac zmiany ? gdzie najlepsza jakosc, gdzie duplikaty

    public partial class MangaCrawlerForm : Form
    {
        public ReprioritizableTaskScheduler m_scheduler = new ReprioritizableTaskScheduler();

        public Dictionary<ChapterInfo, ChapterItem> m_chapters = new Dictionary<ChapterInfo, ChapterItem>();
        public Dictionary<SerieInfo, SerieItem> m_series = new Dictionary<SerieInfo, SerieItem>();
        public BindingList<ServerItem> m_servers = new BindingList<ServerItem>();

        public SerieItem SelectedSerie;
        public ServerItem SelectedServer;
        public ChapterItem SelectedChapter;

        public MangaCrawlerForm()
        {
            InitializeComponent();
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

            foreach (var server in ServerInfo.ServersInfos)
                m_servers.Add(new ServerItem(server));

            UpdateServers();

            seriesFilterTextBox.Text = Settings.Instance.SeriesFilter;

            splitContainer.SplitterDistance = Settings.Instance.SplitterDistance;

            tasksGridView.AutoGenerateColumns = false;
        }

        private void UpdateSeries()
        {
            if (IsDisposed)
                return;

            List<SerieItem> list = new List<SerieItem>();

            if ((SelectedServer != null) && (SelectedServer.ServerInfo.Series != null))
            {
                foreach (var serie in SelectedServer.ServerInfo.Series)
                {
                    if (m_series.ContainsKey(serie))
                        continue;

                    m_series[serie] = new SerieItem(serie);
                }

                list = (from serie in SelectedServer.ServerInfo.Series
                        where serie.Name.ToLower().IndexOf(seriesFilterTextBox.Text.ToLower()) != -1
                        select m_series[serie]).ToList();
            }

            seriesListBox.ReloadItems(list);

            UpdateChapters();
        }

        private void UpdateServers()
        {
            serversListBox.ReloadItems(m_servers);
            UpdateSeries();
        }

        private void serversListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Object.ReferenceEquals(serversListBox.SelectedItem, SelectedServer))
                return;
            SelectedServer = (ServerItem)serversListBox.SelectedItem;

            if (SelectedServer != null)
            {
                if (!(SelectedServer.Downloading || (SelectedServer.Finished && !SelectedServer.Error)))
                {
                    SelectedServer.Initialize();
                    SelectedServer.Downloading = true;

                    Task task = new Task((s) =>
                    {
                        ServerItem server = (ServerItem)s;

                        try
                        {
                            server.ServerInfo.DownloadSeries((progress) =>
                            {
                                server.SetProgress(progress);

                                TryInvoke(() =>
                                {
                                    UpdateServers();
                                });
                            });
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                        catch (Exception)
                        {
                            server.Error = true;
                        }

                        server.Downloading = false;
                        server.Finished = true;

                        TryInvoke(() =>
                        {
                            UpdateServers();
                        });
                    }, SelectedServer);

                    task.Start(m_scheduler);
                }
            }

            UpdateServers();
            seriesListBox.SelectedIndex = -1;
        }

        private void seriesFilterTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.SeriesFilter = seriesFilterTextBox.Text;

            UpdateSeries();
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Object.ReferenceEquals(seriesListBox.SelectedItem, SelectedSerie))
                return;
            SelectedSerie = (SerieItem)seriesListBox.SelectedItem;

            if (SelectedSerie != null)
            {
                if (!(SelectedSerie.Downloading || (SelectedSerie.Finished && !SelectedSerie.Error)))
                {
                    SelectedSerie.Initialize();
                    SelectedSerie.Downloading = true;

                    Task task = new Task((s) =>
                    {
                        SerieItem serie = (SerieItem)s;
                        try
                        {
                            serie.SerieInfo.DownloadChapters((progress) =>
                            {
                                serie.SetProgress(progress);

                                TryInvoke(() =>
                                {
                                    UpdateSeries();
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
                            UpdateSeries();
                        });

                    }, SelectedSerie);

                    task.Start(m_scheduler);
                    m_scheduler.Prioritize(task);
                }
            }

            UpdateSeries();
            chaptersListBox.SelectedIndex = -1;
        }

        private void UpdateChapters()
        {
            if (IsDisposed)
                return;

            List<ChapterItem> list = new List<ChapterItem>();

            if ((SelectedSerie != null) && (SelectedSerie.SerieInfo.Chapters != null))
            {
                foreach (var ch in SelectedSerie.SerieInfo.Chapters)
                {
                    if (m_chapters.ContainsKey(ch))
                        continue;

                    m_chapters[ch] = new ChapterItem(ch);
                }

                list = (from ch in SelectedSerie.SerieInfo.Chapters
                        select m_chapters[ch]).ToList();
            }

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

            foreach (var ch in m_chapters.Values)
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
            {
                if (list.Count == 0)
                    list.ResetBindings();
                else
                {
                    for (int i = 0; i < list.Count; i++)
                        list.ResetItem(i);
                }
            }

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
            foreach (var c in m_chapters.Values)
                c.Delete();
        }

        private void MangaCrawlerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_chapters.Values.Any(chi => chi.Downloading))
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
                System.Diagnostics.Process.Start(SelectedChapter.ChapterInfo.URL);
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

        private void chaptersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Object.ReferenceEquals(SelectedChapter, chaptersListBox.SelectedItem))
                return;

            SelectedChapter = (ChapterItem)chaptersListBox.SelectedItem;
        }

        class TestItem
        {
            public int V;
            private static readonly Random r = new Random();

            public TestItem()
            {
                V = r.Next(1000000);
            }

            public override string ToString()
            {
                return V.ToString() + V.ToString() + V.ToString() + V.ToString() + V.ToString() + 
                V.ToString() + V.ToString() + V.ToString() + V.ToString() + V.ToString();
            }
        }
    }
}
