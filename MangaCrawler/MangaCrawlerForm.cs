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

namespace MangaCrawler
{
    public partial class MangaCrawlerForm : Form
    {
        class ProgressData
        {
            public string Text = "";
            public Color Color = Color.Black;
        }

        public List<QueueChapter> m_queue = new List<QueueChapter>();
        public AutoResetEvent m_close_event = new AutoResetEvent(false);

        public SerieInfo SelectedSerie
        {
            get
            {
                return (SerieInfo)seriesListBox.SelectedItem;
            }
        }

        public ServerInfo SelectedServer
        {
            get
            {
                return (ServerInfo)serversListBox.SelectedItem;
            }
        }

        public ChapterInfo SelectedChapter
        {
            get
            {
                return (ChapterInfo)chaptersListBox.SelectedItem;
            }
        }

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
            serversListBox.Items.AddRange(ServerInfo.ServerInfos.ToArray());
            seriesFilterTextBox.Text = Settings.Instance.SeriesFilter;

            splitContainer.SplitterDistance = Settings.Instance.SplitterDistance;
            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (IsDisposed)
                return;

            ProgressData pd = (ProgressData)e.UserState;

            UpdateQueue();

            chaptersListBox.RefreshItems();

            logRichTextBox.SelectionStart = logRichTextBox.TextLength;
            logRichTextBox.ScrollToCaret();

            logRichTextBox.SelectionColor = pd.Color;
            logRichTextBox.SelectedText = pd.Text + System.Environment.NewLine;
        }

        private void UpdateSeries(ServerInfo a_info = null)
        {
            if (IsDisposed)
                return;

            serversListBox.RefreshItems();

            if (SelectedServer == null)
                return;
            if ((a_info != null) && (SelectedServer != a_info))
                return;
            if (SelectedServer.Series == null)
                return;

            var filtered_series = (from serie in SelectedServer.Series
                                  where serie.Name.ToLower().IndexOf(seriesFilterTextBox.Text.ToLower()) != -1
                                  select serie).ToList();

            seriesListBox.ReloadItems(filtered_series);
        }

        private void serversListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            seriesListBox.Items.Clear();
            chaptersListBox.Items.Clear();

            if (SelectedServer == null)
                return;

