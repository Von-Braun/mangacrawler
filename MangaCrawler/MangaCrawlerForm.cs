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
using log4net;

namespace MangaCrawler
{
    // TODO: dodac nowa karte na ktorej bedzie mozna wykreslic ilosc polaczen ogolnie i na serwer
    // TODO: czy byl jakis pik po wydaniu nowej wersji jesli nie to zmienic wersjonowanie na korzystanie z daty
    // TODO: wersja to data, ustawiana automatycznie podczas budowania, generowanie jakiegos pliku 
    //       z data.
    //
    // TODO: po sciagnieciu nie kasowac taska, dac mozliwosc przejrzenia mangi
    // lib zawsze pamieta taski, to gui je odrzuca
    //
    // TODO: pamietanie taskow podczas zamkniecia
    // 
    // TODO: http://www.mangareader.net/alphabetical
    // TODO: http://mangable.com/manga-list/
    // TODO: http://www.readmangaonline.net/
    // TODO: http://www.anymanga.com/directory/all/
    // TODO: http://manga.animea.net/browse.html
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

            DownloadManager.GetImagesBaseDir = () => Settings.Instance.ImagesBaseDir;
            DownloadManager.UseCBZ = () => Settings.Instance.UseCBZ;
            DownloadManager.GetSeriesVisualState += () => new ListBoxVisualState(seriesListBox);
            DownloadManager.GetChaptersVisualState += () => new ListBoxVisualState(chaptersListBox);
            
            directoryPathTextBox.Text = Settings.Instance.ImagesBaseDir;
            seriesSearchTextBox.Text = Settings.Instance.SeriesFilter;
            splitter1.SplitPosition = Settings.Instance.SplitterDistance;
            cbzCheckBox.Checked = Settings.Instance.UseCBZ;

            DownloadManager.Load(Settings.GetSettingsDir());

