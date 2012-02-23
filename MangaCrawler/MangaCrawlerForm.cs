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
using log4net.Core;
using log4net.Config;
using log4net.Layout;

namespace MangaCrawler
{
    //
    // TODO: testowe serwery, testowe srodowisko testowe, wiesza sie
    //
    // TODO: chapter powienine miec page, wraz s sciezka, hashem
    // 
    // TODO: jesli zbyt duzo bledow web to zmiejszamy liczbe polaczen na serwer
    //
    // TODO: wersja to data, ustawiana automatycznie podczas budowania, generowanie jakiegos pliku 
    //       z data.
    //
    // TODO: po sciagnieciu nie kasowac taska, dac mozliwosc przejrzenia mangi
    //       lib zawsze pamieta taski, to gui je odrzuca
    //
    // TODO: pamietanie taskow podczas zamkniecia
    //
    // TODO: pamietanie pobranych
    // 
    // TODO: http://www.mangareader.net/alphabetical
    // TODO: http://mangable.com/manga-list/
    // TODO: http://www.readmangaonline.net/
    // TODO: http://www.anymanga.com/directory/all/
    // TODO: http://manga.animea.net/browse.html
    // TODO: http://www.mangamonger.com/
    //
    // TODO: instalator, x86 i x64
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
        private Dictionary<Server, ListBoxVisualState<SerieListItem>> m_series_visual_states =
            new Dictionary<Server, ListBoxVisualState<SerieListItem>>();
        private Dictionary<Serie, ListBoxVisualState<ChapterListItem>> m_chapters_visual_states =
            new Dictionary<Serie, ListBoxVisualState<ChapterListItem>>();

        private Color BAD_DIR = Color.Red;

        public MangaCrawlerForm()
        {
            InitializeComponent();

            Settings.Instance.FormState.Init(this);
        }

        private void MangaShareCrawlerForm_Load(object sender, EventArgs e)
        {
            Text = String.Format("{0} {1}.{2}", Text,
                Assembly.GetAssembly(GetType()).GetName().Version.Major, 
                Assembly.GetAssembly(GetType()).GetName().Version.Minor);

            DownloadManager.GetMangaRootDir = () => 
            {
                string str = Settings.Instance.MangaRootDir;
                if (mangaRootDirTextBox.Text.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    mangaRootDirTextBox.Text = mangaRootDirTextBox.Text.Remove(
                        mangaRootDirTextBox.Text.Length - 1);
                }
                return str;
            };

            DownloadManager.UseCBZ = () => Settings.Instance.UseCBZ;

            mangaRootDirTextBox.Text = Settings.Instance.MangaRootDir;
            seriesSearchTextBox.Text = Settings.Instance.SeriesFilter;
            splitter1.SplitPosition = Settings.Instance.SplitterDistance;
            cbzCheckBox.Checked = Settings.Instance.UseCBZ;

            SetupLog4NET();

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
                null, worksGridView, new object[] { true });

            worksGridView.AutoGenerateColumns = false;
            worksGridView.DataSource = new BindingList<WorkGridRow>();

            refreshTimer.Enabled = true;

