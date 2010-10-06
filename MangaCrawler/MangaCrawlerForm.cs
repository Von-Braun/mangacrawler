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
        private static readonly string ERROR = "ERROR";
        private static readonly string DOWNLOADED = "OK";
        private static readonly string WAITING = "WAITING";
        private static readonly string DELETING = "DELETING";

        public MangaCrawlerForm()
        {
            InitializeComponent();
        }

        private void MangaShareCrawlerForm_Load(object sender, EventArgs e)
        {
            tasksGridView.AutoGenerateColumns = false;
            tasksGridView.DataSource = new BindingList<ChapterItem>();

            DownloadManager.Form = this;

            DownloadManager.TasksChanged += (tasks) => UpdateTasks(tasks);

            DownloadManager.GetSeriesFilter += () => seriesFilterTextBox.Text;
            DownloadManager.GetDirectoryPath += () => Settings.Instance.DirectoryPath;

            DownloadManager.GetServersVisualState += () => new ListBoxVisualState(serversListBox);
            DownloadManager.GetSeriesVisualState += () => new ListBoxVisualState(seriesListBox);
            DownloadManager.GetChaptersVisualState += () => new ListBoxVisualState(chaptersListBox);

            directoryPathTextBox.Text = Settings.Instance.DirectoryPath;

            seriesFilterTextBox.Text = Settings.Instance.SeriesFilter;

            splitContainer.SplitterDistance = Settings.Instance.SplitterDistance;

            DownloadManager.UpdateVisuals();
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
            DownloadManager.SelectedServer = (ServerItem)serversListBox.SelectedItem;
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.SelectedSerie = (SerieItem)seriesListBox.SelectedItem;
        }

        private void chaptersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.SelectedChapter = (ChapterItem)chaptersListBox.SelectedItem;
        }

        private void seriesFilterTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.SeriesFilter = seriesFilterTextBox.Text;
            DownloadManager.UpdateVisuals();
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

        private void UpdateTasks(IEnumerable<ChapterItem> a_tasks)
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

        private void seriesListBox_HorizontalScroll(object a_sender, int a_topIndex, bool a_tracking)
        {
            DownloadManager.SeriesVisualState = DownloadManager.GetSeriesVisualState();
        }

        private void chaptersListBox_HorizontalScroll(object a_sender, int a_topIndex, bool a_tracking)
        {
            DownloadManager.ChaptersVisualState = DownloadManager.GetChaptersVisualState();
        }

        private void ListBox_DrawItem(DrawItemEventArgs e, string a_text, bool a_downloading, 
            string a_progress, bool a_error, bool a_downloaded, bool a_waiting, bool a_deleting)
        {
            e.DrawBackground();

            e.Graphics.DrawString(a_text, e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);

            var size = e.Graphics.MeasureString(a_text, e.Font);
            int left = (int)Math.Round(size.Width + e.Graphics.MeasureString(" ", e.Font).Width);

            Rectangle bounds = new Rectangle(left, e.Bounds.Y, e.Bounds.Width - left, e.Bounds.Height);

            Font font = new Font(e.Font.FontFamily, e.Font.Size * 8 / 10, FontStyle.Bold);

            if (a_error)
                e.Graphics.DrawString(ERROR, font, Brushes.Tomato, bounds, StringFormat.GenericDefault);
            else if (a_downloaded)
                e.Graphics.DrawString(DOWNLOADED, font, Brushes.LightGreen, bounds, StringFormat.GenericDefault);
            else if (a_waiting)
                e.Graphics.DrawString(WAITING, font, Brushes.LightBlue, bounds, StringFormat.GenericDefault);
            else if (a_deleting)
                e.Graphics.DrawString(DELETING, font, Brushes.Tomato, bounds, StringFormat.GenericDefault);
            else if (a_downloading)
            {
                e.Graphics.DrawString(a_progress, font, Brushes.LightBlue, bounds,
                    StringFormat.GenericDefault);
            }

            e.DrawFocusRectangle();
        }

        private void ListBox_MeasureItem(MeasureItemEventArgs e, ListBox a_listBox, string a_text, bool a_downloading,
            string a_progress, bool a_error, bool a_downloaded, bool a_waiting, bool a_deleting)
        {
            e.ItemWidth = (int)Math.Round(e.Graphics.MeasureString(a_text, a_listBox.Font,
                 Int32.MaxValue, StringFormat.GenericDefault).Width);

            Font font = new Font(a_listBox.Font.FontFamily, a_listBox.Font.Size * 8 / 10, FontStyle.Bold);

            if (a_error)
            {
                e.ItemWidth += (int)Math.Round(e.Graphics.MeasureString(" ", a_listBox.Font,
                    Int32.MaxValue, StringFormat.GenericDefault).Width);
                e.ItemWidth += (int)Math.Round(e.Graphics.MeasureString(ERROR, font,
                    Int32.MaxValue, StringFormat.GenericDefault).Width);
            }
            else if (a_downloaded)
            {

                e.ItemWidth += (int)Math.Round(e.Graphics.MeasureString(" ", a_listBox.Font,
                    Int32.MaxValue, StringFormat.GenericDefault).Width);
                e.ItemWidth += (int)Math.Round(e.Graphics.MeasureString(DOWNLOADED, font,
                    Int32.MaxValue, StringFormat.GenericDefault).Width);
            }
            else if (a_downloading)
            {
                e.ItemWidth += (int)Math.Round(e.Graphics.MeasureString(" ", a_listBox.Font,
                    Int32.MaxValue, StringFormat.GenericDefault).Width);
                e.ItemWidth += (int)Math.Round(e.Graphics.MeasureString(a_progress, font,
                    Int32.MaxValue, StringFormat.GenericDefault).Width);
            }
            else if (a_waiting)
            {
                e.ItemWidth += (int)Math.Round(e.Graphics.MeasureString(" ", a_listBox.Font,
                    Int32.MaxValue, StringFormat.GenericDefault).Width);
                e.ItemWidth += (int)Math.Round(e.Graphics.MeasureString(WAITING, font,
                    Int32.MaxValue, StringFormat.GenericDefault).Width);
            }
            else if (a_deleting)
            {
                e.ItemWidth += (int)Math.Round(e.Graphics.MeasureString(" ", a_listBox.Font,
                    Int32.MaxValue, StringFormat.GenericDefault).Width);
                e.ItemWidth += (int)Math.Round(e.Graphics.MeasureString(DELETING, font,
                    Int32.MaxValue, StringFormat.GenericDefault).Width);
            }
        }

        private void serversListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ServerItem server = (ServerItem)serversListBox.Items[e.Index];
            ListBox_DrawItem(e, server.ServerInfo.Name, server.Downloading, 
                String.Format("({0}%)", server.Progress), server.Error, server.Downloaded, false, false);
        }

        private void serversListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            ServerItem server = (ServerItem)serversListBox.Items[e.Index];
            ListBox_MeasureItem(e, serversListBox, server.ServerInfo.Name, server.Downloading, 
                String.Format("({0}%)", server.Progress), server.Error, server.Downloaded, false, false);
        }

        private void seriesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            SerieItem serie = (SerieItem)seriesListBox.Items[e.Index];
            ListBox_DrawItem(e, serie.SerieInfo.Name, serie.Downloading, 
                String.Format("({0}%)", serie.Progress), serie.Error, serie.Downloaded, false, false);
        }

        private void seriesListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            SerieItem serie = (SerieItem)seriesListBox.Items[e.Index];
            ListBox_MeasureItem(e, serversListBox, serie.SerieInfo.Name, serie.Downloading, 
                String.Format("({0}%)", serie.Progress), serie.Error, serie.Downloaded, false, false);
        }

        private void chaptersListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ChapterItem chapter = (ChapterItem)chaptersListBox.Items[e.Index];
            ListBox_DrawItem(e, chapter.ChapterInfo.Name, chapter.Downloading, 
                String.Format("{0}/{1}", chapter.DownloadedPages, 
                (chapter.ChapterInfo.Pages == null) ? 0 : chapter.ChapterInfo.Pages.Count()), 
                chapter.Error, chapter.Downloaded, chapter.Waiting, chapter.Deleting);
        }

        private void chaptersListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            ChapterItem chapter = (ChapterItem)chaptersListBox.Items[e.Index];
            ListBox_MeasureItem(e, serversListBox, chapter.ChapterInfo.Name, chapter.Downloading,
                String.Format("{0}/{1}", chapter.DownloadedPages, 
                (chapter.ChapterInfo.Pages == null) ? 0 : chapter.ChapterInfo.Pages.Count()), 
                chapter.Error, chapter.Downloaded, chapter.Waiting, chapter.Deleting);
        }
    }
}
