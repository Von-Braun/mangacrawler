using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;
using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;
using MangaCrawler.Properties;
using System.Drawing;
using System.Media;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Threading;
using TomanuExtensions;
using System.IO;

namespace MangaCrawler
{
	partial class MangaCrawlerForm
	{
        private class MangaCrawlerFormGUI
        {
            public MangaCrawlerForm Form;
            public MangaCrawlerFormCommands Commands;

            public bool PlaySoundWhenDownloaded;
            public bool RefreshOnceAfterAllDone;
            public bool Working;
            public bool ForceClose;
            public bool IconCreated;
            public DateTime LastBookmarkCheck = DateTime.Now;
            public bool GreenIcon;

            public void Init()
            {
                PlaySoundWhenDownloaded = DownloadManager.Instance.Works.List.Any();
            }

            public Server SelectedServer
            {
                get
                {
                    if (Form.serversListBox.SelectedItem == null)
                        return null;
                    else
                        return (Form.serversListBox.SelectedItem as ServerListItem).Server;
                }
            }

            public Serie SelectedSerie
            {
                get
                {
                    if (Form.seriesListBox.SelectedItem == null)
                        return null;
                    else
                        return (Form.seriesListBox.SelectedItem as SerieListItem).Serie;
                }
            }

            public Chapter[] SelectedChapters
            {
                get
                {
                    return Form.chaptersListBox.SelectedItems.Cast<ChapterListItem>().Select(c => c.Chapter).ToArray();
                }
            }

            public Serie SelectedBookmarkedSerie
            {
                get
                {
                    if (Form.serieBookmarksListBox.SelectedItem == null)
                        return null;
                    else
                        return (Form.serieBookmarksListBox.SelectedItem as SerieBookmarkListItem).Serie;
                }
            }

            public Chapter[] SelectedBookmarkedChapters
            {
                get
                {
                    return Form.chapterBookmarksListBox.SelectedItems.Cast<ChapterBookmarkListItem>().Select(
                        c => c.Chapter).ToArray();
                }
            }

            public Chapter[] SelectedWorks
            {
                get
                {
                    var works = Form.worksGridView.Rows.Cast<DataGridViewRow>().Where(r => r.Selected).
                        Select(r => r.DataBoundItem).Cast<WorkGridRow>().Select(w => w.Chapter).ToArray();

                    return works;
                }
            }

            public void UpdateAll()
            {
                if (Form.tabControl.SelectedTab == Form.optionsTabPage)
                    UpdateOptions();
                else if (Form.tabControl.SelectedTab == Form.worksTabPage)
                    UpdateWorksTab();
                if (Form.tabControl.SelectedTab == Form.seriesTabPage)
                {
                    UpdateServers();
                    UpdateSeries();
                    UpdateChapters();
                }
                else if (Form.tabControl.SelectedTab == Form.bookmarksTabPage)
                {
                    UpdateSerieBookmarks();
                    UpdateChapterBookmarks();
                }

                UpdateButtons();
                UpdateIcons();
            }

