using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using MangaCrawlerLib;
using System.Threading.Tasks;
using System.ComponentModel;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using System.IO;
using MangaCrawler.Properties;
using System.Diagnostics;
using System.Media;
using HtmlAgilityPack;
using System.Threading;
using NHibernate.Linq;
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
     * po kliknieciu w chapter, podczas browse, sprawdzay czy mamy wszystkie pliki, spwadzamy czy po stronie 
     * serwera nic sie nie zmienilo, ale nie pobieramy kazdeo obrazka na nowo by sprawdzic czy sie zmienil, moze 
     * wystarczy srawdzic date ostatniej modyfikacji pliku, jego rozmiar, lub jakis html text z data, dac mozliwosc 
     * pelnej recznej weryfikacji serii i rozdzialu
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
     * oznaczanie w specjalny sposob juz pobranych serwerow, seri, chapterow (miedzy uruchomieniami)
     * dla serwerow, serii, chapterow, pagow weryfikacja zdalna tylko raz na uruchomienie, albo np. dla pracy w tle
     * kasowanie oznaczen co godzine
     * 
     * czy dac mozliwosc uzytkownikowi zdecydowania o predkosci pobierania i ilosci polaczen do nawiazania
     * 
     * taski, z tym mam problem, kiedy klikniemy na chapter i w niego wejdziemy poprzz browser to jest on pobierany 
     * tak dlugo jak w nim jestesmy i nie jest na liscie taskow, taski to zupelnie inny mechanizm kiedy dorzucamy cos do 
     * sciagania w tle, nie powinny one znikac po zakonczeniu, chyba ze dodamy taka opcje, powinny miec one nizszy priorytet
     * niz klikniecia usera
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
     * 
     * pamietanie taskow podczas zamkniecia i ich wznawianie
     * w przypadku ponownego uruchomienia jesli sa zadania otwarte to pokazac zakladke zadan albo wyswietlic message
     * boxa bo to nie jest typowa sytuacja
     * 
     * zmiana katalogu bazowego - o zrobic z aktualnymi danymi, przeniesc albo skasowac, pytac sie usera najlepiej
     * 
     * totalnie przerobic pobieranie, recznie stowrzyc potrzebna pule watkow i recznie odpalac kolejne zadania na nich, 
     * recznie decydowac o priorytetach, zlikwidowac takze podzial zadan na watki, tak by rzeczy sciagaly sie jeden po drugim, 
     * a nie podzielone zostaly na dwie polwki (partitioner)
     * 
     * jak usuwac skonczone worksy z downloadmanagera, najlepiej tak blednych nie usuwac wogole, skonczone usuwac 
     * tylko jesli tak zostalo zaznaczone
     * 
     * dodac przycisk przejdz do katalogu, dla calej bazy, servera, serii, chapteru
     * 
     * dodac mechanizm oszczedzania pamieci, albo DownloadManaer.Server za kazdym razem pobierane z bazy (wolnee) albo
     * pamietac zaznaczane idki, lub inaczej te do pobierania i okresowo kasowac nieuzywane czesci drzewa, co pewien 
     * czas, co pewna ilosc, co pewne zuzcie pamieci
     * 
     * maksymalna ilosc polaczen, teraz jest sto, jaka powinna byc racjonalna ilosc
     */

    public partial class MangaCrawlerForm : Form
    {
        private Dictionary<int, ListBoxVisualState> m_series_visual_states =
            new Dictionary<int, ListBoxVisualState>();
        private Dictionary<int, ListBoxVisualState> m_chapters_visual_states =
            new Dictionary<int, ListBoxVisualState>();

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
            NH.SetupFromFile(Settings.GetSettingsDir());

            Task.Factory.StartNew(() => CheckNewVersion(), TaskCreationOptions.LongRunning);

            if (!Log())
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

            refreshTimer.Enabled = true;

            UpdateSeriesTab();
        }

        private bool Log()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
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
                ListBoxVisualState vs;
                if (m_series_visual_states.TryGetValue(SelectedServer.ID, out vs))
                    vs.Restore();
            }
        }

        private void seriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedServer != null)
                m_series_visual_states[SelectedServer.ID] = new ListBoxVisualState(seriesListBox);
            
            DownloadManager.DownloadChapters(SelectedSerie);

            UpdateChapters();

            if (SelectedSerie != null)
            {
                ListBoxVisualState vs;
                if (m_chapters_visual_states.TryGetValue(SelectedSerie.ID, out vs))
                    vs.Restore();
            }
        }

        private void chaptersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedSerie != null)
                m_chapters_visual_states[SelectedSerie.ID] = new ListBoxVisualState(chaptersListBox);
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
                m_series_visual_states[SelectedServer.ID] = new ListBoxVisualState(seriesListBox);
        }

        private void chaptersListBox_VerticalScroll(object a_sender, bool a_tracking)
        {
            if (SelectedSerie != null)
                m_chapters_visual_states[SelectedSerie.ID] = new ListBoxVisualState(chaptersListBox);
        }

        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            ((sender as ListBox).Items[e.Index] as ListItem).DrawItem(e);
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
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            if (serversListBox.SelectedItem == null)
                return;

            var t = TomanuExtensions.Utils.Profiler.Measure(() =>
            {
                UpdateWorksTab();
                UpdateSeriesTab();
            });

            Loggers.GUI.InfoFormat("refresh time: {0} [ms]", t);
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
                NH.TransactionLock(SelectedSerie, () =>
                {
                    ar = (from chapter in SelectedSerie.GetChapters()
                          select new ChapterListItem(chapter)).ToArray();
                });
            }

            new ListBoxVisualState(chaptersListBox).ReloadItems(ar);
        }

        private void UpdateServers()
        {
            if (!ShowingSeriesTab)
                return;

            var servers = (from server in DownloadManager.Servers
                           select new ServerListItem(server)).ToArray();

            new ListBoxVisualState(serversListBox).ReloadItems(servers);
        }

        private void UpdateSeries()
        {
            if (!ShowingSeriesTab)
                return;

            SerieListItem[] ar = new SerieListItem[0];

            if (SelectedServer != null)
            {
                NH.TransactionLock(SelectedServer, () =>
                {
                    string filter = seriesSearchTextBox.Text.ToLower();
                    ar = (from serie in SelectedServer.GetSeries()
                          where serie.Title.ToLower().IndexOf(filter) != -1
                          select new SerieListItem(serie)).ToArray();
                });
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
    }
}
