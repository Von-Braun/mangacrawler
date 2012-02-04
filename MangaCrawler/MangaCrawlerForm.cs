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
using System.Media;
using MangaCrawler.Properties;

namespace MangaCrawler
{
    // TODO: inaczej zrobic interacke z watkiami, przelazywac funcktor do odswiezenia danego elementu, 
    //       najpierw sprawdzic czy owogle jest on widoczny
    // 
    // TODO: wersja to data, ustawiana automatycznie podczas budowania, generowanie jakiegos pliku 
    //       z data.
    // 
    // TODO: zaznaczamy serwer, chapter, serie, przechodzimy na inny serwer, 
    //       filtrujemy poprez xxxx, przechodzimy na poprzedni serwer, przechodzimy na 
    //       inny, zdejmujemy filtr (lub zdejmujemy go na tym z zaznaczeniem - nie pojawia sie ono),
    //       po zdjeciu filtru tez stare zaznaczenie nie dziala.
    //
    // TODO: http://www.mangareader.net/alphabetical
    // TODO: http://www.mangamonger.com/
    //
    // TODO: instalator
    //
    // TODO: cache, ladowanie w cachu, update w tle, pamietanie co sie sciaglo, jakie hashe, 
    //       podczas ponownego uruchomienia 
    //       weryfikacja tego, pamietanie urli obrazkow, dat modyfikacji zdalnych, szybka 
    //       weryfikacja
    //
    // TODO: bookmarks,
    // TODO: wykrywanie zmian w obserwowanych seriach, praca w tle, 
    //
    // TODO: wpf, silverlight, telefony
    //
    // TODO: wbudowany browser
    //
    // TODO: widok wspolny dla wszystkich serwisow, scalac jakos serie, wykrywac zmiany ? 
    //       gdzie najlepsza jakosc, gdzie duplikaty

    public partial class MangaCrawlerForm : Form
    {
        public MangaCrawlerForm()
        {
            InitializeComponent();
        }

        private void MangaShareCrawlerForm_Load(object sender, EventArgs e)
        {
            Text = String.Format("{0} {1}.{2}", Text,
                Assembly.GetAssembly(GetType()).GetName().Version.Major, 
                Assembly.GetAssembly(GetType()).GetName().Version.Minor);

            tasksGridView.AutoGenerateColumns = false;
            tasksGridView.DataSource = new BindingList<ChapterState>();

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
            splitter1.SplitPosition = Settings.Instance.SplitterDistance;
            cbzCheckBox.Checked = Settings.Instance.UseCBZ;

            DownloadManager.UpdateVisuals();

            Task.Factory.StartNew(() => CheckNewVersion());
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
            {
                directoryPathTextBox.Text = directoryPathTextBox.Text.Remove(
                    directoryPathTextBox.Text.Length - 1);
            }

            Settings.Instance.DirectoryPath = directoryPathTextBox.Text;
        }

        private void serversListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.SelectedServerState = (ServerState)serversListBox.SelectedItem;
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.SelectedSerieState = (SerieState)seriesListBox.SelectedItem;
        }

