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
    // seleckaj chapter i serie ma pamietac stan - selekcja i top index, scrollbar horz itp
    // podczas selekecji i ladowania rozleglego serie lub server cos sie dzieje nie tak

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
        public MangaCrawlerForm()
        {
            InitializeComponent();
        }

        private void MangaShareCrawlerForm_Load(object sender, EventArgs e)
        {
            tasksGridView.AutoGenerateColumns = false;
            tasksGridView.DataSource = new BindingList<ChapterItem>();

            DownloadManager.Form = this;
            DownloadManager.ChaptersChanged = (chapters, selection) => UpdateChapters(chapters, selection);
            DownloadManager.SeriesChanged = (series, selection) => UpdateSeries(series, selection);
            DownloadManager.ServersChanged = (servers) => UpdateServers(servers);
            DownloadManager.GetSeriesFilter = () => seriesFilterTextBox.Text;
            DownloadManager.GetDirectoryPath = () => Settings.Instance.DirectoryPath;
            DownloadManager.TasksChanged = (tasks) => UpdateTasks(tasks);

            directoryPathTextBox.Text = Settings.Instance.DirectoryPath;

            DownloadManager.OnServersChanged();

            seriesFilterTextBox.Text = Settings.Instance.SeriesFilter;

            splitContainer.SplitterDistance = Settings.Instance.SplitterDistance;
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

        private void serversListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.SelectedServerSerieListBoxState = new ListBoxState(seriesListBox);
            DownloadManager.SelectedServer = (ServerItem)serversListBox.SelectedItem;
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.SelectedSerieChapterListBoxState = new ListBoxState(chaptersListBox);
            DownloadManager.SelectedSerie = (SerieItem)seriesListBox.SelectedItem;
        }

        private void chaptersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.SelectedChapter = (ChapterItem)chaptersListBox.SelectedItem;
        }

        private void seriesFilterTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.SeriesFilter = seriesFilterTextBox.Text;
            DownloadManager.OnSeriesChanged();
        }

        private void UpdateServers(IList<ServerItem> a_servers)
        {
            serversListBox.ReloadItems(a_servers);
        }

        private void UpdateSeries(IList<SerieItem> a_series, ListBoxState a_state)
        {
            seriesListBox.ReloadItems(a_series, a_state);
        }

        private void UpdateChapters(IList<ChapterItem> a_chapters, ListBoxState a_state)
        {
            chaptersListBox.ReloadItems(a_chapters, a_state);
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            DownloadSelectedChapters();
        }

        private void DownloadSelectedChapters()
        {
            if (chaptersListBox.SelectedItems.Count == 0)
                System.Media.SystemSounds.Beep.Play();
            else
                DownloadManager.DownloadPages(chaptersListBox.SelectedItems.Cast<ChapterItem>());
        }

        private void UpdateTasks(IList<ChapterItem> a_tasks)
        {
            BindingList<ChapterItem> list = (BindingList<ChapterItem>)tasksGridView.DataSource;

            var add = (from task in a_tasks
                       where !list.Contains(task)
                       select task).ToList();

            var remove = (from task in list
                          where !a_tasks.Contains(task)
                          select task).ToList();

            foreach (var el in add)
                list.Add(el);
            foreach (var el in remove)
                list.Remove(el);

            tasksGridView.Invalidate();
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
            if (DownloadManager.SelectedServer != null)
                System.Diagnostics.Process.Start(DownloadManager.SelectedServer.ServerInfo.URL);
            else
                System.Media.SystemSounds.Beep.Play();
        }

        private void seriesURLButton_Click(object sender, EventArgs e)
        {
            if (DownloadManager.SelectedSerie != null)
                System.Diagnostics.Process.Start(DownloadManager.SelectedSerie.SerieInfo.URL);
            else
                System.Media.SystemSounds.Beep.Play();
        }

        private void chapterURLButton_Click(object sender, EventArgs e)
        {
            if (DownloadManager.SelectedChapter != null)
                System.Diagnostics.Process.Start(DownloadManager.SelectedChapter.ChapterInfo.URL);
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
                DownloadManager.DeleteTask(list[e.RowIndex]);
            }
        }
    }
}