            Task.Factory.StartNew(() => CheckNewVersion(),TaskCreationOptions.LongRunning);
#if !DEBUG
            tabControl.TabPages.Remove(logTabPage);
            LogManager.Shutdown();
#endif
            //Flicker-free.
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, tasksGridView, new object[] { true });

            tasksGridView.AutoGenerateColumns = false;
            tasksGridView.DataSource = new BindingList<TaskInfo>();

            UpdateSeriesTab();
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

            Settings.Instance.ImagesBaseDir = directoryPathTextBox.Text;
        }

        private void serversListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.SelectedServer = (ServerInfo)serversListBox.SelectedItem;
            UpdateChapters();
        }

        private ServerInfo SelectedServer
        {
            get
            {
                return (ServerInfo)serversListBox.SelectedItem;
            }
        }

        private SerieInfo SelectedSerie
        {
            get
            {
                return (SerieInfo)seriesListBox.SelectedItem;
            }
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.SelectedSerie = (SerieInfo)seriesListBox.SelectedItem;
            UpdateChapters();
        }

        private void chaptersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.SelectedChapter = (ChapterInfo)chaptersListBox.SelectedItem;
        }

        private void seriesSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.SeriesFilter = seriesSearchTextBox.Text;
            UpdateSeriesTab();
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            DownloadSelectedChapters();
        }

        public static bool IsDirectoryPathValid()

        {
            try
            {
                new DirectoryInfo(Settings.Instance.ImagesBaseDir);
            }
            catch
            {
                MessageBox.Show(String.Format(Resources.DirError1, 
                    Settings.Instance.ImagesBaseDir),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                new DirectoryInfo(Settings.Instance.ImagesBaseDir).Create();
            }
            catch
            {
                MessageBox.Show(String.Format(Resources.DirError2, 
                    Settings.Instance.ImagesBaseDir),
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
                DownloadManager.DownloadPages(chaptersListBox.SelectedItems.Cast<ChapterInfo>());
        }

        private bool ShowingDownloadingTab
        {
            get
            {
                return tabControl.SelectedIndex == 1;
            }
        }

        private bool ShowingSeriesTab
        {
            get
            {
                return tabControl.SelectedIndex == 0;
            }
        }

        private void UpdateTasksTab()
        {
            if (!ShowingDownloadingTab)
                return;

            BindingList<TaskInfo> list = (BindingList<TaskInfo>)tasksGridView.DataSource;

            var tasks = DownloadManager.Tasks;

            var add = (from task in tasks
                       where !list.Contains(task)
                       select task).ToList();

            var remove = (from task in list
                          where !tasks.Contains(task)
                          select task).ToList();

            foreach (var el in add)
                list.Add(el);
            foreach (var el in remove)
                list.Remove(el);

            tasksGridView.Invalidate();
        }

        private bool DownloadingPages
        {
            get
            {
                return DownloadManager.Tasks.Any(t => t.State == TaskState.Downloading);
            }
        }

        private void MangaCrawlerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DownloadingPages)
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
            Settings.Instance.ImagesBaseDir = directoryPathTextBox.Text;
            directoryPathTextBox.Text = Settings.Instance.ImagesBaseDir;
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
                Process.Start(DownloadManager.SelectedServer.URL);
            else
                SystemSounds.Beep.Play();
        }

        private void seriesURLButton_Click(object sender, EventArgs e)
        {
            if (DownloadManager.SelectedSerie != null)
                Process.Start(DownloadManager.SelectedSerie.URL);
            else
                SystemSounds.Beep.Play();
        }

        private void chapterURLButton_Click(object sender, EventArgs e)
        {
            if (DownloadManager.SelectedChapter != null)
                Process.Start(DownloadManager.SelectedChapter.URL);
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
                BindingList<TaskInfo> list = (BindingList<TaskInfo>)tasksGridView.DataSource;
                list[e.RowIndex].DeleteTask();
                UpdateTasksTab();
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

        private void ListBox_DrawItem(DrawItemEventArgs e, string a_text, 
            Action<Rectangle, Font> a_draw_tip)
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

            a_draw_tip(bounds, font);

            e.DrawFocusRectangle();
        }

        private void serversListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            ServerInfo server_info = (ServerInfo)serversListBox.Items[e.Index];

            Action<Rectangle, Font> draw_tip = (rect, font) =>
            {
                switch (server_info.State)
                {
                    case ServerState.Error: 

                        e.Graphics.DrawString(Resources.Error, font, 
                            Brushes.Red, rect, StringFormat.GenericDefault); 
                        break;

                    case ServerState.Downloaded:

                        e.Graphics.DrawString(
                            String.Format(Resources.Series, server_info.Series.Count()), 
                            font, Brushes.Green, rect, StringFormat.GenericDefault); 
                        break;

                    case ServerState.Waiting: 

                        e.Graphics.DrawString(Resources.Waiting, font, 
                            Brushes.Blue, rect, StringFormat.GenericDefault); 
                         break;

                    case ServerState.Downloading: 

                        e.Graphics.DrawString(
                            String.Format("({0}%)", server_info.DownloadProgress), 
                            font, Brushes.Blue, rect, StringFormat.GenericDefault); 
                        break;

                    case ServerState.Initial: break;

                    default: throw new NotImplementedException();
                }
            };

            ListBox_DrawItem(e, server_info.Name, draw_tip);
        }

        private void seriesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            SerieInfo serie_info = (SerieInfo)seriesListBox.Items[e.Index];

            Action<Rectangle, Font> draw_tip = (rect, font) =>
            {
                switch (serie_info.State)
                {
                    case SerieState.Error:

                        e.Graphics.DrawString(Resources.Error, font,
                            Brushes.Red, rect, StringFormat.GenericDefault);
                        break;

                    case SerieState.Downloaded:

                        e.Graphics.DrawString(
                            String.Format(Resources.Chapters, serie_info.Chapters.Count()), 
                            font, Brushes.Green, rect, StringFormat.GenericDefault);
                        break;

                    case SerieState.Waiting:

                        e.Graphics.DrawString(Resources.Waiting, font,
                            Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;

                    case SerieState.Downloading:

                        e.Graphics.DrawString(
                            String.Format("({0}%)", serie_info.DownloadProgress), 
                            font, Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;

                    case SerieState.Initial: break;

                    default: throw new NotImplementedException();
                }
            };

            ListBox_DrawItem(e, serie_info.Title, draw_tip);
        }

        private void chaptersListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ChapterInfo chapter_info = (ChapterInfo)chaptersListBox.Items[e.Index];

            Action<Rectangle, Font> draw_tip = (rect, font) =>
            {
                switch (chapter_info.State)
                {
                    case ChapterState.Error:

                        e.Graphics.DrawString(Resources.Error, font,
                            Brushes.Red, rect, StringFormat.GenericDefault);
                        break;

                    case ChapterState.Aborted:

                        e.Graphics.DrawString(Resources.Aborted, font,
                            Brushes.Red, rect, StringFormat.GenericDefault);
                        break;

                    case ChapterState.Downloaded:

                        e.Graphics.DrawString(Resources.Downloaded, font,
                            Brushes.Green, rect, StringFormat.GenericDefault);
                        break;

                    case ChapterState.WasDownloaded:

                        e.Graphics.DrawString(Resources.WasDownloaded, font,
                            Brushes.Green, rect, StringFormat.GenericDefault);
                        break;

                    case ChapterState.Waiting:

                        e.Graphics.DrawString(Resources.Waiting, font,
                            Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;

                    case ChapterState.Deleting:

                        e.Graphics.DrawString(Resources.Deleting, font,
                            Brushes.Red, rect, StringFormat.GenericDefault);
                        break;

                    case ChapterState.Downloading:
                    {
                        e.Graphics.DrawString(
                            String.Format("{0}/{1}", chapter_info.Task.DownloadedPages, chapter_info.Task.Pages.Count),
                            font, Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;
                    }

                    case ChapterState.Zipping:

                        e.Graphics.DrawString(Resources.Zipping, font,
                            Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;

                    case ChapterState.Initial: break;

                    default: throw new NotImplementedException();
                }
            };

            ListBox_DrawItem(e, chapter_info.Title, draw_tip);
        }

        private void cbzCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.UseCBZ = cbzCheckBox.Checked;
        }

        private void linkLabel1_LinkClicked(object sender, 
            LinkLabelLinkClickedEventArgs e)
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

        private void MangaCrawlerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Instance.Save();
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateTasksTab();
            UpdateSeriesTab();
        }

        private void UpdateSeriesTab()
        {
            if (!ShowingSeriesTab)
                return;

            UpdateServers();
            UpdateSeries();
            UpdateChapters();
            
        }

        private void UpdateChapters()
        {
            if (!ShowingSeriesTab)
                return;

            ChapterInfo[] ar = new ChapterInfo[0];

            if (SelectedSerie != null)
            {
                ar = (from ch in SelectedSerie.Chapters
                      select ch).ToArray();
            }

            new ListBoxVisualState(chaptersListBox).ReloadItems(ar);
        }

        private void UpdateServers()
        {
            if (!ShowingSeriesTab)
                return;

            new ListBoxVisualState(serversListBox).ReloadItems(DownloadManager.Servers);
        }

        private void UpdateSeries()
        {
            if (!ShowingSeriesTab)
                return;

            SerieInfo[] ar = new SerieInfo[0];

            if (SelectedServer != null)
            {
                string filter = seriesSearchTextBox.Text.ToLower();
                ar = (from serie in SelectedServer.Series
                      where serie.Title.ToLower().IndexOf(filter) != -1
                      select serie).ToArray();
            }

            new ListBoxVisualState(seriesListBox).ReloadItems(ar);
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTasksTab();
        }

        private void clearLogButton_Click(object sender, EventArgs e)
        {
            //TODO: richedit appender
            //(LogManager.Configuration.FindTargetByName("richTextBox") as NLog.Targets.RichTextBoxTarget).Clear();
        }

        private void saveTimer_Tick(object sender, EventArgs e)
        {
            Settings.Instance.Save();
            DownloadManager.Save();
        }
    }
}