            public void UpdateButtons()
            {
                {
                    bool en = SelectedServer != null;

                    foreach (var item in Form.serversToolStrip.Items.OfType<ToolStripItem>())
                        item.Enabled = en;
                    foreach (var item in Form.serversContextMenuStrip.Items.OfType<ToolStripMenuItem>())
                        item.Enabled = en;

                    if (en)
                    {
                        Form.updateNowForSelectedServerToolStripButton.Enabled = 
                            !SelectedServer.IsDownloading;
                        Form.updateNowForSelectedServerToolStripMenuItem.Enabled =
                            Form.updateNowForSelectedServerToolStripButton.Enabled;

                        Form.openFolderForSelectedServerToolStripButton.Enabled =
                            new DirectoryInfo(SelectedServer.GetDirectory()).Exists;
                        Form.openFolderForSelectedServerToolStripMenuItem.Enabled =
                            Form.openFolderForSelectedServerToolStripButton.Enabled;
                    }
                }

                {
                    bool en = SelectedSerie != null;

                    foreach (var item in Form.seriesToolStrip.Items.OfType<ToolStripItem>())
                        item.Enabled = en;
                    foreach (var item in Form.seriesContextMenuStrip.Items.OfType<ToolStripMenuItem>())
                        item.Enabled = en;

                    if (en)
                    {
                        Form.bookmarkSelectedSerieToolStripButton.Enabled = 
                            !SelectedSerie.IsBookmarked && !SelectedSerie.IsDownloading;
                        Form.bookmarkSerieToolStripMenuItem.Enabled = 
                            Form.bookmarkSelectedSerieToolStripButton.Enabled;

                        Form.updateNowForSelectedSerieToolStripButton.Enabled = 
                            !SelectedSerie.IsDownloading;
                        Form.updateNowForSelectedSerieToolStripMenuItem.Enabled = 
                            Form.updateNowForSelectedSerieToolStripButton.Enabled;

                        Form.openFolderForSelectedSerieToolStripButton.Enabled =
                            new DirectoryInfo(SelectedSerie.GetDirectory()).Exists;
                        Form.openFolderForSelectedSerieToolStripMenuItem.Enabled =
                            Form.openFolderForSelectedSerieToolStripButton.Enabled;
                    }
                }

                {
                    bool en = SelectedChapters.Any();

                    foreach (var item in Form.chaptersToolStrip.Items.OfType<ToolStripItem>())
                        item.Enabled = en;
                    foreach (var item in Form.chaptersContextMenuStrip.Items.OfType<ToolStripMenuItem>())
                        item.Enabled = en;

                    if (en)
                    {
                        Form.openFolderForSelectedChaptersToolStripButton.Enabled =
                            SelectedChapters.Any(c => new DirectoryInfo(c.GetDirectory()).Exists);
                        Form.openFolderForSelectedChaptersToolStripMenuItem.Enabled =
                            Form.openFolderForSelectedChaptersToolStripButton.Enabled;

                        Form.downloadForSelectedChaptersToolStripButton.Enabled =
                            SelectedChapters.Any(c => !c.IsDownloading);
                        Form.downloadForSelectedChaptersToolStripMenuItem.Enabled =
                            Form.downloadForSelectedChaptersToolStripButton.Enabled;

                        Form.readMangaForSelectedChaptersToolStripMenuItem.Enabled =
                            SelectedChapters.Any(c => c.CanReadFirstPage());
                        Form.readMangaForSelectedChaptersToolStripButton.Enabled =
                            Form.readMangaForSelectedChaptersToolStripMenuItem.Enabled;
                    }
                }

                {
                    bool en = SelectedWorks.Any();

                    foreach (var item in Form.worksToolStrip.Items.OfType<ToolStripItem>())
                        item.Enabled = en;
                    foreach (var item in Form.worksContextMenuStrip.Items.OfType<ToolStripMenuItem>())
                        item.Enabled = en;

                    if (en)
                    {
                        Form.openFolderForSelectedWorksToolStripButton.Enabled =
                            SelectedWorks.Any(c => new DirectoryInfo(c.GetDirectory()).Exists);
                        Form.openFolderForSelectedWorksToolStripMenuItem.Enabled =
                            Form.openFolderForSelectedWorksToolStripButton.Enabled;

                        Form.downloadForSelectedWorksToolStripButton.Enabled =
                            SelectedWorks.Any(c => !c.IsDownloading);
                        Form.downloadForSelectedWorksToolStripMenuItem.Enabled =
                            Form.downloadForSelectedWorksToolStripButton.Enabled;

                        Form.readMangaForSelectedWorksToolStripMenuItem.Enabled =
                            SelectedWorks.Any(c => c.CanReadFirstPage());
                        Form.readMangaForSelectedWorksToolStripButton.Enabled =
                            Form.readMangaForSelectedWorksToolStripMenuItem.Enabled;

                        Form.deleteForSelectedWorksToolStripButton.Enabled =
                            SelectedWorks.Any(c => c.State != ChapterState.Cancelling);
                        Form.deleteForSelectedWorksToolStripMenuItem.Enabled =
                            Form.deleteForSelectedWorksToolStripButton.Enabled;

                        Form.showInSeriesForSelectedWorksToolStripButton.Enabled =
                            SelectedWorks.Count() == 1;
                        Form.showInSeriesForSelectedWorksToolStripMenuItem.Enabled =
                            Form.showInSeriesForSelectedWorksToolStripButton.Enabled;
                    }
                }

                {
                    bool en = SelectedBookmarkedSerie != null;

                    foreach (var item in Form.bookmarkedSeriesToolStrip.Items.OfType<ToolStripItem>())
                        item.Enabled = en;
                    foreach (var item in Form.bookmarkedSeriesContextMenuStrip.Items.OfType<ToolStripMenuItem>())
                        item.Enabled = en;

                    if (en)
                    {
                        Form.openFolderForSelectedBookmarkedSerieToolStripButton.Enabled =
                             new DirectoryInfo(SelectedBookmarkedSerie.GetDirectory()).Exists;
                        Form.openFolderForSelectedBookmarkedSerieToolStripMenuItem.Enabled =
                            Form.openFolderForSelectedBookmarkedSerieToolStripButton.Enabled;

                        Form.updateNowForSelectedBookmarkedSerieToolStripButton.Enabled = 
                            !SelectedBookmarkedSerie.IsDownloading;
                        Form.updateNowForSelectedBookmarkedSerieToolStripMenuItem.Enabled =
                            Form.updateNowForSelectedBookmarkedSerieToolStripButton.Enabled;
                    }
                }

                {
                    bool en = SelectedBookmarkedChapters.Any();

                    foreach (var item in Form.bookmarkedChaptersToolStrip.Items.OfType<ToolStripItem>())
                        item.Enabled = en;
                    foreach (var item in Form.bookmarkedChaptersContextMenuStrip.Items.OfType<ToolStripMenuItem>())
                        item.Enabled = en;

                    if (en)
                    {
                        Form.openFolderForSelectedBookmarkedChaptersToolStripButton.Enabled =
                            SelectedBookmarkedChapters.Any(c => new DirectoryInfo(c.GetDirectory()).Exists);
                        Form.openFolderForSelectedBookmarkedChaptersToolStripMenuItem.Enabled =
                            Form.openFolderForSelectedBookmarkedChaptersToolStripButton.Enabled;

                        Form.downloadForSelectedBookmarkedChaptersToolStripButton.Enabled =
                            SelectedBookmarkedChapters.Any(c => !c.IsDownloading);
                        Form.downloadForSelectedBookmarkedChaptersToolStripMenuItem.Enabled =
                            Form.downloadForSelectedBookmarkedChaptersToolStripButton.Enabled;

                        Form.readMangaForSelectedBookmarkedChaptersToolStripMenuItem.Enabled =
                            SelectedBookmarkedChapters.Any(c => c.CanReadFirstPage());
                        Form.readMangaForSelectedBookmarkedChaptersToolStripButton.Enabled =
                            Form.readMangaForSelectedBookmarkedChaptersToolStripMenuItem.Enabled;
                    }
                }
            }

