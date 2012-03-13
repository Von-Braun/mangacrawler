using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using MangaCrawlerLib;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using MangaCrawler.Properties;
using System.Diagnostics;
using System.Media;
using HtmlAgilityPack;
using System.Threading;
using log4net.Core;
using log4net.Layout;
using log4net.Config;
using log4net;

namespace MangaCrawler
{
    /* TODO:
     * 
     * co zrobic jak serwer, seria, page nie istnieje po stornie serwera a my mamy z niego informacje na dysku
     * nic nie rob, kasuj, oznacz jako skasowane, 
     * istnieje tez mozliwosc zachowaj w wizualizacji, oznacz jako skasowane, zachowaj tak dlugo jak istnieje na dysku
     * a najlepiej zapytac sie usera co zrobic, skasowac, zmienic nazwe, zostawic jak jest
     * 
     * co zrobic jesli chaptery nie sa dane alfabetycznie, nie sa ponumerowane, innymi slowy widok leb moze byc 
     * nieposortowany, ale widok folderow na dysku juz tak
     * 
     * wbudowany browser
     * 
     * widok wspolny dla wszystkich serwisow, scalac jakos serie,
     * gdzie najlepsza jakosc, gdzie duplikaty
     * 
     * wpf, windows phone, inne
     * 
     * bookmarks
     * 
     * praca w tle, powiadamianie o zmianach w bookmarks
     * 
     * instalator, x86, x64
     * 
     * nowe serwisy:
     * http://www.mangahere.com/
     * http://www.mangareader.net/alphabetical
     * http://mangable.com/manga-list/
     * http://www.readmangaonline.net/
     * http://www.anymanga.com/directory/all/
     * http://manga.animea.net/browse.html
     * http://www.mangamonger.com/
     * 
     * jesli duzo bledow to zmniejszac ilosc polaczen
     * 
     * przejsc na nowa wersje w ktorej jest ona data, roznoczesnie zachowujac obecny system powiadamiania o nowej 
     * wersji, najlepiej wydac wersje tymczasowa i potrzymac ja przez miesiac, wyswieltac messagebox o nowej wersji
     * 
     * Testy
     * sciaganie losowe wielu rzeczy i ch anulowanie, brak wyjatkow, deadlockow, spojnosc danych
     * dodac procedure ktora testuje spojnosc danych - ich stan i powiazania
     * pobranie serii, chapterow, pagey - dodanie nowych, usuniecie istniejacych, jakies zmiany, czy ponowne 
     *   pobranie sobie z tym radzi
     * page - zmiana hashu juz pobranego, usunicie go z dysku
     * page - symulacja webexcption podczas pobierania
     * page - symulacja pobrania 0 lenth
     * page - symulacja pobrania smieci - np 404 not found
     * testy na series, chapter na zwrocenie jakis wyjatkow z czesci web
     * page - wywalenie wyjatku, niemozliwy zapis pliku (na plik dac lock)
     * podotykac wszystkiego, sprawdzic zajetosc pamieci
     * ktos klika w element ktory nie istnieje, pojawia sie error, albo jest on w trakcie sciagania, w tym 
     *   czasie nastepuje jego odswiezenie i znika on z listy
     * zmiana katalogu glownego z kombinacja powyzszych
     * uruchomienie aplikacji na czysto - sprawdzanie czy wszystk sie dobrze laduje
     * usunelismy jakis serie, chapter, powinno sie pokasowac co trzeba w katalogu, dla serii, wszystkie chaptery i page, 
     * pojawienie sie czegos nowego, czy zostanie dodany tylko jeden nowy id
     * testy na pamietanie, przywracanie stanu vizualizacji kiedy kasowane sa zaznaczone elementy, tak ze znikaja 
     * itemy, indexy wychodza za zakres itp, multiselekcja takze test
     * xml - wpisac cos nie poprawnego do nazwy pliku co sprawi ze fileinfo wywali wyjatek
     * przetestowac compaktowanie 
     * - dodac jakies inne liki
     * - dodac xml o zlych nazwach
     * - wprowadzac bledy w xmlach
     * - czy usuwane sa sieroty
     * - czy usuwane sa chaptery bez imagow (nie ma byc all image)
     * - czy usuwane sa uste serie
     * - czy brute force usuwa te najstarsze
     * - czy brute force nie rusza do akcji jesli nie trzeba
     * strategie zmiany nazw imaagow
     * 
     * jak usuwac skonczone worksy z downloadmanagera, najlepiej tak blednych nie usuwac wogole, skonczone usuwac 
     * tylko jesli tak zostalo zaznaczone
     * 
     * dodac do paraller.foreach, for, wlasny partitioner, ktory bedzie wybieral sekwencje jedna po drugiej
     * 
     * deinstalacja powinna usunac katalog
     * 
     * pamietanie pobieranych, prawidlowa reinicjalizacja stanow chapteru, stron, zapis deleting
     * zapisywac limiter order
     * 
     */