            UpdateSeriesTab();
        }

        private void SetupLog4NET()
        {
            XmlConfigurator.Configure();
            
            RichTextBoxAppender rba = new RichTextBoxAppender(logRichTextBox);
            rba.Threshold = Level.All;
            rba.Layout = new PatternLayout(
                "%date{yyyy-MM-dd HH:mm:ss,fff} %-7level %-14logger %thread %class.%method - %message %newline");

            LevelTextStyle ilts = new LevelTextStyle();
            ilts.Level = Level.Info;
            ilts.TextColor = Color.Black;
            rba.AddMapping(ilts);

            LevelTextStyle dlts = new LevelTextStyle();
            dlts.Level = Level.Debug;
            dlts.TextColor = Color.LightBlue;
            rba.AddMapping(dlts);

            LevelTextStyle wlts = new LevelTextStyle();
            wlts.Level = Level.Warn;
            wlts.TextColor = Color.Yellow;
            rba.AddMapping(wlts);

            LevelTextStyle elts = new LevelTextStyle();
            elts.Level = Level.Error;
            elts.TextColor = Color.Red;
            rba.AddMapping(elts);

            BasicConfigurator.Configure(rba);
            rba.ActivateOptions();
        }

        public Serie SelectedSerie
        {
            get
            {
                if (seriesListBox.SelectedItem == null)
                    return null;

                return (seriesListBox.SelectedItem as SerieListItem).Serie;
            }
        }

        public Server SelectedServer
        {
            get
            {
                if (serversListBox.SelectedItem == null)
                    return null;

                return (serversListBox.SelectedItem as ServerListItem).Server;
            }
        }

        public Chapter SelectedChapter
        {
            get
            {
                if (chaptersListBox.SelectedItem == null)
                    return null;

                return (chaptersListBox.SelectedItem as ChapterListItem).Chapter;
            }
        }

        private void mangaRootDirChooseButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = mangaRootDirTextBox.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                mangaRootDirTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        private void serversListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadManager.DownloadSeries(SelectedServer);

            UpdateSeries();

            if (SelectedServer != null)
            {
                ListBoxVisualState<SerieListItem> vs;
                if (m_series_visual_states.TryGetValue(SelectedServer, out vs))
                    vs.Restore();
            }
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedServer != null)
                m_series_visual_states[SelectedServer] = new ListBoxVisualState<SerieListItem>(seriesListBox);
            
            DownloadManager.DownloadChapters(SelectedSerie);

            UpdateChapters();

            if (SelectedSerie != null)
            {
                ListBoxVisualState<ChapterListItem> vs;
                if (m_chapters_visual_states.TryGetValue(SelectedSerie, out vs))
                    vs.Restore();
            }
        }

        private void chaptersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedSerie != null)
                m_chapters_visual_states[SelectedSerie] = new ListBoxVisualState<ChapterListItem>(chaptersListBox);
        }

        private void seriesSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.SeriesFilter = seriesSearchTextBox.Text;
            UpdateSeries();
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            DownloadSelectedChapters();
        }

        public static bool IsDirectoryPathValid()
        {
            try
            {
                new DirectoryInfo(Settings.Instance.MangaRootDir);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void DownloadSelectedChapters()
        {
            if (!IsDirectoryPathValid())
            {
                PulseMangaRootDirTextBox();
                return;
            }


            try
            {
                new DirectoryInfo(Settings.Instance.MangaRootDir).Create();
            }
            catch
            {
                MessageBox.Show(String.Format(Resources.DirError, 
                    Settings.Instance.MangaRootDir),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (chaptersListBox.SelectedItems.Count == 0)
            {
                SystemSounds.Beep.Play();
                return;
            }

            DownloadManager.DownloadPages(
                chaptersListBox.SelectedItems.Cast<ChapterListItem>().Select(cli => cli.Chapter));
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

        private void UpdateWorksTab()
        {
            if (!ShowingDownloadingTab)
                return;

            BindingList<WorkGridRow> list = (BindingList<WorkGridRow>)worksGridView.DataSource;

            var works = DownloadManager.Works;

            var add = (from work in works
                       where !list.Any(w => w.Chapter == work)
                       select work).ToList();

            var remove = (from work in list
                          where !works.Any(w => w == work.Chapter)
                          select work).ToList();

            foreach (var el in add)
                list.Add(new WorkGridRow(el));
            foreach (var el in remove)
                list.Remove(el);

            worksGridView.Invalidate();
        }

        private bool DownloadingPages
        {
            get
            {
                return DownloadManager.Works.Any(w => w.State == ChapterState.Downloading);
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

        private void mangaRootDirTextBox_TextChanged(object sender, EventArgs e)
        {
            Settings.Instance.MangaRootDir = mangaRootDirTextBox.Text;

            if (!IsDirectoryPathValid())
                mangaRootDirTextBox.BackColor = BAD_DIR;
            else
                mangaRootDirTextBox.BackColor = SystemColors.Window;
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
            {
                try
                {
                    Process.Start(SelectedServer.URL);
                }
                catch
                {
                    Loggers.GUI.Error("Exception");
                    SystemSounds.Beep.Play();
                }
            }
            else
                SystemSounds.Beep.Play();
        }

        private void seriesURLButton_Click(object sender, EventArgs e)
        {
            var serie = SelectedSerie;
            if (serie == null)
            {
                if (seriesListBox.Items.Count == 1)
                    serie = (seriesListBox.Items[0] as SerieListItem).Serie;
            }

            if (serie != null)
            {
                try
                {
                    Process.Start(serie.URL);
                }
                catch
                {
                    Loggers.GUI.Error("Exception");
                    SystemSounds.Beep.Play();
                }
            }
            else
                SystemSounds.Beep.Play();
        }

        private void chapterURLButton_Click(object sender, EventArgs e)
        {
            var chapter = SelectedChapter;
            if (chapter == null)
            {
                if (chaptersListBox.Items.Count == 1)
                    chapter = (chaptersListBox.Items[0] as ChapterListItem).Chapter;

            }
            if (chapter != null)
            {
                try
                {
                    Process.Start(chapter.URL);
                }
                catch
                {
                    Loggers.GUI.Error("Exception");
                    SystemSounds.Beep.Play();
                }
            }
            else
                SystemSounds.Beep.Play();
        }

        private void chaptersListBox_DoubleClick(object sender, EventArgs e)
        {
            DownloadSelectedChapters();
        }

        private void worksGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == 0) && (e.RowIndex >= 0))
            {
                BindingList<WorkGridRow> list = (BindingList<WorkGridRow>)worksGridView.DataSource;
                list[e.RowIndex].Chapter.DeleteWork();
                UpdateWorksTab();
            }
        }

        private void seriesListBox_VerticalScroll(object a_sender, bool a_tracking)
        {
            if (SelectedServer != null)
                m_series_visual_states[SelectedServer] = new ListBoxVisualState<SerieListItem>(seriesListBox);
        }

        private void chaptersListBox_VerticalScroll(object a_sender, bool a_tracking)
        {
            if (SelectedSerie != null)
                m_chapters_visual_states[SelectedSerie] = new ListBoxVisualState<ChapterListItem>(chaptersListBox);
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

            Server server = (serversListBox.Items[e.Index] as ServerListItem).Server;

            Action<Rectangle, Font> draw_tip = (rect, font) =>
            {
                switch (server.State)
                {
                    case ServerState.Error: 

                        e.Graphics.DrawString(Resources.Error, font, 
                            Brushes.Red, rect, StringFormat.GenericDefault); 
                        break;

                    case ServerState.Downloaded:

                        e.Graphics.DrawString(
                            String.Format(Resources.Series, server.GetSeries().Count), 
                            font, Brushes.Green, rect, StringFormat.GenericDefault); 
                        break;

                    case ServerState.Waiting: 

                        e.Graphics.DrawString(Resources.Waiting, font, 
                            Brushes.Blue, rect, StringFormat.GenericDefault); 
                         break;

                    case ServerState.Downloading: 

                        e.Graphics.DrawString(
                            String.Format("({0}%)", server.DownloadProgress), 
                            font, Brushes.Blue, rect, StringFormat.GenericDefault); 
                        break;

                    case ServerState.Initial: break;

                    default: throw new NotImplementedException();
                }
            };

            ListBox_DrawItem(e, server.Name, draw_tip);
        }

        private void seriesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            Serie serie = (seriesListBox.Items[e.Index] as SerieListItem).Serie;

            Action<Rectangle, Font> draw_tip = (rect, font) =>
            {
                switch (serie.State)
                {
                    case SerieState.Error:

                        e.Graphics.DrawString(Resources.Error, font,
                            Brushes.Red, rect, StringFormat.GenericDefault);
                        break;

                    case SerieState.Downloaded:

                        e.Graphics.DrawString(
                            String.Format(Resources.Chapters, serie.GetChapters().Count), 
                            font, Brushes.Green, rect, StringFormat.GenericDefault);
                        break;

                    case SerieState.Waiting:

                        e.Graphics.DrawString(Resources.Waiting, font,
                            Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;

                    case SerieState.Downloading:

                        e.Graphics.DrawString(
                            String.Format("({0}%)", serie.DownloadProgress), 
                            font, Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;

                    case SerieState.Initial: break;

                    default: throw new NotImplementedException();
                }
            };

            ListBox_DrawItem(e, serie.Title, draw_tip);
        }

        private void chaptersListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            Chapter chapter = (chaptersListBox.Items[e.Index] as ChapterListItem).Chapter;

            Action<Rectangle, Font> draw_tip = (rect, font) =>
            {
                switch (chapter.State)
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
                            String.Format("{0}/{1}", chapter.DownloadedPages, chapter.GetPages().Count()),
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

            ListBox_DrawItem(e, chapter.Title, draw_tip);
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

                    Task.Factory.StartNew(() => PulseNewVersionLinkLabel());
                }
            }
            catch
            {
                Loggers.GUI.Error("Exception");
            }
        }

        private void Pulse(Color a_color_org, Color a_color_alter, int a_count, int pulse_time_ms, Action<Color> a_action)
        {
            const int SLEEP_TIME = 25;
            int steps = pulse_time_ms / SLEEP_TIME;

            Func<IEnumerable<Color>> get_colors = () =>
            {
                List<Color> result = new List<Color>();

                // limit to byte
                Func<int, int> limit =
                    (c) => (c < 0) ? byte.MinValue : (c > 255) ? byte.MaxValue : c;

                // ph=0 return c1 ... ph=255 return c2
                Func<int, int, int, int> calc =
                    (c1, c2, ph) => limit(c1 + (c2 - c1) * ph / 255);

                for (int i = 0; i < a_count; i++)
                {
                    for (int phase = 0; phase < steps; phase++)
                    {
                        // 0 ... steps/2 ... 0
                        var p = steps / 2 - Math.Abs(phase - steps / 2);

                        // 0 ... pi ... 0
                        var pp = p * Math.PI / (steps / 2);

                        // 1 ... -1 ... 1
                        pp = Math.Cos(pp);

                        // 2 .. 0 .. 2
                        pp = pp + 1;

                        // 0 .. 2 .. 0
                        pp = 2 - pp;

                        // 0 ... 1 ... 0
                        pp = pp / 2;

                        // 0 ... 255 ... 0 
                        p = limit((int)(Math.Round(pp * 255)));

                        var r = calc(a_color_org.R, a_color_alter.R, p);
                        var g = calc(a_color_org.G, a_color_alter.G, p);
                        var b = calc(a_color_org.B, a_color_alter.B, p);
                        result.Add(Color.FromArgb(r, g, b));
                    }
                }

                return result;
            };

            Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var color in get_colors())
                    {
                        Invoke(a_action, color);
                        Thread.Sleep(SLEEP_TIME);
                    }
                }
                catch
                {
                    Loggers.GUI.Error("Exception");
                }
            });
        }

        private void PulseNewVersionLinkLabel()
        {
            Pulse(versionLinkLabel.LinkColor, Color.Red, 12, 700, (c) => versionLinkLabel.LinkColor = c);
        }

        private void PulseMangaRootDirTextBox()
        {
            Color c1 = mangaRootDirTextBox.BackColor;
            Color c2 = (c1 == BAD_DIR) ? SystemColors.Window : BAD_DIR;
            Pulse(c1, c2, 4, 500, (c) => mangaRootDirTextBox.BackColor = c);
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
            if (serversListBox.SelectedItem == null)
                return;

            UpdateWorksTab();
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

            ChapterListItem[] ar = new ChapterListItem[0];

            if (SelectedSerie != null)
            {
                ar = (from ch in SelectedSerie.GetChapters()
                      select new ChapterListItem(ch)).ToArray();
            }

            new ListBoxVisualState<ChapterListItem>(chaptersListBox).ReloadItems(ar);
        }

        private void UpdateServers()
        {
            if (!ShowingSeriesTab)
                return;

            var ar = from server in DownloadManager.Servers
                     select new ServerListItem(server);

            new ListBoxVisualState<ServerListItem>(serversListBox).ReloadItems(ar.ToArray());
        }

        private void UpdateSeries()
        {
            if (!ShowingSeriesTab)
                return;

            SerieListItem[] ar = new SerieListItem[0];

            if (SelectedServer != null)
            {
                string filter = seriesSearchTextBox.Text.ToLower();
                ar = (from serie in SelectedServer.GetSeries()
                      where serie.Title.ToLower().IndexOf(filter) != -1
                      select new SerieListItem(serie)).ToArray();
            }

            new ListBoxVisualState<SerieListItem>(seriesListBox).ReloadItems(ar);
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWorksTab();
        }

        private void clearLogButton_Click(object sender, EventArgs e)
        {
            logRichTextBox.Clear();
        }
    }
}