            private void UpdateOptions()
            {
                bool show = DownloadManager.Instance.Works.List.All(w => !w.IsDownloading);

                foreach (var control in Form.optionsTabPage.Controls.Cast<Control>())
                {
                    if (control == Form.optionslLabel)
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

            private void UpdateWorksTab()
            {
                BindingList<WorkGridRow> list = (BindingList<WorkGridRow>)Form.worksGridView.DataSource;

                Form.worksGridView.SuspendDrawing();

                try
                {
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
                        if (Form.worksGridView.Rows.Count != 0)
                            Debug.Assert(Form.worksGridView.SelectedRows.Count != 0);
                        Form.worksGridView.ClearSelection();
                    }
                }
                finally
                {
                    Form.worksGridView.ResumeDrawing();
                }

                Form.worksGridView.Refresh();
            }

            private void UpdateChapters()
            {
                ChapterListItem[] ar = new ChapterListItem[0];

                if (SelectedSerie != null)
                {
                    ar = (from chapter in SelectedSerie.Chapters
                          select new ChapterListItem(chapter)).ToArray();
                }

                ListBoxVisualState vs = new ListBoxVisualState(Form.chaptersListBox);
                vs.Clear();
                if (SelectedSerie != null)
                {
                    if (Form.m_chapters_visual_states.ContainsKey(SelectedSerie))
                        vs = Form.m_chapters_visual_states[SelectedSerie];
                }
                vs.ReloadItems(ar);
            }

            private void UpdateServers()
            {
                var servers = (from server in DownloadManager.Instance.Servers
                               select new ServerListItem(server)).ToArray();

                new ListBoxVisualState(Form.serversListBox).ReloadItems(servers);
            }

            public void UpdateSeries()
            {
                SerieListItem[] ar = new SerieListItem[0];

                if (SelectedServer != null)
                {
                    string filter = Form.seriesSearchTextBox.Text.ToLower();
                    ar = (from serie in SelectedServer.Series
                          where serie.Title.ToLower().IndexOf(filter) != -1
                          select new SerieListItem(serie)).ToArray();
                }

                ListBoxVisualState vs = new ListBoxVisualState(Form.seriesListBox);
                vs.Clear();
                if (SelectedServer != null)
                {
                    if (Form.m_series_visual_states.ContainsKey(SelectedServer))
                        vs = Form.m_series_visual_states[SelectedServer];
                }
                vs.ReloadItems(ar);
            }

            public void UpdateIcons()
            {
                bool need_green = DownloadManager.Instance.Bookmarks.GetSeriesWithNewChapters().Any();

                if (IconCreated)
                {
                    if (need_green)
                    {
                        if (GreenIcon)
                            return;
                    }
                    if (!need_green)
                    {
                        if (!GreenIcon)
                            return;
                    }
                }

                IconCreated = true;
                GreenIcon = need_green;

                Icon old1 = Form.Icon;
                Icon old2 = Form.notifyIcon.Icon;

                if (need_green)
                    Form.Icon = Icon.FromHandle(Resources.Manga_Crawler_Green.GetHicon());
                else
                    Form.Icon = Icon.FromHandle(Resources.Manga_Crawler_Orange.GetHicon());
         
                Form.notifyIcon.Icon = Form.Icon;

                if (old1 != null)
                    old1.Dispose();
                if (old2 != null)
                    old2.Dispose();
            }

            private void UpdateSerieBookmarks()
            {
                var bookmarks = (from bookmark in DownloadManager.Instance.Bookmarks.List
                                 orderby new SerieBookmarkListItem(bookmark).ToString()
                                 select new SerieBookmarkListItem(bookmark)).ToList();

                new ListBoxVisualState(Form.serieBookmarksListBox).ReloadItems(bookmarks);
            }

            private void UpdateChapterBookmarks()
            {
                ChapterBookmarkListItem[] ar = new ChapterBookmarkListItem[0];

                if (SelectedBookmarkedSerie != null)
                {
                    ar = (from chapter in SelectedBookmarkedSerie.Chapters
                          select new ChapterBookmarkListItem(chapter)).ToArray();
                }

                ListBoxVisualState vs = new ListBoxVisualState(Form.chapterBookmarksListBox);
                vs.Clear();
                if (SelectedBookmarkedSerie != null)
                {
                    if (Form.m_chapter_bookmarks_visual_states.ContainsKey(SelectedBookmarkedSerie))
                        vs = Form.m_chapter_bookmarks_visual_states[SelectedBookmarkedSerie];
                }
                vs.ReloadItems(ar);
            }

            public void PulseMangaRootDirTextBox()
            {
                SystemSounds.Asterisk.Play();
                Form.tabControl.SelectedTab = Form.optionsTabPage;
                Color c1 = Form.mangaRootDirTextBox.BackColor;
                Color c2 = (c1 == BAD_DIR) ? SystemColors.Window : BAD_DIR;
                Pulse(c1, c2, 4, 500, (c) => Form.mangaRootDirTextBox.BackColor = c);
            }

            public void InformAboutNewVersion()
            {
                Action action = () => Form.versionLinkLabel.Text = Resources.NewVersion;
                Form.Invoke(action);
                Pulse(Form.versionLinkLabel.LinkColor, Color.Red, 12, 700, (c) => Form.versionLinkLabel.LinkColor = c);
            }

            public void MinimizeToTray(bool a_minimize)
            {
                if (a_minimize)
                {
                    Form.Hide();
                    Form.notifyIcon.Visible = true;
                }
                else
                {
                    Form.Show();
                    Form.notifyIcon.Visible = false;
                }
            }

            private void Pulse(Color a_color_org, Color a_color_alter, int a_count, int pulse_time_ms,
                Action<Color> a_action)
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
                            Form.Invoke(a_action, color);
                            Thread.Sleep(SLEEP_TIME);
                        }
                    }
                    catch
                    {
                        Loggers.GUI.Error("Exception");
                    }
                });
            }

            public void ShowNotificationAboutNewChapters()
            {
                if (Form.Visible)
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

                Form.notifyIcon.Visible = true;

                string str = Resources.TrayNotificationNewSeries;
                str += Environment.NewLine + new_chapters.Skip(1).Aggregate(
                    new_chapters.First().ToString(), (r, b) => r + Environment.NewLine + b.ToString());
                if (too_much)
                    str += Environment.NewLine + "...";

                Form.notifyIcon.ShowBalloonTip(5000, Application.ProductName, str, ToolTipIcon.Info);
            }

            private void SelectServer(Server a_server)
            {
                Form.serversListBox.SelectedItem =
                   Form.serversListBox.Items.Cast<ServerListItem>().FirstOrDefault(s => s.Server == a_server);
            }

            private void SelectSerie(Serie a_serie)
            {
                Form.seriesListBox.SelectedItem =
                    Form.seriesListBox.Items.Cast<SerieListItem>().FirstOrDefault(s => s.Serie == a_serie);
            }

            private void SelectChapter(Chapter a_chapter)
            {
                Form.chaptersListBox.ClearSelected();

                var cli = Form.chaptersListBox.Items.Cast<ChapterListItem>().FirstOrDefault(c => c.Chapter == a_chapter);

                if (cli == null)
                    return;

                Form.chaptersListBox.SelectedItem = cli;
            }

            public void SelectBookmarkedSerie(Serie a_serie)
            {
                Form.serieBookmarksListBox.SelectedItem =
                    Form.serieBookmarksListBox.Items.Cast<SerieBookmarkListItem>().FirstOrDefault(
                        sbli => sbli.Serie == a_serie);
            }

            public void SelectBookmarkedChapter(Chapter a_chapter)
            {
                var bli = Form.chapterBookmarksListBox.Items.Cast<ChapterBookmarkListItem>().FirstOrDefault(
                    cbli => cbli.Chapter == a_chapter);

                if (bli == null)
                    return;

                Form.chapterBookmarksListBox.ClearSelected();
                Form.chapterBookmarksListBox.SelectedItem = bli;
            }

            public void ShowInSeriesFromWorks()
            {
                if (SelectedWorks.Length != 1)
                {
                    SystemSounds.Asterisk.Play();
                    return;
                }

                var chapter = SelectedWorks.First();

                Form.tabControl.SelectTab(Form.seriesTabPage);

                SelectServer(chapter.Server);
                SelectSerie(chapter.Serie);
                SelectChapter(chapter);
            }

            public void Refresh()
            {
                if (DownloadManager.Instance.NeedGUIRefresh(true))
                {
                    RefreshOnceAfterAllDone = false;
                    Working = DownloadManager.Instance.Works.List.Any();
                }
                else if (!RefreshOnceAfterAllDone)
                {
                    RefreshOnceAfterAllDone = true;
                }
                else
                {
                    if (Working)
                    {
                        if (PlaySoundWhenDownloaded)
                        {
                            if (Settings.Instance.PlaySoundWhenDownloaded)
                                SystemSounds.Beep.Play();
                        }
                    }

                    PlaySoundWhenDownloaded = false;
                    Working = false;

                    return;
                }

                Commands.SaveWorks();
                ShowNotificationAboutNewChapters();
                UpdateAll();
            }

            public void Close(bool a_force_close)
            {
                ForceClose = a_force_close;
                Form.Close();
            }

            public void RefreshBookmarks()
            {
                if (DateTime.Now - LastBookmarkCheck < Settings.Instance.CheckBookmarksPeriod)
                    return;

                LastBookmarkCheck = DateTime.Now;

                Commands.CheckNowBookmarks();
            }

            public bool MinimizeOrClose()
            {
                if (!ForceClose && Settings.Instance.MinimizeOnClose)
                {
                    MinimizeToTray(true);
                    return true;
                }

                return false;
            }
        }
	}
}