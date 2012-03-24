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
using System.Runtime.InteropServices;
using TomanuExtensions;

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
     * zmiana nazwy, zmiana url - nie tracimy powiazania, zmiana obu - trudno
     * 
     * klikniecie prawym przyciskiem w liste zaznacza element przed pojawieniem sie menu kontekstowego
     * 
     * brak dostepu do katalogu - nie mozna utworzyc katalogu, nie mozna zapisac pliku w katalogu, nie mozna 
     * zapisac cbz, nie mozna podmienic pliku image, nie mozna podmienic pliku cbz
     * 
     * ktos klika w element ktory nie istnieje, pojawia sie error, albo jest on w trakcie sciagania, w tym 
     * czasie nastepuje jego odswiezenie i znika on z listy
     * 
     * zalaczenie do pobierania mnostwa chapterow, serii, itp zobaczyc czy nie bledow pamieciowych
     * 
     * wiele chapterow z jednego chaptera do sciagania, zamkniecie aplikacji, ponowne uruchomienie, 
     * czy wszystkie sa wznawiane, powinny sie wznawiac w kolejnosci dodania
     * 
     * /////////////
     * 
     * dodac przycisk check now
     * 
     * zlikwidowac przyciski, dodac context menu, porobic skrotty klawiszowe (del, enter), 
     * 
     * dodac warstwe posrednia zawierajaca akcje stojace za eventami - nazwac je komendami i zapakowac w obiekty
     * 
     * dodanie wielu chapterow wolne
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
     * /////////////////
     * 
     * potestowac jak dzialaja zywe serwery
     * testy masowego pobierania cala noc
     * uruchomienie aplikacji na czysto - sprawdzanie czy wszystk sie dobrze laduje
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
        private bool m_icon_created;
        private bool m_green_icon;

        private static Color BAD_DIR = Color.Red;

        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string a_name);

        private uint WM_TASKBARCREATED;

        public MangaCrawlerForm()
        {
            InitializeComponent();
            Settings.Instance.FormState.Init(this);
        }

        private void MangaShareCrawlerForm_Load(object sender, EventArgs e)
        {
            Debug.Assert(versionLinkLabel.Text == Resources.HomePage);

            WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");

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

        protected override void WndProc(ref Message a_msg)
        {
            if (a_msg.Msg == WM_TASKBARCREATED)
            {
                if (notifyIcon.Visible)
                    notifyIcon.Visible = true;
            }

            base.WndProc(ref a_msg);
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

        private void mangaRootDirChooseButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = mangaRootDirTextBox.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                mangaRootDirTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        private void serversListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DownloadSeriesForSelectedServer();
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedServer != null)
                m_series_visual_states[SelectedServer] = 
                    new ListBoxVisualState(seriesListBox);

            DownloadChapterForSelectedSerie();
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
            DownloadPagesForSelectedChapters();
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
                DownloadPagesForSelectedChapters();

            if ((e.KeyCode == Keys.A) && (e.Control))
                chaptersListBox.SelectAll();
        }

        private void serverURLButton_Click(object sender, EventArgs e)
        {
            VisitPageForSelectedServer();
        }

        private void seriesURLButton_Click(object sender, EventArgs e)
        {
            VisitPageForSelectedSerie();
        }

        private void chapterURLButton_Click(object sender, EventArgs e)
        {
            VisitPageForSelectedChapters();
        }

        private void chaptersListBox_DoubleClick(object sender, EventArgs e)
        {
            DownloadPagesForSelectedChapters();
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

        private void versionLinkLabel_LinkClicked(object sender, 
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

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            if (DownloadManager.Instance.NeedGUIRefresh(true))
            {
                m_refresh_once_after_all_done = false;
                m_working = DownloadManager.Instance.Works.List.Any();
            }
            else if (!m_refresh_once_after_all_done)
            {
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

                    DownloadManager.Instance.ClearCache();
                    GC.Collect();
                }

                return;
            }

            DownloadManager.Instance.Works.Save();
            ShowNotificationAboutNewChapters();
            UpdateIcons();

            UpdateAll();
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
            OpenFolderForSelectedServer();
        }

        private void openSeriesFolderButton_Click(object sender, EventArgs e)
        {
            OpenFolderForSelectedSerie();
        }

        private void viewPagesButton_Click(object sender, EventArgs e)
        {
            ViewPagesForSelectedChapters();
        }

        private void openPagesFolder_Click(object sender, EventArgs e)
        {
            OpenFolderForSelectedChapters();
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

        

        private Chapter[] SelectedWorks
        {
            get
            {
                var works = worksGridView.Rows.Cast<DataGridViewRow>().Where(r => r.Selected).
                    Select(r => r.DataBoundItem).Cast<WorkGridRow>().Select(w => w.Chapter).ToArray();

                return works;
            }
        }

        private void viewWorkButton_Click(object sender, EventArgs e)
        {
            ViewPagesForSelectedWorks();
        }

        private void visitPageWorkButton_Click(object sender, EventArgs e)
        {
            VisitPageForSelectedWorks();
        }

        private void cancelWorkButton_Click(object sender, EventArgs e)
        {
            CancelSelectedWorks();
        }

        private void openFolderWorksButton_Click(object sender, EventArgs e)
        {
            OpenFolderForSelectedWorks();
        }

        private void goToSeriesTabButton_Click(object sender, EventArgs e)
        {
            if (SelectedWorks.Length != 1)
            {
                SystemSounds.Asterisk.Play();
                return;
            }

            var chapter = SelectedWorks.First();

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
            DownloadPagesForSelectedWorks();
        }

        private void worksGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                CancelClearSelectedWorks();
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
            BookmarkSelectedSerie();
        }

        private void removeSerieBooksPanel_Click(object sender, EventArgs e)
        {
            RemoveSelectedBookmark();
        }

        private void openSerieFolderBooksButton_Click(object sender, EventArgs e)
        {
            var serie = SelectedSerieForBookmark;

            if (serie == null)
            {
                if (seriesListBox.Items.Count == 1)
                    serie = (seriesListBox.Items[0] as SerieBookmarkListItem).Serie;
            }

            OpenFolder(serie);
        }

        private void visitSerieBooksButton_Click(object sender, EventArgs e)
        {
            VisitPage(SelectedSerieForBookmark);
        }

        private void downloadChapterBooksButton_Click(object sender, EventArgs e)
        {
            DownloadPages(SelectedChaptersForBookmarks);
        }

        private void visitChapterBooksButton_Click(object sender, EventArgs e)
        {
            VisitPages(SelectedChaptersForBookmarks);

            foreach (var chapter in SelectedChaptersForBookmarks)
                chapter.BookmarkIgnored = true;

            UpdateAll();
        }

        private void openChapterFolderBooksButton_Click(object sender, EventArgs e)
        {
            OpenFolders(SelectedChaptersForBookmarks);
        }

        private void viewChapterBoksButton_Click(object sender, EventArgs e)
        {
            VisitPages(SelectedChaptersForBookmarks);
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
            if (SelectedSerieForBookmark != null)
                DownloadManager.Instance.DownloadChapters(SelectedSerieForBookmark, a_force: false);

            UpdateAll();
        }

        private void chapterBookmarksListBox_DoubleClick(object sender, EventArgs e)
        {
            DownloadPages(SelectedChaptersForBookmarks);
        }

        private void chapterBookmarksListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedSerieForBookmark != null)
                m_chapter_bookmarks_visual_states[SelectedSerieForBookmark] =
                    new ListBoxVisualState(chapterBookmarksListBox);
        }

        private void chapterBookmarksListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                DownloadPages(SelectedChaptersForBookmarks);

            if ((e.KeyCode == Keys.A) && (e.Control))
                chapterBookmarksListBox.SelectAll();
        }

        private void chapterBookmarksListBox_VerticalScroll(object a_sender, bool a_tracking)
        {
            if (SelectedSerieForBookmark != null)
                m_chapter_bookmarks_visual_states[SelectedSerieForBookmark] =
                    new ListBoxVisualState(chapterBookmarksListBox);
        }

        private void serieBookmarksListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                RemoveSelectedBookmark();
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
            foreach (var chapter in SelectedChapters)
                DownloadManager.Instance.Debug_RemoveChapter(chapter);
        }

        private void renameSerieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_RenameSerie(SelectedSerie);
        }

        private void renameChapterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var chapter in SelectedChapters)
                DownloadManager.Instance.Debug_RenameChapter(chapter);
        }

        private void changeSerieURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.Debug_ChangeSerieURL(SelectedSerie);
        }

        private void changeChapterURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var chapter in SelectedChapters)
                DownloadManager.Instance.Debug_ChangeChapterURL(chapter);
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
                notifyIcon.Visible = true;

                if (!m_working)
                {
                    DownloadManager.Instance.ClearCache();
                    GC.Collect();
                }
            }
            else
            {
                Show();
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

        // TODO: jesli wymusilismy sprawdzanie bookmarkow to zaznaczyc nowy chapter jesli jest jakis

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

        private void clearMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadManager.Instance.ClearCache();
            GC.Collect();
        }

        private void loadAllFromCatalogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int servers = 0;
            int series = 0;
            int chapters = 0;
            int pages = 0;
            DownloadManager.Instance.Debug_LoadAllFromCatalog(ref servers, ref series, ref chapters, ref pages);

            Loggers.GUI.InfoFormat("servers: {0}, series: {1}, chapters: {2}, pages: {3}", servers, series, chapters, pages);
        }

        private void clearWorkButton_Click(object sender, EventArgs e)
        {
            ClearAllWorks();
        }

        private void checkNowServerButton_Click(object sender, EventArgs e)
        {
            CheckNowSelectedServer();
        }

        private void checkNowSerieButton_Click(object sender, EventArgs e)
        {
            CheckNowForSelectedSerie();
        }
    }
}