    public partial class MangaCrawlerForm : Form
    {
        private Dictionary<Server, ListBoxVisualState> m_series_visual_states =
            new Dictionary<Server, ListBoxVisualState>();
        private Dictionary<Serie, ListBoxVisualState> m_chapters_visual_states =
            new Dictionary<Serie, ListBoxVisualState>();

        private bool m_refresh_once_after_all_done;

        private Color BAD_DIR = Color.Red;

        public MangaCrawlerForm()
        {
            InitializeComponent();

            Settings.Instance.FormState.Init(this);
        }

        private void MangaShareCrawlerForm_Load(object sender, EventArgs e)
        {
            SetupLog4NET();

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
            DownloadManager.GetSettingsDir = () => Settings.GetSettingsDir();
            DownloadManager.GetCheckTimeDelta = () => Settings.Instance.CheckTimeDelta;
            DownloadManager.PageNamingStrategy = () => Settings.Instance.PageNamingStrategy;
            DownloadManager.UpdateGUI = () => UpdateAll();

            mangaRootDirTextBox.Text = Settings.Instance.MangaRootDir;
            seriesSearchTextBox.Text = Settings.Instance.SeriesFilter;
            cbzCheckBox.Checked = Settings.Instance.UseCBZ;

            Task.Factory.StartNew(() => CheckNewVersion(), TaskCreationOptions.LongRunning);

            if (!Loggers.Log())
            {
                tabControl.TabPages.Remove(logTabPage);
                LogManager.Shutdown();
            }

            //Flicker-free.
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, worksGridView, new object[] { true });

            worksGridView.AutoGenerateColumns = false;
            worksGridView.DataSource = new BindingList<WorkGridRow>();

            UpdateAll();

            refreshTimer.Enabled = true;
        }

