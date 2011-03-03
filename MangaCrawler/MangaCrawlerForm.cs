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
using HtmlAgilityPack;

namespace MangaCrawler
{
    // TODO: wybranie chapteru, zmiana filtru tak ze zaznaczenie seri znika, chapter zostaje zaznaczony.
    //
    // TODO: podczas sciagania chapteru, ustawiamy focus na inny wiersz, i przewijamy rolka, 
    //       update ciagle cofa nas na zaznaczenie
    //
    // TODO: test rozlegly zrobic
    // TODO: instalator
    //
    // TODO: cache, ladowanie w cachu, update w tle, pamietanie co sie sciaglo, jakie hashe, podczas ponownego uruchomienia 
    //       weryfikacja tego, pamietanie urli obrazkow, dat modyfikacji zdalnych, szybka weryfikacja
    //
    // TODO: bookmarks,
    // TODO: wykrywanie zmian w obserwowanych seriach, praca w tle, 
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

            DownloadManager.TasksChanged += (tasks) => UpdateTasks(tasks);

            DownloadManager.GetSeriesFilter = () => seriesFilterTextBox.Text;
            DownloadManager.GetDirectoryPath = () => Settings.Instance.DirectoryPath;
            DownloadManager.UseCBZ = () => Settings.Instance.UseCBZ;

            DownloadManager.GetServersVisualState += () => new ListBoxVisualState(serversListBox);
            DownloadManager.GetSeriesVisualState += () => new ListBoxVisualState(seriesListBox); ;
            DownloadManager.GetChaptersVisualState += () => new ListBoxVisualState(chaptersListBox);
            
            directoryPathTextBox.Text = Settings.Instance.DirectoryPath;
            seriesFilterTextBox.Text = Settings.Instance.SeriesFilter;
            splitContainer.SplitterDistance = Settings.Instance.SplitterDistance;
            cbzCheckBox.Checked = Settings.Instance.UseCBZ;

            DownloadManager.UpdateVisuals();

            System.Threading.Tasks.Task.Factory.StartNew(() => CheckNewVersion());
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

        public static bool IsDirectoryPathValid()

        {
            try
            {
                new DirectoryInfo(Settings.Instance.DirectoryPath);
            }
            catch
            {
                MessageBox.Show(String.Format(MangaCrawler.Properties.Resources.DIR_ERROR_1, Settings.Instance.DirectoryPath),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                new DirectoryInfo(Settings.Instance.DirectoryPath).Create();
            }
            catch
            {
                MessageBox.Show(String.Format(MangaCrawler.Properties.Resources.DIR_ERROR_2, Settings.Instance.DirectoryPath),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void DownloadSelectedChapters()
        {
            if (chaptersListBox.SelectedItems.Count == 0)
                System.Media.SystemSounds.Beep.Play();
            else if (IsDirectoryPathValid())
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
                    if (MessageBox.Show(MangaCrawler.Properties.Resources.EXIT_QUESTION,
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

        private void ListBox_DrawItem(DrawItemEventArgs e, string a_text, ItemState a_state, 
            string a_downloading, string a_downloaded)
        {
            e.DrawBackground();

            if (e.State.HasFlag(DrawItemState.Selected))
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);

            var size = e.Graphics.MeasureString(a_text, e.Font);
            Rectangle bounds = new Rectangle(e.Bounds.X, e.Bounds.Y + (e.Bounds.Height - size.ToSize().Height) / 2, 
                e.Bounds.Width, size.ToSize().Height);

            e.Graphics.DrawString(a_text, e.Font, Brushes.Black, bounds, StringFormat.GenericDefault);

            int left = (int)Math.Round(size.Width + e.Graphics.MeasureString(" ", e.Font).Width);
            Font font = new Font(e.Font.FontFamily, e.Font.Size * 9 / 10, FontStyle.Bold);
            size = e.Graphics.MeasureString("(ABGHRTW%)", font).ToSize();
            bounds = new Rectangle(left, e.Bounds.Y + (e.Bounds.Height - size.ToSize().Height) / 2 - 1, 
                bounds.Width - left, bounds.Height);

            switch (a_state)
            {
                case ItemState.Error: 

                    e.Graphics.DrawString(MangaCrawler.Properties.Resources.ERROR, font, 
                        Brushes.Red, bounds, StringFormat.GenericDefault); 
                    break;

                case ItemState.Downloaded: 

                    e.Graphics.DrawString(a_downloaded, font, 
                        Brushes.Green, bounds, StringFormat.GenericDefault); 
                    break;

                case ItemState.Waiting: 

                    e.Graphics.DrawString(MangaCrawler.Properties.Resources.WAITING, font, 
                        Brushes.Blue, bounds, StringFormat.GenericDefault); 
                     break;

                case ItemState.Deleting: 

                    e.Graphics.DrawString(MangaCrawler.Properties.Resources.DELETING, font, 
                        Brushes.Red, bounds, StringFormat.GenericDefault); 
                    break;

                case ItemState.Downloading: 

                    e.Graphics.DrawString(a_downloading, font, 
                        Brushes.Blue, bounds, StringFormat.GenericDefault); 
                    break;

                case ItemState.Zipping: 

                    e.Graphics.DrawString(MangaCrawler.Properties.Resources.ZIPPING, font, 
                        Brushes.Blue, bounds, StringFormat.GenericDefault); 
                    break;

                case ItemState.Initial: break;

                default: throw new NotImplementedException();
            }

            e.DrawFocusRectangle();
        }

        private void serversListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ServerItem server = (ServerItem)serversListBox.Items[e.Index];
            ListBox_DrawItem(e, server.ServerInfo.Name, server.State,
                String.Format("({0}%)", server.Progress), String.Format(MangaCrawler.Properties.Resources.SERIES,
                server.ServerInfo.Series.Count()));
        }

        private void seriesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            SerieItem serie = (SerieItem)seriesListBox.Items[e.Index];
            ListBox_DrawItem(e, serie.SerieInfo.Name, serie.State,
                String.Format("({0}%)", serie.Progress), String.Format(MangaCrawler.Properties.Resources.CHAPTERS, 
                serie.SerieInfo.Chapters.Count()));
        }

        private void chaptersListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ChapterItem chapter = (ChapterItem)chaptersListBox.Items[e.Index];
            ListBox_DrawItem(e, chapter.ChapterInfo.Name, chapter.State, 
                String.Format("{0}/{1}", chapter.DownloadedPages,
                chapter.ChapterInfo.Pages.Count()), MangaCrawler.Properties.Resources.DOWNLOADED);
        }

        private void cbzCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.UseCBZ = cbzCheckBox.Checked;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://mangacrawler.codeplex.com/");
        }

        private void CheckNewVersion()
        {
            try
            {
                var doc = new HtmlWeb().Load("http://mangacrawler.codeplex.com/");
                var node = doc.DocumentNode.SelectSingleNode("//td[@id='ReleaseName']");
                var name = node.InnerText;
                var version1 = Double.Parse(name.Replace("Manga Crawler", "").Trim().Replace(".", ","));

                var assembly_version = System.Reflection.Assembly.GetAssembly(typeof(MangaCrawlerForm)).GetName().Version;
                var version2 = Double.Parse(assembly_version.Major.ToString() + "," + assembly_version.Minor.ToString());

                if (version1 > version2)
                {
                    Action action = () => versionPanel.Visible = true;
                    Invoke(action);
                }
            }
            catch
            {
            }
         }
        
    }
}