            if (SelectedServer.DownloadingSeries)
                UpdateSeries();
            else if (SelectedServer.Series != null)
                UpdateSeries();
            else
            {
                System.Threading.Tasks.Task.Factory.StartNew((server) =>
                {
                    ServerInfo si = (ServerInfo)server;

                    try
                    {
                        si.DownloadSeries(() => Invoke((Action)(() =>
                        {
                            UpdateSeries(si);
                        })));
                    }
                    catch (Exception)
                    {
                        Invoke((Action)(() =>
                            {
                                MessageBox.Show("Downloading series from server '" + si.Name + "' failed.", Application.ProductName,
                                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                                UpdateSeries();
                            }));
                    }
                }, SelectedServer);

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

            if (SelectedSerie.DownloadingChapters)
                UpdateChapters();
            else if (SelectedSerie.Chapters!= null)
                UpdateChapters();
            else
            {
                System.Threading.Tasks.Task.Factory.StartNew((serie) =>
                {
                    SerieInfo si = (SerieInfo)serie;

                    try
                    {
                        si.DownloadChapters(() => Invoke((Action)(() =>
                        {
                            UpdateChapters(si);
                        })));
                    }
                    catch (Exception)
                    {
                        Invoke((Action)(() =>
                        {
                            MessageBox.Show("Downloading chapters for series '" + SelectedSerie.Name + "' from server '" + 
                                            SelectedServer.Name + "' failed.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            UpdateChapters();
                        }));
                    }
                }, SelectedSerie);

                UpdateChapters();
            }
        }

        private void UpdateChapters(SerieInfo a_serie = null)
        {
            if (IsDisposed)
                return;

            seriesListBox.RefreshItems();

            if (SelectedSerie == null)
                return;
            if ((a_serie != null) && (a_serie != SelectedSerie))
                return;
            if (SelectedSerie.Chapters == null)
                return;

            chaptersListBox.ReloadItems(SelectedSerie.Chapters);
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            AddChaptersToQueue();
        }

        private void AddChaptersToQueue()
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
                MessageBox.Show("Directory path is invalid.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                new DirectoryInfo(Settings.Instance.DirectoryPath).Create();
            }
            catch
            {
                MessageBox.Show("Cannot create directory.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lock (m_queue)
            {
                foreach (var item in chaptersListBox.SelectedItems)
                {
                    ChapterInfo cs = (ChapterInfo)item;

                    cs.Queue = true;

                    QueueChapter q = new QueueChapter()
                    {
                        ChapterInfo = cs,
                        DirectoryBase = Settings.Instance.DirectoryPath
                    };

                    m_queue.Add(q);
                }
            }

            UpdateQueue();
        }

        private void UpdateQueue()
        {
            if (IsDisposed)
                return;

            lock (m_queue)
            {
                queueListBox.ReloadItems(m_queue);
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (!backgroundWorker.CancellationPending)
                {
                    QueueChapter qc;

                    try
                    {
                        lock (m_queue)
                        {
                            if (m_queue.Count != 0)
                                qc = m_queue[0];
                            else
                                qc = null;
                        }

                        if (qc == null)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    qc.Processing = true;

                    try
                    {
                        qc.ChapterInfo.Queue = false;
                        qc.ChapterInfo.DownloadedPages = 0;
                        qc.ChapterInfo.Downloading = true;

                        backgroundWorker.ReportProgress(0, new ProgressData()
                        {
                            Text = qc.ToString(),
                            Color = Color.Blue
                        });

                        if (new DirectoryInfo(qc.Directory).Exists)
                            new DirectoryInfo(qc.Directory).Delete(true);

                        foreach (var page in qc.ChapterInfo.Pages)
                        {
                            backgroundWorker.ReportProgress(0, new ProgressData()
                            {
                                Text = "\t" + page.ToString()
                            });

                            qc.DownloadAndSavePageImage(page);

                            if (backgroundWorker.CancellationPending)
                                break;

                            if (qc.Deleted)
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);

                        backgroundWorker.ReportProgress(0, new ProgressData()
                        {
                            Text = "ERROR: " + qc.ToString(),
                            Color = Color.Red
                        });

                        Invoke((Action)(() =>
                        {
                            MessageBox.Show(String.Format("Downloading '{0}' failed.", qc.ToString() ), Application.ProductName,
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                            UpdateSeries();
                        }));
                    }

                    qc.ChapterInfo.Downloading = false;
                    qc.Processing = false;

                    lock (m_queue)
                    {
                        qc.Deleted = true;
                        m_queue.Remove(qc);
                    }

                    if (m_queue.Count == 0)
                    {
                        backgroundWorker.ReportProgress(0, new ProgressData()
                        {
                            Text = "Done",
                            Color = Color.Green
                        });
                    }
                }
            }
            finally
            {
                m_close_event.Set();
            }
        }

        private void MangaCrawlerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            backgroundWorker.CancelAsync();

            m_close_event.WaitOne();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (queueListBox.SelectedItems.Count == 0)
                System.Media.SystemSounds.Beep.Play();
            else
            {
                lock (m_queue)
                {
                    foreach (var item in queueListBox.SelectedItems)
                    {
                        QueueChapter qc = (QueueChapter)item;

                        qc.Deleted = true;
                        qc.ChapterInfo.Queue = false;

                        if (!qc.Processing)
                            m_queue.Remove(qc);
                    }
                }

                UpdateQueue();
            }
        }

        private void MangaCrawlerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_queue.Count > 0)
            {
                if ((e.CloseReason != CloseReason.WindowsShutDown) || (e.CloseReason != CloseReason.TaskManagerClosing))
                {
                    if (MessageBox.Show("Downloading in progress. Exit anyway ?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        e.Cancel = true;
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
                AddChaptersToQueue();

            if ((e.KeyCode == Keys.A) && (e.Control))
                chaptersListBox.SelectAll();
        }

        private void chaptersListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            AddChaptersToQueue();
        }

        private void serverURLButton_Click(object sender, EventArgs e)
        {
            if (SelectedServer != null)
                System.Diagnostics.Process.Start(SelectedServer.URL);
            else
                System.Media.SystemSounds.Beep.Play();
        }

        private void seriesURLButton_Click(object sender, EventArgs e)
        {
            if (SelectedSerie != null)
                System.Diagnostics.Process.Start(SelectedSerie.URL);
            else
                System.Media.SystemSounds.Beep.Play();
        }

        private void chapterURLButton_Click(object sender, EventArgs e)
        {
            if (SelectedChapter != null)
                System.Diagnostics.Process.Start(SelectedChapter.URL);
            else
                System.Media.SystemSounds.Beep.Play();

            return;
        }

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Settings.Instance.SplitterDistance = splitContainer.SplitterDistance;
        }

        private void chaptersListBox_DoubleClick(object sender, EventArgs e)
        {
            AddChaptersToQueue();
        }
    }
}
