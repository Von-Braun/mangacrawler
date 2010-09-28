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
    // TODO: uzupelnic testy o testy rownoleglosci
    // TODO: test rozlegly zrobic
    // TODO: wiecej kolorow dla statusow, downloading, error, downloaded
    //
    // TODO: cache, ladowanie w cachu, update w tle, pamietanie co sie sciaglo, jakie hashe, podczas ponownego uruchomienia weryfikacja tego, 
    //       pamietanie urli obrazkow, dat modyfikacji zdalnych, szybka weryfikacja
    //
    // TODO: bookmarks,
    // TODO: wykrywanie zmian w obserwowanych seriach, praca w tle
    //
    // TODO: pobieranie jako archiwum
    //
    // TODO: wpf, silverlight, telefony
    //
    // TODO: wbudowany browser
    //
    // TODO: widok wspolny dla wszystkich serwisow, scalac jakos serie, wykrywac zmiany ? gdzie najlepsza jakosc, gdzie duplikaty

    public partial class MangaCrawlerForm : Form
    {
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
            tasksGridView.AutoGenerateColumns = false;

            DownloadManager.Form = this;
            DownloadManager.ChaptersChanged += () => UpdateChapters();
            DownloadManager.SeriesChanged += () => UpdateSeries();
            DownloadManager.ServersChanged += () => UpdateServers();

            directoryPathTextBox.Text = Settings.Instance.DirectoryPath;

            UpdateServers();

            seriesFilterTextBox.Text = Settings.Instance.SeriesFilter;

            splitContainer.SplitterDistance = Settings.Instance.SplitterDistance;
        }

        private void UpdateSeries()
        {
            if (IsDisposed)
                return;

            List<SerieItem> list = new List<SerieItem>();

            if ((SelectedServer != null) && (SelectedServer.ServerInfo.Series != null))
            {
                list = (from serie in SelectedServer.ServerInfo.Series
                        where serie.Name.ToLower().IndexOf(seriesFilterTextBox.Text.ToLower()) != -1
                        select DownloadManager.Series[serie]).ToList();
            }

            seriesListBox.ReloadItems(list);

            UpdateChapters();
        }

        private void UpdateServers()
        {
            serversListBox.ReloadItems(DownloadManager.Servers);
            UpdateSeries();
        }

        private void serversListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Object.ReferenceEquals(serversListBox.SelectedItem, SelectedServer))
                return;
            SelectedServer = (ServerItem)serversListBox.SelectedItem;

            if (SelectedServer != null)
                DownloadManager.DownloadSeries(SelectedServer);
            else
                UpdateSeries();
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
                DownloadManager.DownloadChapters(SelectedSerie);
            else
                UpdateChapters();
        }

        private void UpdateChapters()
        {
            if (IsDisposed)
                return;

            List<ChapterItem> list = new List<ChapterItem>();

            if ((SelectedSerie != null) && (SelectedSerie.SerieInfo.Chapters != null))
            {
                list = (from ch in SelectedSerie.SerieInfo.Chapters
                        select DownloadManager.Chapters[ch]).ToList();
            }

            chaptersListBox.ReloadItems(list);

            UpdateTasks();
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

            DownloadManager.DownloadPages(chaptersListBox.SelectedItems.Cast<ChapterItem>(), Settings.Instance.DirectoryPath);
        }

        private void UpdateTasks()
        {
            BindingList<ChapterItem> list;

            if (tasksGridView.DataSource == null)
                list = new BindingList<ChapterItem>();
            else
                list = (BindingList<ChapterItem>)tasksGridView.DataSource;

            var add = (from task in DownloadManager.Tasks
                       where !list.Contains(task)
                       select task).ToList();

            var remove = (from task in list
                          where !DownloadManager.Tasks.Contains(task)
                          select task).ToList();

            foreach (var el in add)
                list.Add(el);
            foreach (var el in remove)
                list.Remove(el);

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
        }

        private void MangaCrawlerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DownloadManager.DownloadingPages)
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
    }
}