        private void SetupLog4NET()
        {
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
                ListBoxVisualState vs;
                if (m_series_visual_states.TryGetValue(SelectedServer, out vs))
                    vs.Restore();
            }
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedServer != null)
                m_series_visual_states[SelectedServer] = 
                    new ListBoxVisualState(seriesListBox);

            DownloadManager.DownloadChapters(SelectedSerie);

            UpdateChapters();

            if (SelectedSerie != null)
            {
                ListBoxVisualState vs;
                if (m_chapters_visual_states.TryGetValue(SelectedSerie, out vs))
                    vs.Restore();
            }
        }

        private void chaptersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedSerie != null)
                m_chapters_visual_states[SelectedSerie] = 
                    new ListBoxVisualState(chaptersListBox);
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
                chaptersListBox.SelectedItems.Cast<ChapterListItem>().Select(cli => cli.Chapter).ToArray());
        }

        private void UpdateWorksTab()
        {
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

        private void MangaCrawlerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DownloadManager.Works.Any(w => w.IsWorking))
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
                catch (Exception ex)
                {
                    Loggers.GUI.Error("Exception", ex);
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
                catch (Exception ex)
                {
                    Loggers.GUI.Error("Exception", ex);
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
                catch (Exception ex)
                {
                    Loggers.GUI.Error("Exception", ex);
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
                m_series_visual_states[SelectedServer] = 
                    new ListBoxVisualState(seriesListBox);
        }

        private void chaptersListBox_VerticalScroll(object a_sender, bool a_tracking)
        {
            if (SelectedSerie != null)
                m_chapters_visual_states[SelectedSerie] = 
                    new ListBoxVisualState(chaptersListBox);
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
            catch (Exception ex)
            {
                Loggers.GUI.Error("Exception", ex);
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
            DownloadManager.Save();
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            if (DownloadManager.IsDownloading())
            {
                if (m_refresh_once_after_all_done)
                    Loggers.GUI.Debug("Starting refreshing");
                m_refresh_once_after_all_done = false;
            }
            else if (!m_refresh_once_after_all_done)
            {
                Loggers.GUI.Debug("Stopping refreshing");
                m_refresh_once_after_all_done = true;
            }
            else
                return;

            UpdateAll();
        }

        private void UpdateAll()
        {
            UpdateOptions();
            UpdateWorksTab();
            UpdateServers();
            UpdateSeries();
            UpdateChapters();
        }       

        private void UpdateChapters()
        {
            ChapterListItem[] ar = new ChapterListItem[0];

            if (SelectedSerie != null)
            {
                ar = (from chapter in SelectedSerie.Chapters
                        select new ChapterListItem(chapter)).ToArray();
            }

            new ListBoxVisualState(chaptersListBox).ReloadItems(ar);
        }

        private void UpdateServers()
        {
            var servers = (from server in DownloadManager.Servers
                           select new ServerListItem(server)).ToArray();

            new ListBoxVisualState(serversListBox).ReloadItems(servers);
        }

        private void UpdateSeries()
        {
            SerieListItem[] ar = new SerieListItem[0];

            if (SelectedServer != null)
            {
                string filter = seriesSearchTextBox.Text.ToLower();
                ar = (from serie in SelectedServer.Series
                      where serie.Title.ToLower().IndexOf(filter) != -1
                      select new SerieListItem(serie)).ToArray();
            }

            new ListBoxVisualState(seriesListBox).ReloadItems(ar);
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWorksTab();
        }

        private void clearLogButton_Click(object sender, EventArgs e)
        {
            logRichTextBox.Clear();
        }

        private void serversListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            ((sender as ListBox).Items[e.Index] as ListItem).DrawItem(e);
        }

        private void seriesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            ((sender as ListBox).Items[e.Index] as ListItem).DrawItem(e);

        }

        private void chaptersListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            ((sender as ListBox).Items[e.Index] as ListItem).DrawItem(e);
        }

        private void MangaCrawlerForm_Shown(object sender, EventArgs e)
        {
            splitter1.SplitPosition = Settings.Instance.SplitterDistance;

            if (Catalog.GetCatalogSize() > Settings.Instance.MaxCatalogSize)
                new CatalogOptimizeForm().ShowDialog();
        }

        private void openServerFolderButton_Click(object sender, EventArgs e)
        {
            if (SelectedServer != null)
            {
                try
                {
                    Process.Start(SelectedServer.GetServerDirectory());
                }
                catch (Exception ex)
                {
                    Loggers.GUI.Error("Exception", ex);
                    SystemSounds.Beep.Play();
                }
            }
            else
                SystemSounds.Beep.Play();
        }

        private void openSeriesFolderButton_Click(object sender, EventArgs e)
        {
            if (SelectedSerie != null)
            {
                try
                {
                    Process.Start(SelectedSerie.GetSerieDirectory());
                }
                catch (Exception ex)
                {
                    Loggers.GUI.Error("Exception", ex);
                    SystemSounds.Beep.Play();
                }
            }
            else
                SystemSounds.Beep.Play();
        }

        private void viewPagesButton_Click(object sender, EventArgs e)
        {
            var chapter = SelectedChapter;
            if (chapter == null)
            {
                if (chaptersListBox.Items.Count == 1)
                    chapter = (chaptersListBox.Items[0] as ChapterListItem).Chapter;

            }
            if (chapter != null)
            {
                if (chapter.Pages.Any())
                {
                    if (chapter.Pages.First().State == PageState.Downloaded)
                    {
                        if (new FileInfo(chapter.Pages.First().ImageFilePath).Exists)
                        {
                            try
                            {
                                Process.Start(chapter.Pages.First().ImageFilePath);
                            }
                            catch (Exception ex)
                            {
                                Loggers.GUI.Error("Exception", ex);
                                SystemSounds.Beep.Play();
                            }
                        }
                        else
                            SystemSounds.Beep.Play();
                    }
                    else
                        SystemSounds.Beep.Play();
                }
                else
                    SystemSounds.Beep.Play();
            }
            else
                SystemSounds.Beep.Play();
        }

        private void openPagesFolder_Click(object sender, EventArgs e)
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
                    Process.Start(chapter.GetChapterDirectory());
                }
                catch (Exception ex)
                {
                    Loggers.GUI.Error("Exception", ex);
                    SystemSounds.Beep.Play();
                }
            }
            else
                SystemSounds.Beep.Play();
        }

        private void pageNamingStrategyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pageNamingStrategyComboBox.SelectedIndex == 0)
                Settings.Instance.PageNamingStrategy = PageNamingStrategy.DoNothing;
            else if (pageNamingStrategyComboBox.SelectedIndex == 1)
                Settings.Instance.PageNamingStrategy = PageNamingStrategy.PrefixWithIndexWhenNotOrdered;
            else if (pageNamingStrategyComboBox.SelectedIndex == 2)
                Settings.Instance.PageNamingStrategy = PageNamingStrategy.OnlyIndex;
            else
                Loggers.GUI.Error("Invalid PageNamingStrategy");
        }

        private void UpdateOptions()
        {
            bool show = DownloadManager.Works.All(w => !w.IsWorking);

            foreach (var control in optionsTabPage.Controls.Cast<Control>())
            {
                if (control == optionslLabel)
                {
                    if (show)
                        control.Hide();
                    else
                        control.Show();
                }
                else
                {
                    if (show)
                        control.Show();
                    else
                        control.Hide();
                }
            }
        }
    }
}