        private void chaptersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.SelectedChapterState = (ChapterState)chaptersListBox.SelectedItem;
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
                MessageBox.Show(String.Format(Resources.DirError1, 
                    Settings.Instance.DirectoryPath),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                new DirectoryInfo(Settings.Instance.DirectoryPath).Create();
            }
            catch
            {
                MessageBox.Show(String.Format(Resources.DirError2, 
                    Settings.Instance.DirectoryPath),
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
                DownloadManager.DownloadPages(chaptersListBox.SelectedItems.Cast<ChapterState>());
        }

        private void UpdateTasks(IEnumerable<ChapterState> a_tasks)
        {
            BindingList<ChapterState> list = (BindingList<ChapterState>)tasksGridView.DataSource;

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
                if ((e.CloseReason != CloseReason.WindowsShutDown) || 
                    (e.CloseReason != CloseReason.TaskManagerClosing))
                {
                    if (MessageBox.Show(Resources.ExitQuestion,
                            Application.ProductName, MessageBoxButtons.YesNo, 
                            MessageBoxIcon.Question) == DialogResult.No)
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
            if (DownloadManager.SelectedServerState != null)
                Process.Start(DownloadManager.SelectedServerState.ServerInfo.URL);
            else
                SystemSounds.Beep.Play();
        }

        private void seriesURLButton_Click(object sender, EventArgs e)
        {
            if (DownloadManager.SelectedSerieState != null)
                Process.Start(DownloadManager.SelectedSerieState.SerieInfo.URL);
            else
                SystemSounds.Beep.Play();
        }

        private void chapterURLButton_Click(object sender, EventArgs e)
        {
            if (DownloadManager.SelectedChapterState != null)
                Process.Start(DownloadManager.SelectedChapterState.ChapterInfo.URL);
            else
                SystemSounds.Beep.Play();
        }

        private void chaptersListBox_DoubleClick(object sender, EventArgs e)
        {
            DownloadSelectedChapters();
        }

        private void tasksGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == 0) && (e.RowIndex >= 0))
            {
                BindingList<ChapterState> list = (BindingList<ChapterState>)tasksGridView.DataSource;
                DownloadManager.DeleteTask(list[e.RowIndex]);
            }
        }

        private void seriesListBox_VerticalScroll(object a_sender, bool a_tracking)
        {
            DownloadManager.SeriesVisualState = DownloadManager.GetSeriesVisualState();
        }

        private void chaptersListBox_VerticalScroll(object a_sender, bool a_tracking)
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
            Rectangle bounds = new Rectangle(e.Bounds.X, e.Bounds.Y + 
                (e.Bounds.Height - size.ToSize().Height) / 2, 
                e.Bounds.Width, size.ToSize().Height);

            e.Graphics.DrawString(a_text, e.Font, Brushes.Black, bounds, 
                StringFormat.GenericDefault);

            int left = (int)Math.Round(size.Width + e.Graphics.MeasureString(" ", e.Font).Width);
            Font font = new Font(e.Font.FontFamily, e.Font.Size * 9 / 10, FontStyle.Bold);
            size = e.Graphics.MeasureString("(ABGHRTW%)", font).ToSize();
            bounds = new Rectangle(left, e.Bounds.Y + 
                (e.Bounds.Height - size.ToSize().Height) / 2 - 1, 
                bounds.Width - left, bounds.Height);

            switch (a_state)
            {
                case ItemState.Error: 

                    e.Graphics.DrawString(Resources.Error, font, 
                        Brushes.Red, bounds, StringFormat.GenericDefault); 
                    break;

                case ItemState.Downloaded: 

                    e.Graphics.DrawString(a_downloaded, font, 
                        Brushes.Green, bounds, StringFormat.GenericDefault); 
                    break;

                case ItemState.DownloadedMissingPages:

                    e.Graphics.DrawString(a_downloaded, font,
                        Brushes.Green, bounds, StringFormat.GenericDefault);
                    break;

                case ItemState.Waiting: 

                    e.Graphics.DrawString(Resources.Waiting, font, 
                        Brushes.Blue, bounds, StringFormat.GenericDefault); 
                     break;

                case ItemState.Deleting: 

                    e.Graphics.DrawString(Resources.Deleting, font, 
                        Brushes.Red, bounds, StringFormat.GenericDefault); 
                    break;

                case ItemState.Downloading: 

                    e.Graphics.DrawString(a_downloading, font, 
                        Brushes.Blue, bounds, StringFormat.GenericDefault); 
                    break;

                case ItemState.Zipping: 

                    e.Graphics.DrawString(Resources.Zipping, font, 
                        Brushes.Blue, bounds, StringFormat.GenericDefault); 
                    break;

                case ItemState.Initial: break;

                default: throw new NotImplementedException();
            }

            e.DrawFocusRectangle();
        }

        private void serversListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            ServerState serverState = (ServerState)serversListBox.Items[e.Index];
            ListBox_DrawItem(e, serverState.ServerInfo.Name, serverState.State,
                String.Format("({0}%)", serverState.Progress), String.Format(Resources.Series,
                serverState.ServerInfo.Series.Count()));
        }

        private void seriesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            SerieState serie_state = (SerieState)seriesListBox.Items[e.Index];
            ListBox_DrawItem(e, serie_state.SerieInfo.Name, serie_state.State,
                String.Format("({0}%)", serie_state.Progress), String.Format(Resources.Chapters, 
                serie_state.SerieInfo.Chapters.Count()));
        }

        private void chaptersListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ChapterState chapter_state = (ChapterState)chaptersListBox.Items[e.Index];
            ListBox_DrawItem(e, chapter_state.ChapterInfo.Name, chapter_state.State, 
                String.Format("{0}/{1}", chapter_state.DownloadedPages,
                chapter_state.ChapterInfo.Pages.Count()), (chapter_state.State == ItemState.Downloaded) ? 
                Resources.Downloaded : Resources.DownloadMissingPages);
        }

        private void cbzCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.UseCBZ = cbzCheckBox.Checked;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Resources.HomePage);
        }

        private void CheckNewVersion()
        {
            try
            {
                var doc = new HtmlWeb().Load(Resources.HomePage);
                var node = doc.DocumentNode.SelectSingleNode("//td[@id='ReleaseName']");
                var name = node.InnerText;
                var version1 = Double.Parse(
                    name.Replace("Manga Crawler", "").Trim().Replace(".", ","));
                
                var assembly_version = System.Reflection.Assembly.GetAssembly(
                    typeof(MangaCrawlerForm)).GetName().Version;
                var version2 = Double.Parse(assembly_version.Major.ToString() + "," + 
                    assembly_version.Minor.ToString());
                
                if (version1 > version2)
                {
                    Action action = () => versionLinkLabel.Text = Resources.NewVersion;
                        
                    Invoke(action);

                    Task.Factory.StartNew(() => PulseNewVersion());
                }
            }
            catch
            {
            }
        }

        private IEnumerable<Color> PulseColors
        {
            get
            {
                Color color1 = versionLinkLabel.LinkColor;

                // Parameters.
                Color color2 = Color.Red;
                const int steps = 256;
                const int count = 12;

                // limit to byte
                Func<int, int> limit = 
                    (c) => (c < 0) ? byte.MinValue : (c > 255) ? byte.MaxValue : c;

                // ph=0 return c1 ... ph=255 return c2
                Func<int, int, int, int> calc =
                    (c1, c2, ph) => limit(c1 + (c2 - c1) * ph / 255);

                for (int i = 0; i < count; i++)
                {
                    for (int phase = 0; phase < steps; phase++)
                    {
                        // 0 ... steps/2 ... 0
                        var p = steps / 2 - Math.Abs(phase - steps / 2);

                        // 0 ... 2*pi ... 0
                        var pp = p * 2 * Math.PI / (steps / 2);

                        // 1 ... -1 ... 1
                        pp = Math.Cos(pp);

                        // 0 ... 1 ... 0
                        pp = (2 - (pp + 1)) / 2;

                        // 0 ... 255 ... 0 
                        p = limit((int)(Math.Round(pp * 255)));

                        var r = calc(color1.R, color2.R, p);
                        var g = calc(color1.G, color2.G, p);
                        var b = calc(color1.B, color2.B, p);
                        yield return Color.FromArgb(r, g, b);
                    }
                }
            }
        }

        private void PulseNewVersion()
        {
            try
            {
                foreach (var color in PulseColors)
                {
                    Action action = () => versionLinkLabel.LinkColor = color;
                    Invoke(action);
                    Thread.Sleep(4);
                }
            }
            catch
            {
            }
        }

        private void splitter1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (panel4.Width - splitter1.SplitPosition < panel3.MinimumSize.Width)
                splitter1.SplitPosition = panel4.Width - panel3.MinimumSize.Width;
            Settings.Instance.SplitterDistance = splitter1.SplitPosition;
        }

        private void MangaCrawlerForm_ResizeEnd(object sender, EventArgs e)
        {
            if (panel3.Bounds.Right > panel4.ClientRectangle.Right)
                splitter1.SplitPosition = panel4.Width - panel3.MinimumSize.Width;
        }
    }
}
