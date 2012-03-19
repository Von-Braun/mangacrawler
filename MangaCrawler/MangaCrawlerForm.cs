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
     * wbudowany browser
     * 
     * widok wspolny dla wszystkich serwisow, scalac jakos serie,
     * gdzie najlepsza jakosc, gdzie duplikaty
     * 
     * wpf, windows phone, inne
     * 
     * instalator, x86, x64
     * deinstalacja powinna usunac katalog
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
     * testy gui
     * testy czy pobieranie jest w kolejnosci zaznaczania
     * testy na dzialanie priorytetow
     * testy na manga dir ze i bez slasha na koncu
     * test ze wszystkie stany potrafia sie odrysowac - niektore nie powinny przejsc do rysowania, ale moze sie zdarzyc
     * przetestowac dzialanie wszelkich przyciskow
     * bookmark, a usuwanie serii, rozdzialu
     * 
     * praca w tle, powiadamianie o zmianach w bookmarks
     * 
     * zapamietywanie splittera
     * 
     */

    public partial class MangaCrawlerForm : Form
    {
        private Dictionary<Server, ListBoxVisualState> m_series_visual_states =
            new Dictionary<Server, ListBoxVisualState>();
        private Dictionary<Serie, ListBoxVisualState> m_chapters_visual_states =
            new Dictionary<Serie, ListBoxVisualState>();
        private Dictionary<Serie, ListBoxVisualState> m_chapter_bookmarks_visual_states =
            new Dictionary<Serie, ListBoxVisualState>();

        private bool m_refresh_once_after_all_done;
        private bool m_working;

        private static Color BAD_DIR = Color.Red;

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

            DownloadManager.Create(
                Settings.Instance.MangaSettings, 
                Settings.GetSettingsDir());

            mangaRootDirTextBox.Text = Settings.Instance.MangaSettings.GetMangaRootDir(false);
            seriesSearchTextBox.Text = Settings.Instance.SeriesFilter;
            cbzCheckBox.Checked = Settings.Instance.MangaSettings.UseCBZ;

            if (Settings.Instance.MangaSettings.PageNamingStrategy == PageNamingStrategy.DoNothing)
                pageNamingStrategyComboBox.SelectedIndex = 0;
            else if (Settings.Instance.MangaSettings.PageNamingStrategy == PageNamingStrategy.PrefixWithIndexWhenNotOrdered)
                pageNamingStrategyComboBox.SelectedIndex = 1;
            else if (Settings.Instance.MangaSettings.PageNamingStrategy == PageNamingStrategy.OnlyIndex)
                pageNamingStrategyComboBox.SelectedIndex = 2;
            else
                Loggers.GUI.Error("Invalid PageNamingStrategy");

            playSoundWhenDownloadedCheckBox.Checked = Settings.Instance.PlaySoundWhenDownloaded;

            Task.Factory.StartNew(() => CheckNewVersion(), TaskCreationOptions.LongRunning);

            if (!Loggers.Log())
            {
                tabControl.TabPages.Remove(logTabPage);
                LogManager.Shutdown();
                ContextMenuStrip = null;
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


        public Serie SelectedSerieBookmark
        {
            get
            {
                if (serieBookmarksListBox.SelectedItem == null)
                    return null;

                return (serieBookmarksListBox.SelectedItem as SerieBookmarkListItem).Serie;
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
            DownloadManager.Instance.DownloadSeries(SelectedServer);

            UpdateAll();

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

            DownloadManager.Instance.DownloadChapters(SelectedSerie);

            UpdateAll();

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
            DownloadChapters(GetSelectedChapters());
        }

        public static bool IsDirectoryPathValid()
        {
            try
            {
                new DirectoryInfo(Settings.Instance.MangaSettings.GetMangaRootDir(false));
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void DownloadChapters(IEnumerable<Chapter> a_chapters)
        {
            if (!IsDirectoryPathValid())
            {
                PulseMangaRootDirTextBox();
                return;
            }

            try
            {
                new DirectoryInfo(Settings.Instance.MangaSettings.GetMangaRootDir(false)).Create();
            }
            catch
            {
                MessageBox.Show(String.Format(Resources.DirError,
                    Settings.Instance.MangaSettings.GetMangaRootDir(false)),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (a_chapters.Count() == 0)
            {
                SystemSounds.Beep.Play();
                return;
            }

            if (a_chapters.Any(c => c.IsDownloading))
                SystemSounds.Beep.Play();

            DownloadManager.Instance.DownloadPages(a_chapters);
            UpdateAll();
        }

        private void UpdateWorksTab()
        {
            BindingList<WorkGridRow> list = (BindingList<WorkGridRow>)worksGridView.DataSource;

            var works = DownloadManager.Instance.Works.List;

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
            if (DownloadManager.Instance.Works.List.Any(w => w.IsDownloading))
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
            Settings.Instance.MangaSettings.SetMangaRootDir(mangaRootDirTextBox.Text);

            if (!IsDirectoryPathValid())
                mangaRootDirTextBox.BackColor = BAD_DIR;
            else
                mangaRootDirTextBox.BackColor = SystemColors.Window;
        }

        private void chaptersListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                DownloadChapters(GetSelectedChapters());

            if ((e.KeyCode == Keys.A) && (e.Control))
                chaptersListBox.SelectAll();
        }

        private void serverURLButton_Click(object sender, EventArgs e)
        {
            VisitPage(SelectedServer);
        }

        private void seriesURLButton_Click(object sender, EventArgs e)
        {
            var serie = SelectedSerie;
            if (serie == null)
            {
                if (seriesListBox.Items.Count == 1)
                    serie = (seriesListBox.Items[0] as SerieListItem).Serie;
            }

            VisitPage(serie);
        }

        private void VisitPage(Entity a_entity)
        {
            VisitPages(new[] { a_entity });
        }

        private void chapterURLButton_Click(object sender, EventArgs e)
        {
            VisitPages(GetSelectedChapters());
        }

        private void chaptersListBox_DoubleClick(object sender, EventArgs e)
        {
            DownloadChapters(GetSelectedChapters());
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
            Settings.Instance.MangaSettings.UseCBZ = cbzCheckBox.Checked;
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
            SystemSounds.Beep.Play();
            tabControl.SelectedTab = optionsTabPage;
            Color c1 = mangaRootDirTextBox.BackColor;
            Color c2 = (c1 == BAD_DIR) ? SystemColors.Window : BAD_DIR;
            Pulse(c1, c2, 4, 500, (c) => mangaRootDirTextBox.BackColor = c);
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            if (DownloadManager.Instance.IsDownloading())
            {
                if (m_refresh_once_after_all_done)
                    Loggers.GUI.Debug("Starting refreshing");
                m_refresh_once_after_all_done = false;
                m_working = DownloadManager.Instance.Works.List.Any();
            }
            else if (!m_refresh_once_after_all_done)
            {
                Loggers.GUI.Debug("Stopping refreshing");
                m_refresh_once_after_all_done = true;
            }
            else
            {
                if (m_working)
                {
                    m_working = false;
                    SystemSounds.Beep.Play();
                }

                return;
            }

            DownloadManager.Instance.Works.Save();
            DownloadManager.Instance.Bookmarks.Save();

            UpdateAll();
        }

        private void UpdateAll()
        {
            if (tabControl.SelectedTab == optionsTabPage)
                UpdateOptions();
            else if (tabControl.SelectedTab == worksTabPage)
                UpdateWorksTab();
            if (tabControl.SelectedTab == seriesTabPage)
            {
                UpdateServers();
                UpdateSeries();
                UpdateChapters();
            }
            else if (tabControl.SelectedTab == bookmarksTabPage)
            {
                UpdateSerieBookmarks();
                UpdateChapterBookmarks();
            }
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
            var servers = (from server in DownloadManager.Instance.Servers
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
            UpdateAll();
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
            splitter.SplitPosition = Settings.Instance.SplitterDistance;
            splitterBooks.SplitPosition = Settings.Instance.SplitterBookmarksDistance;

            if (Catalog.GetCatalogSize() > Settings.Instance.MangaSettings.MaxCatalogSize)
                new CatalogOptimizeForm().ShowDialog();
        }

        private void openServerFolderButton_Click(object sender, EventArgs e)
        {
            OpenFolder(SelectedServer);
        }

        private void OpenFolders(IEnumerable<Entity> a_entities)
        {
            if (a_entities.Count() == 0)
            {
                SystemSounds.Beep.Play();
                return;
            }

            bool error = false;

            foreach (var entity in a_entities)
            {

                try
                {
                    Process.Start(entity.GetDirectory());
                }
                catch (Exception ex)
                {
                    Loggers.GUI.Error("Exception", ex);
                    error = true;
                }
            }

            if (error)
                SystemSounds.Beep.Play();
        }

        private void OpenFolder(Entity a_entity)
        {
            OpenFolders(new[] { a_entity });
        }

        private void openSeriesFolderButton_Click(object sender, EventArgs e)
        {
            var serie = SelectedSerie;
            if (serie == null)
            {
                if (seriesListBox.Items.Count == 1)
                    serie = (seriesListBox.Items[0] as SerieListItem).Serie;
            }

            OpenFolder(serie);
        }

        private void viewPagesButton_Click(object sender, EventArgs e)
        {
            ViewChapters(GetSelectedChapters());
        }

        private void ViewChapters(IEnumerable<Chapter> a_chapters)
        {
            if (a_chapters.Count() == 0)
            {
                SystemSounds.Beep.Play();
                return;
            }

            bool error = false;

            foreach (var chapter in a_chapters)
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
                                error = true;
                            }
                        }
                        else
                            error = true;
                    }
                    else
                        error = true;
                }
                else
                    error = true;
            }

            if (error)
                SystemSounds.Beep.Play();
        }

        private void openPagesFolder_Click(object sender, EventArgs e)
        {
            OpenFolders(GetSelectedChapters());
        }

        private IEnumerable<Chapter> GetSelectedChapters()
        {
            var chapters = chaptersListBox.SelectedItems.Cast<ChapterListItem>().Select(c => c.Chapter);

            if (chapters.Count() == 0)
            {
                if (chaptersListBox.Items.Count == 1)
                    chapters = new List<Chapter>() { (chaptersListBox.Items[0] as ChapterListItem).Chapter };
            }

            return chapters;
        }

        private void pageNamingStrategyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pageNamingStrategyComboBox.SelectedIndex == 0)
                Settings.Instance.MangaSettings.PageNamingStrategy = PageNamingStrategy.DoNothing;
            else if (pageNamingStrategyComboBox.SelectedIndex == 1)
                Settings.Instance.MangaSettings.PageNamingStrategy = PageNamingStrategy.PrefixWithIndexWhenNotOrdered;
            else if (pageNamingStrategyComboBox.SelectedIndex == 2)
                Settings.Instance.MangaSettings.PageNamingStrategy = PageNamingStrategy.OnlyIndex;
            else
                Loggers.GUI.Error("Invalid PageNamingStrategy");
        }

        private void UpdateOptions()
        {
            bool show = DownloadManager.Instance.Works.List.All(w => !w.IsDownloading);

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

        private List<Chapter> GetSelectedWorks()
        {
            var works = worksGridView.SelectedRows.Cast<DataGridViewRow>().Select(
                r => r.DataBoundItem).Cast<WorkGridRow>().Select(w => w.Chapter).ToList();

            if (works.Count == 0)
            {
                BindingList<WorkGridRow> list = (BindingList<WorkGridRow>)worksGridView.DataSource;
                if (list.Count == 1)
                    works.Add(list[0].Chapter);
            }

            return works;
        }

        private void viewWorkButton_Click(object sender, EventArgs e)
        {
            ViewChapters(GetSelectedWorks());
        }

        private void visitPageWorkButton_Click(object sender, EventArgs e)
        {
            VisitPages(GetSelectedWorks());
        }

        private void VisitPages(IEnumerable<Entity> a_entitites)
        {
            a_entitites = a_entitites.Where(e => e != null);

            if (a_entitites.Count() == 0)
            {
                SystemSounds.Beep.Play();
                return;
            }

            bool error = false;

            foreach (var entity in a_entitites)
            {
                try
                {
                    Process.Start(entity.URL);
                }
                catch (Exception ex)
                {
                    Loggers.GUI.Error("Exception", ex);
                    error = true;
                }
            }

            if (error)
                SystemSounds.Beep.Play();
        }

        private void deleteWorkButton_Click(object sender, EventArgs e)
        {
            DeleteSelectedWorks();
        }

        private void openFolderWorksButton_Click(object sender, EventArgs e)
        {
            OpenFolders(GetSelectedWorks());
        }

        private void goToChaptersWorkButton_Click(object sender, EventArgs e)
        {
            if (GetSelectedWorks().Count != 1)
            {
                SystemSounds.Beep.Play();
                return;
            }

            var chapter = GetSelectedWorks().First();

            tabControl.SelectTab(seriesTabPage);

            serversListBox.SelectedItem = 
                serversListBox.Items.Cast<ServerListItem>().FirstOrDefault(s => s.Server == chapter.Server);

            if (serversListBox.SelectedItem == null)
            {
                SystemSounds.Beep.Play();
                return;
            }

            seriesListBox.SelectedItem = 
                seriesListBox.Items.Cast<SerieListItem>().FirstOrDefault(s => s.Serie == chapter.Serie);

            if (seriesListBox.SelectedItem == null)
            {
                SystemSounds.Beep.Play();
                return;
            }

            chaptersListBox.SelectedItem = 
                chaptersListBox.Items.Cast<ChapterListItem>().FirstOrDefault(c => c.Chapter == chapter);

            if (chaptersListBox.SelectedItem == null)
                SystemSounds.Beep.Play();
        }

        private void downloadWorkButton_Click(object sender, EventArgs e)
        {
            DownloadChapters(GetSelectedWorks());
        }

        private void worksGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteSelectedWorks();
        }

        private void DeleteSelectedWorks()
        {
            var works = GetSelectedWorks();

            if (!works.Any())
            {
                SystemSounds.Beep.Play();
                return;
            }

            foreach (var work in works)
            {
                if (work.IsDownloading)
                    work.DeleteWork();
                else
                    DownloadManager.Instance.Works.Remove(work);
            }

            UpdateAll();
        }

        private void splitter1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (splitPanel.Width - splitter.SplitPosition < chaptersPanel.MinimumSize.Width)
                splitter.SplitPosition = splitPanel.Width - chaptersPanel.MinimumSize.Width;
            Settings.Instance.SplitterDistance = splitter.SplitPosition;
        }

        private void MangaCrawlerForm_ResizeEnd(object sender, EventArgs e)
        {
            if (chaptersPanel.Bounds.Right > splitPanel.ClientRectangle.Right)
                splitter.SplitPosition = splitPanel.Width - chaptersPanel.MinimumSize.Width;
        }

        private void splitterBooks_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (splitBookmarksPanel.Width - splitterBooks.SplitPosition < chapterBookmarksPanel.MinimumSize.Width)
                splitterBooks.SplitPosition = splitBookmarksPanel.Width - chapterBookmarksPanel.MinimumSize.Width;

            Settings.Instance.SplitterBookmarksDistance = splitterBooks.SplitPosition;
        }

        private void BookmarkSerieButton_Click(object sender, EventArgs e)
        {
            var serie = SelectedSerie;
            if (serie == null)
            {
                if (seriesListBox.Items.Count == 1)
                    serie = (seriesListBox.Items[0] as SerieListItem).Serie;
            }

            DownloadManager.Instance.Bookmarks.Add(serie);

            UpdateAll();
        }

        private void UpdateSerieBookmarks()
        {
            var ar = from bookmark in DownloadManager.Instance.Bookmarks.List
                     select new SerieBookmarkListItem(bookmark);

            new ListBoxVisualState(serieBookmarksListBox).ReloadItems(
                ar.OrderBy(b => b.Serie.ToString()));
        }    

        private void UpdateChapterBookmarks()
        {
            ChapterBookmarkListItem[] ar = new ChapterBookmarkListItem[0];

            if (SelectedSerieBookmark != null)
            {
                ar = (from chapter in SelectedSerieBookmark.Chapters
                      select new ChapterBookmarkListItem(chapter)).ToArray();
            }

            new ListBoxVisualState(chapterBookmarksListBox).ReloadItems(ar);
        }

        private void removeSerieBooksPanel_Click(object sender, EventArgs e)
        {
            RemoveBookmark();
        }

        private void openSerieFolderBooksButton_Click(object sender, EventArgs e)
        {
            var serie = SelectedSerieBookmark;

            if (serie == null)
            {
                if (seriesListBox.Items.Count == 1)
                    serie = (seriesListBox.Items[0] as SerieBookmarkListItem).Serie;
            }

            OpenFolder(serie);
        }

        private void visitSerieBooksButton_Click(object sender, EventArgs e)
        {
            var serie = SelectedSerieBookmark;
            if (serie == null)
            {
                if (seriesListBox.Items.Count == 1)
                    serie = (seriesListBox.Items[0] as SerieBookmarkListItem).Serie;
            }

            VisitPage(serie);
        }

        private IEnumerable<Chapter> GetSelectedBookmarkChapters()
        {
            var chapters = chapterBookmarksListBox.SelectedItems.Cast<ChapterBookmarkListItem>().Select(c => c.Chapter);

            if (chapters.Count() == 0)
            {
                if (chapterBookmarksListBox.Items.Count == 1)
                    chapters = new List<Chapter>() { (chapterBookmarksListBox.Items[0] as ChapterBookmarkListItem).Chapter };
            }

            return chapters;
        }

        private void downloadChapterBooksButton_Click(object sender, EventArgs e)
        {
            DownloadChapters(GetSelectedBookmarkChapters());
        }

        private void visitChapterBooksButton_Click(object sender, EventArgs e)
        {
            VisitPages(GetSelectedBookmarkChapters());

            foreach (var chapter in GetSelectedBookmarkChapters())
                chapter.BookmarkIgnored = true;

            UpdateAll();
        }

        private void openChapterFolderBooksButton_Click(object sender, EventArgs e)
        {
            OpenFolders(GetSelectedBookmarkChapters());
        }

        private void viewChapterBoksButton_Click(object sender, EventArgs e)
        {
            ViewChapters(GetSelectedBookmarkChapters());
        }

        private void serieBookmarksListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            ((sender as ListBox).Items[e.Index] as ListItem).DrawItem(e);
        }

        private void chapterBookmarksListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            ((sender as ListBox).Items[e.Index] as ListItem).DrawItem(e);
        }

        private void serieBookmarksListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedSerieBookmark != null)
                DownloadManager.Instance.DownloadChapters(SelectedSerieBookmark);

            UpdateAll();

            if (SelectedSerieBookmark != null)
            {
                ListBoxVisualState vs;
                if (m_chapter_bookmarks_visual_states.TryGetValue(SelectedSerieBookmark, out vs))
                    vs.Restore();
            }
        }

        private void chapterBookmarksListBox_DoubleClick(object sender, EventArgs e)
        {
            DownloadChapters(GetSelectedBookmarkChapters());
        }

        private void chapterBookmarksListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedSerieBookmark != null)
                m_chapter_bookmarks_visual_states[SelectedSerieBookmark] =
                    new ListBoxVisualState(chapterBookmarksListBox);
        }

        private void chapterBookmarksListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                DownloadChapters(GetSelectedBookmarkChapters());

            if ((e.KeyCode == Keys.A) && (e.Control))
                chapterBookmarksListBox.SelectAll();
        }

        private void chapterBookmarksListBox_VerticalScroll(object a_sender, bool a_tracking)
        {
            if (SelectedSerieBookmark != null)
                m_chapter_bookmarks_visual_states[SelectedSerieBookmark] =
                    new ListBoxVisualState(chapterBookmarksListBox);
        }

        private void serieBookmarksListBox_KeyDown(object sender, KeyEventArgs e)
        {
            RemoveBookmark();
        }

        private void RemoveBookmark()
        {
            if (SelectedSerieBookmark == null)
            {
                SystemSounds.Beep.Play();
                return;
            }

            DownloadManager.Instance.Bookmarks.Remove(SelectedSerieBookmark);

            UpdateAll();
        }

        private void resetCheckDatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_ResetCheckDate();
        }

        private void addSerieFirsttoolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_InsertSerie(0, SelectedServer);
        }

        private void addSerieMiddleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_InsertSerie(
                new Random().Next(1, SelectedServer.Series.Count - 1), SelectedServer);
        }

        private void addSerieLastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_InsertSerie(SelectedServer.Series.Count, SelectedServer);
        }

        private void removeSerieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_RemoveSerie(SelectedServer, SelectedSerie);
        }

        private void addChapterFirstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_InsertChapter(0, SelectedSerie);
        }

        private void addChapterMiddleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_InsertChapter(
                new Random().Next(1, SelectedSerie.Chapters.Count - 1), SelectedSerie);
        }

        private void addChapterLastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_InsertChapter(SelectedSerie.Chapters.Count, SelectedSerie);
        }

        private void removeChapterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_RemoveChapter(SelectedChapter);
        }

        private void renameSerieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_RenameSerie(SelectedSerie);
        }

        private void renameChapterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_RenameChapter(SelectedChapter);
        }

        private void changeSerieURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_ChangeSerieURL(SelectedSerie);
        }

        private void changeChapterURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_ChangeChapterURL(SelectedChapter);
        }
    }
}

