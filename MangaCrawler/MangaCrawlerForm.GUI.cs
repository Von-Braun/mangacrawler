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

namespace MangaCrawler
{
	partial class MangaCrawlerForm
	{
        public Server SelectedServer
        {
            get
            {
                if (serversListBox.SelectedItem == null)
                    return null;
                else
                    return (serversListBox.SelectedItem as ServerListItem).Server;
            }
        }

        public Serie SelectedSerie
        {
            get
            {
                if (seriesListBox.SelectedItem == null)
                    return null;
                else
                    return (seriesListBox.SelectedItem as SerieListItem).Serie;
            }
        }

        private Chapter[] SelectedChapters
        {
            get
            {
                return chaptersListBox.SelectedItems.Cast<ChapterListItem>().Select(c => c.Chapter).ToArray();
            }
        }

        public Serie SelectedSerieForBookmark
        {
            get
            {
                if (serieBookmarksListBox.SelectedItem == null)
                    return null;
                else
                    return (serieBookmarksListBox.SelectedItem as SerieBookmarkListItem).Serie;
            }
        }

        private Chapter[] SelectedChaptersForBookmarks
        {
            get
            {
                return chapterBookmarksListBox.SelectedItems.Cast<ChapterBookmarkListItem>().Select(c => c.Chapter).ToArray();
            }
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
                if (worksGridView.Rows.Count != 0)
                    Debug.Assert(worksGridView.SelectedRows.Count != 0);
                worksGridView.ClearSelection();
            }

            worksGridView.Invalidate();
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

        private void UpdateIcons()
        {
            bool need_green = DownloadManager.Instance.Bookmarks.GetSeriesWithNewChapters().Any();

            if (m_icon_created)
            {
                if (need_green)
                {
                    if (m_green_icon)
                        return;
                }
                if (!need_green)
                {
                    if (!m_green_icon)
                        return;
                }
            }

            m_icon_created = true;

            Icon old1 = Icon;
            Icon old2 = notifyIcon.Icon;

            if (need_green)
            {
                Icon = Icon.FromHandle(Resources.Manga_Crawler_Green.GetHicon());
                m_green_icon = true;
            }
            else
            {
                Icon = Icon.FromHandle(Resources.Manga_Crawler_Orange.GetHicon());
                m_green_icon = false;
            }

            notifyIcon.Icon = Icon;

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

            new ListBoxVisualState(serieBookmarksListBox).ReloadItems(bookmarks);
        }

        private void UpdateChapterBookmarks()
        {
            ChapterBookmarkListItem[] ar = new ChapterBookmarkListItem[0];

            if (SelectedSerieForBookmark != null)
            {
                ar = (from chapter in SelectedSerieForBookmark.Chapters
                      select new ChapterBookmarkListItem(chapter)).ToArray();
            }

            ListBoxVisualState vs = new ListBoxVisualState(chapterBookmarksListBox);
            vs.Clear();
            if (SelectedSerieForBookmark != null)
            {
                if (m_chapter_bookmarks_visual_states.ContainsKey(SelectedSerieForBookmark))
                    vs = m_chapter_bookmarks_visual_states[SelectedSerieForBookmark];
            }
            vs.ReloadItems(ar);
        }

        private void PulseMangaRootDirTextBox()
        {
            SystemSounds.Asterisk.Play();
            tabControl.SelectedTab = optionsTabPage;
            Color c1 = mangaRootDirTextBox.BackColor;
            Color c2 = (c1 == BAD_DIR) ? SystemColors.Window : BAD_DIR;
            Pulse(c1, c2, 4, 500, (c) => mangaRootDirTextBox.BackColor = c);
        }
	}
}
