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
     * wpf
     * 
     * instalator, x86, x64
     * deinstalacja powinna usunac katalog
     * przygotowac nie instalke
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
     */
    
    /*
     * TESTY
     * 
     * usuniecie serii dodanej do bookmark - odswiezenie w series tab  jak i w bookmark tab
     *   w series tab po zmianie serwera powinna zniknac, w bookmarks powina zniknac po okresowym sprawdzeniu 
     *   dostepnosci nowych chapterow, podobnie w zakladce serie tab jesli nie przeklikamy serwerow powinna zniknac 
     *   po okresowym sprawdzeniu bookmarks
     *   
     * usuniecie rozdzialu z bookmark serii ktory jest nowy powinno zdjac znacznik new
     * 
     * dodawanie do bookmark serii ma nie dzialac podczas pobierania serii.
     * 
     * usuniecie z bookmark zachowuje zaznaczenie w liscie
     * 
     * dodanie do bookmark dwa razy tego samego to blad
     * 
     * download (oba przyciski) juz pobieranych rzeczy to blad
     * 
     * dodanie nowego chapteru powinno spowodowac pojawienie sie nowego rozdzialu w bookmarks
     * 
     * dodanie nowego chapteru w minimalizacji powinno pokazac tooltip
     * 
     * klikniecie w tooltip powinno pokazac zkladke bookmarks, serie i nowy rozdzial, 
     *   kiedy cos tam jest wybrane powinno zniknac
     *   
     * pojawienie sie czegos nowego niezaleznie od stanu minimalizajci zakladki, a takze znikniecie tego 
     * powinno zmieniac ikone 
     * 
     * przetestowac elementy puste i elementy generujace bledy przy probie dostepu, sprobowac je dodac do 
     * bookmarks i odswiezyc
     * 
     * zamkniecie na downloading powinno wznowic pobieranie chapteru od poczatku
     * 
     * usuwanie wpisow z downloading: deleted - automatycznie, error, 
     *   downloaded - na rzadanie albo podczas nastepnego uruchomienia, 
     *   downloading - mozna klika tak dlugo jak wejdzie w stan deleting, pozniej dzwiek
     *   
     * wpisy na itemach z listboxach - deleted- trwale zostaje, 
     * downloaded - trwale zostaje, error - trwale zostaje, wszystkie ingi znikaja
     * 
     * priorytety - zalaczyc do pobierania serie - wiele na raz, rozdzialy z jednej serii - wiele na raz, 
     * chaptery dla kazdego serwera - w kolejnosci dodania pojedynczo, dla kazdego chapteru wiele stron na raz, 
     * serie, chaptery maja pierwszenstwo nad stronami, pobieranie chapterow wstrzymuje pobierania serii
     * 
     * ponowic pobranie anulowane rozdzialu
     * 
     * masowe anulowanie rozdzialow z dwoch serwerow, ich ponowne zalaczenie do pobierania
     * 
     * przetestowanie timera na sprawdzanie bookmarka, przetestowanie czasu zanim ponownie sprawdzimy juz pobrane
     * 
     * testy na manga dir ze i bez slasha na koncu
     * 
     * przetestowac detekcje nowej wersji
     * 
     * przetestowanie pamietania i dzialania opcji
     * 
     * dodanie ponownie tego samego pobierania nie wstawia nowego work, pozycja work nie zmienia sie
     * 
     * jesli cbz juz istnieje powinien zostac nadpisany
     * 
     * dzwiek o zakonczeniu pobierania takze jesli pobieramy od startu
     * 
     * zaznaczenie elementu w chapte, przejscie na inna serie, zaznaczenie innego indeksu, przejscie spowrotem - w chapter 
     * powinien byc tylko jeden chapter
     * 
     * usuwanie nieistniejacej zdalnie rzeczy, a zaznaczenie, w bookmark co sie dzieje jesli jednoczesnie jest bookmark, 
     * czy bookmark.xml sie czysci, co jesli zaznaczenie to ostatni item, 
     * 
     * upewnic sie ze uzywane w gui enumeracje sa stale, utrwalane w momencie pobrania, tak by nic nie zmienialo sie 
     * podczas enumeracji, jesli jakies zmiany wprowadza watki pobierajace, dotyczy chapterow dla ktorych jest multiselekcja
     * 
     * przetestowac dzialanie wszystkich przyciskow dla selekcji i multiselekcj, brak wybrania, 
     *   juz zostalo zrobione (dodane, uruchomione)
     *   
     * menu debug nie dziala i nie ma logowania w release
     * 
     * *****************************
     * 
     * zmiana nazwy, zmiana url - nie tracimy powiazania, zmiana obu - trudno
     * 
     * brak dostepu do katalogu - nie mozna utworzyc katalogu, nie mozna zapisac pliku w katalogu, nie mozna 
     * zapisac cbz, nie mozna podmienic pliku image, nie mozna podmienic pliku cbz
     * 
     * potestowac jak dzialaja zywe serwery
     * 
     * testy masowego pobierania cala noc
     * 
     * ktos klika w element ktory nie istnieje, pojawia sie error, albo jest on w trakcie sciagania, w tym 
     * czasie nastepuje jego odswiezenie i znika on z listy
     *  
     * uruchomienie aplikacji na czysto - sprawdzanie czy wszystk sie dobrze laduje
     * 
     * przetestowac compaktowanie 
     * - dodac jakies inne liki
     * - dodac xml o zlych nazwach
     * - wprowadzac bledy w xmlach
     * - czy usuwane sa sieroty
     * - czy usuwane sa chaptery bez imagow (nie ma byc all image)
     * - czy usuwane sa uste serie
     * - czy brute force usuwa te najstarsze
     * - czy brute force nie rusza do akcji jesli nie trzeba
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
        private bool m_force_close;
        private DateTime m_last_bookmark_check = DateTime.Now;
        private bool m_play_sound_when_downloaded;

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

            m_play_sound_when_downloaded = DownloadManager.Instance.Works.List.Any();

            mangaRootDirTextBox.Text = Settings.Instance.MangaSettings.GetMangaRootDir(false);
            seriesSearchTextBox.Text = Settings.Instance.SeriesFilter;
            cbzCheckBox.Checked = Settings.Instance.MangaSettings.UseCBZ;
            minimizeOnCloseCheckBox.Checked = Settings.Instance.MinimizeOnClose;
            if (!minimizeOnCloseCheckBox.Checked)
                showBaloonTipsCheckBox.Enabled = false;
            showBaloonTipsCheckBox.Checked = Settings.Instance.ShowBaloonTips;

            if (Settings.Instance.MangaSettings.PageNamingStrategy == PageNamingStrategy.DoNotChange)
                pageNamingStrategyComboBox.SelectedIndex = 0;
            else if (Settings.Instance.MangaSettings.PageNamingStrategy == PageNamingStrategy.PrefixToPreserverOrder)
                pageNamingStrategyComboBox.SelectedIndex = 1;
            else if (Settings.Instance.MangaSettings.PageNamingStrategy == PageNamingStrategy.IndexToPreserveOrder)
                pageNamingStrategyComboBox.SelectedIndex = 2;
            else if (Settings.Instance.MangaSettings.PageNamingStrategy == PageNamingStrategy.AlwaysUsePrefix)
                pageNamingStrategyComboBox.SelectedIndex = 3;
            else if (Settings.Instance.MangaSettings.PageNamingStrategy == PageNamingStrategy.AlwaysUseIndex)
                pageNamingStrategyComboBox.SelectedIndex = 4;
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

            CheckBookmarks(a_force: true);

            refreshTimer.Enabled = true;

            bookmarksTimer.Interval = (int)Settings.Instance.MangaSettings.CheckTimePeriod.TotalMilliseconds / 10;
            bookmarksTimer.Enabled = true;

            UpdateAll();
        }

        private void UpdateIcons()
        {
            if (DownloadManager.Instance.Bookmarks.GetSeriesWithNewChapters().Any())
                Icon = Icon.FromHandle(Resources.Manga_Crawler_Green.GetHicon());
            else
                Icon = Icon.FromHandle(Resources.Manga_Crawler_Orange.GetHicon());
            
            notifyIcon.Icon = Icon;
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
            DownloadManager.Instance.DownloadSeries(SelectedServer, a_force: false);

            UpdateAll();
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedServer != null)
                m_series_visual_states[SelectedServer] = 
                    new ListBoxVisualState(seriesListBox);

            DownloadManager.Instance.DownloadChapters(SelectedSerie, a_force: false);

            UpdateAll();
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
                SystemSounds.Asterisk.Play();
                return;
            }

            DownloadManager.Instance.DownloadPages(a_chapters);
            m_play_sound_when_downloaded = true;
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

            bool was_empty = list.Count == 0;

            foreach (var el in add)
                list.Add(new WorkGridRow(el));
            foreach (var el in remove)
                list.Remove(el);

            if (was_empty)
            {
                Debug.Assert(worksGridView.SelectedRows.Count != 0);
                worksGridView.ClearSelection();
            }

            worksGridView.Invalidate();
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
            SystemSounds.Asterisk.Play();
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

                    if (m_play_sound_when_downloaded)
                    {
                        m_play_sound_when_downloaded = false;

                        if (Settings.Instance.PlaySoundWhenDownloaded)
                            SystemSounds.Beep.Play();
                    }
                }

                return;
            }

            DownloadManager.Instance.Works.Save();
            ShowNotificationAboutNewChapters();
            UpdateIcons();

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

            UpdateIcons();
        }

        private void UpdateChapters()
        {
            ChapterListItem[] ar = new ChapterListItem[0];

            if (SelectedSerie != null)
            {
                ar = (from chapter in SelectedSerie.Chapters
                        select new ChapterListItem(chapter)).ToArray();
            }

            ListBoxVisualState vs = new ListBoxVisualState(chaptersListBox);
            vs.Clear();
            if (SelectedSerie != null)
            {
                if (m_chapters_visual_states.ContainsKey(SelectedSerie))
                    vs = m_chapters_visual_states[SelectedSerie];
            }
            vs.ReloadItems(ar);
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

            ListBoxVisualState vs = new ListBoxVisualState(seriesListBox);
            vs.Clear();
            if (SelectedServer != null)
            {
                if (m_series_visual_states.ContainsKey(SelectedServer))
                    vs = m_series_visual_states[SelectedServer];
            }
            vs.ReloadItems(ar);
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == bookmarksTabPage)
                splitterBookmarks.SplitPosition = Settings.Instance.SplitterBookmarksDistance;
            else if (tabControl.SelectedTab == seriesTabPage)
                splitter.SplitPosition = Settings.Instance.SplitterDistance;

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

            if (Catalog.GetCatalogSize() > Settings.Instance.MangaSettings.MaxCatalogSize)
                new CatalogOptimizeForm().ShowDialog();
        }

        private void openServerFolderButton_Click(object sender, EventArgs e)
        {
            OpenFolder(SelectedServer);
        }

        private void OpenFolders(IEnumerable<Entity> a_entities)
        {
            a_entities = a_entities.Where(e => e != null);

            if (a_entities.Count() == 0)
            {
                SystemSounds.Asterisk.Play();
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
                SystemSounds.Asterisk.Play();
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
                SystemSounds.Asterisk.Play();
                return;
            }

            bool error = false;

            foreach (var chapter in a_chapters)
            {
                if (chapter.Pages.Any())
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

            if (error)
                SystemSounds.Asterisk.Play();
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
                Settings.Instance.MangaSettings.PageNamingStrategy = PageNamingStrategy.DoNotChange;
            else if (pageNamingStrategyComboBox.SelectedIndex == 1)
                Settings.Instance.MangaSettings.PageNamingStrategy = PageNamingStrategy.PrefixToPreserverOrder;
            else if (pageNamingStrategyComboBox.SelectedIndex == 2)
                Settings.Instance.MangaSettings.PageNamingStrategy = PageNamingStrategy.IndexToPreserveOrder;
            else if (pageNamingStrategyComboBox.SelectedIndex == 3)
                Settings.Instance.MangaSettings.PageNamingStrategy = PageNamingStrategy.AlwaysUsePrefix;
            else if (pageNamingStrategyComboBox.SelectedIndex == 4)
                Settings.Instance.MangaSettings.PageNamingStrategy = PageNamingStrategy.AlwaysUseIndex;
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
            var works = worksGridView.Rows.Cast<DataGridViewRow>().Where(r => r.Selected).
                Select(r => r.DataBoundItem).Cast<WorkGridRow>().Select(w => w.Chapter).ToList();

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
                SystemSounds.Asterisk.Play();
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
                SystemSounds.Asterisk.Play();
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
            if (GetSelectedWorks().Count() != 1)
            {
                SystemSounds.Asterisk.Play();
                return;
            }

            var chapter = GetSelectedWorks().First();

            tabControl.SelectTab(seriesTabPage);

            serversListBox.SelectedItem = 
                serversListBox.Items.Cast<ServerListItem>().FirstOrDefault(s => s.Server == chapter.Server);

            if (serversListBox.SelectedItem == null)
                return;

            seriesListBox.SelectedItem = 
                seriesListBox.Items.Cast<SerieListItem>().FirstOrDefault(s => s.Serie == chapter.Serie);

            if (seriesListBox.SelectedItem == null)
                return;

            chaptersListBox.ClearSelected();
            chaptersListBox.SelectedItem = 
                chaptersListBox.Items.Cast<ChapterListItem>().FirstOrDefault(c => c.Chapter == chapter);
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

            if (!works.Any() || works.Any(c => c.State == ChapterState.Deleting))
            {
                SystemSounds.Asterisk.Play();
                return;
            }

            foreach (var work in works)
            {
                if (work.IsDownloading)
                {
                    work.DeleteWork();
                    m_play_sound_when_downloaded = false;
                }
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
            if (tabControl.SelectedTab == seriesTabPage)
            {
                if (chaptersPanel.Bounds.Right > splitPanel.ClientRectangle.Right)
                    splitter.SplitPosition = splitPanel.Width - chaptersPanel.MinimumSize.Width;
            }
            else if (tabControl.SelectedTab == bookmarksTabPage)
            {
                if (chapterBookmarksPanel.Bounds.Right > splitBookmarksPanel.ClientRectangle.Right)
                    splitterBookmarks.SplitPosition = splitBookmarksPanel.Width - chapterBookmarksPanel.MinimumSize.Width;
            }
        }

        private void splitterBookmarks_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (splitBookmarksPanel.Width - splitterBookmarks.SplitPosition < chapterBookmarksPanel.MinimumSize.Width)
                splitterBookmarks.SplitPosition = splitBookmarksPanel.Width - chapterBookmarksPanel.MinimumSize.Width;

            Settings.Instance.SplitterBookmarksDistance = splitterBookmarks.SplitPosition;
        }

        private void BookmarkSerieButton_Click(object sender, EventArgs e)
        {     
            var serie = SelectedSerie;
            if (serie == null)
            {
                if (seriesListBox.Items.Count == 1)
                    serie = (seriesListBox.Items[0] as SerieListItem).Serie;
            }

            if ((serie == null) || serie.IsDownloading || 
                DownloadManager.Instance.Bookmarks.List.Contains(serie))
            {
                SystemSounds.Asterisk.Play();
                return;
            }

            DownloadManager.Instance.Bookmarks.Add(serie);

            UpdateAll();
        }

        private void UpdateSerieBookmarks()
        {
            var bookmarks = (from bookmark in DownloadManager.Instance.Bookmarks.List
                             orderby new SerieBookmarkListItem(bookmark).ToString()
                             select new SerieBookmarkListItem(bookmark)).ToList();

            new ListBoxVisualState(serieBookmarksListBox).ReloadItems(bookmarks);
        }    

        private void UpdateChapterBookmarks()
        {
            ChapterBookmarkListItem[] ar = new ChapterBookmarkListItem[0];

            if (SelectedSerieBookmark != null)
            {
                ar = (from chapter in SelectedSerieBookmark.Chapters
                      select new ChapterBookmarkListItem(chapter)).ToArray();
            }

            ListBoxVisualState vs = new ListBoxVisualState(chapterBookmarksListBox);
            vs.Clear();
            if (SelectedSerieBookmark != null)
            {
                if (m_chapter_bookmarks_visual_states.ContainsKey(SelectedSerieBookmark))
                    vs = m_chapter_bookmarks_visual_states[SelectedSerieBookmark];
            }
            vs.ReloadItems(ar);
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
                DownloadManager.Instance.DownloadChapters(SelectedSerieBookmark, a_force: false);

            UpdateAll();
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
                SystemSounds.Asterisk.Play();
                return;
            }

            DownloadManager.Instance.Bookmarks.Remove(SelectedSerieBookmark);

            UpdateAll();
        }

        private void resetCheckDatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_last_bookmark_check = DateTime.MinValue;
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

        private void minimizeOnCloseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.MinimizeOnClose = minimizeOnCloseCheckBox.Checked;

            showBaloonTipsCheckBox.Enabled = minimizeOnCloseCheckBox.Checked;
        }

        private void exitTrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_force_close = true;
            Close();
        }

        private void MinimizeToTray(bool a_minimize)
        {
            if (a_minimize)
            {
                Hide();
                ShowInTaskbar = false;
                notifyIcon.Visible = true;
            }
            else
            {
                Show();
                ShowInTaskbar = true;
                notifyIcon.Visible = false;
            }
        }

        private void MangaCrawlerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_force_close)
                return;

            if (Settings.Instance.MinimizeOnClose)
            {
                MinimizeToTray(true);
                e.Cancel = true;
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                MinimizeToTray(false);
        }

        private void ShowNotificationAboutNewChapters()
        {
            if (Visible)
                return;
            if (!Settings.Instance.ShowBaloonTips)
                return;

            var new_chapters = (from serie in DownloadManager.Instance.Bookmarks.GetSeriesWithNewChapters()
                                orderby new SerieBookmarkListItem(serie).ToString()
                                select new SerieBookmarkListItem(serie)).ToArray();

            if (!new_chapters.Any())
                return;

            bool too_much = false;

            if (new_chapters.Length > 5)
            {
                too_much = true;
                new_chapters = new_chapters.Take(4).ToArray();
            }

            notifyIcon.Visible = true;

            string str = Resources.TrayNotificationNewSeries;
            str += Environment.NewLine + new_chapters.Skip(1).Aggregate(
                new_chapters.First().ToString(), (r, b) => r + Environment.NewLine + b.ToString());
            if (too_much)
                str += Environment.NewLine + "...";
 
            notifyIcon.ShowBalloonTip(5000, Application.ProductName, str, ToolTipIcon.Info);
        }

        private void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            MinimizeToTray(false);

            tabControl.SelectedTab = bookmarksTabPage;

            var serie = serieBookmarksListBox.Items.Cast<SerieBookmarkListItem>().FirstOrDefault(
                sbli => sbli.Serie.GetNewChapters().Any());

            if (serie != null)
            {
                serieBookmarksListBox.SelectedItem = serie;

                var chapter = chapterBookmarksListBox.Items.Cast<ChapterBookmarkListItem>().FirstOrDefault(
                    cbli => !cbli.Chapter.BookmarkIgnored);

                if (chapter != null)
                {
                    chapterBookmarksListBox.ClearSelected();
                    chapterBookmarksListBox.SelectedItem = chapter;
                }
            }
        }

        private void notifyIcon_BalloonTipClosed(object sender, EventArgs e)
        {
            if (notifyIcon.Visible && Visible)
                MinimizeToTray(false);
        }

        private void showBaloonTipsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.ShowBaloonTips = showBaloonTipsCheckBox.Checked;
        }

        private void bookmarksTimer_Tick(object sender, EventArgs e)
        {
            CheckBookmarks(a_force: false);
        }

        private void checkNowBookmarksButton_Click(object sender, EventArgs e)
        {
            CheckBookmarks(a_force: true);

            var chapter = chapterBookmarksListBox.Items.Cast<ChapterBookmarkListItem>().FirstOrDefault(
                cbli => !cbli.Chapter.BookmarkIgnored);

            if (chapter != null)
            {
                chapterBookmarksListBox.ClearSelected();
                chapterBookmarksListBox.SelectedItem = chapter;
            }
        }

        private void CheckBookmarks(bool a_force)
        {
            if (!a_force)
            {
                if (DateTime.Now - m_last_bookmark_check < Settings.Instance.CheckBookmarksPeriod)
                    return;
            }

            m_last_bookmark_check = DateTime.Now;

            foreach (var server in DownloadManager.Instance.Bookmarks.List.Select(s => s.Server).Distinct())
                DownloadManager.Instance.DownloadSeries(server, true);

            foreach (var serie in DownloadManager.Instance.Bookmarks.List)
                DownloadManager.Instance.DownloadChapters(serie, true);
        }

        private void forceBookmarksCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Task(() => 
            {
                Thread.Sleep(5000);

                Invoke(new Action(() =>
                {
                    SystemSounds.Exclamation.Play();
                    CheckBookmarks(a_force: true);
                }));
            }).Start();
        }

        private void playSoundWhenDownloadedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.PlaySoundWhenDownloaded = playSoundWhenDownloadedCheckBox.Checked;
        }

        private void worksGridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Rectangle r = worksGridView.GetCellDisplayRectangle(0, worksGridView.Rows.Count - 1, true);
                if (e.Y > r.Bottom)
                    worksGridView.ClearSelection();
            }
        }
    }
}

