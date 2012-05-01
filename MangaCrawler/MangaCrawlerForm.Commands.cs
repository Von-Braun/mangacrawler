using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;
using System.Media;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MangaCrawler.Properties;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace MangaCrawler
{
    partial class MangaCrawlerForm
    {
        private class MangaCrawlerFormCommands
        {
            public MangaCrawlerFormGUI GUI;
            private static int MAX_TO_OPEN = 10;

            private void UpdateNowServer(Server a_server)
            {
                DownloadManager.Instance.DownloadSeries(GUI.SelectedServer, true);
                GUI.UpdateAll();
            }

            public void UpdateNowForSelectedServer()
            {
                UpdateNowServer(GUI.SelectedServer);
            }

            private void OpenFolder(Entity a_entity)
            {
                OpenFolders(a_entity);
            }

            private void OpenFolders(params Entity[] a_entities)
            {
                bool error = a_entities.Count() > MAX_TO_OPEN;

                foreach (var entity in a_entities.Take(MAX_TO_OPEN))
                {
                    try
                    {
                        if (!Directory.Exists(entity.GetDirectory()))
                        {
                            error = true;
                            continue;
                        }

                        Process.Start(entity.GetDirectory());
                    }
                    catch (Exception ex)
                    {
                        Loggers.GUI.Error("Exception", ex);
                        error = true;
                    }
                }

                if (error)
                {
                    SystemSounds.Asterisk.Play();
                    GUI.UpdateButtons();
                }
            }

            public void OpenFolderForSelectedServer()
            {
                OpenFolder(GUI.SelectedServer);
            }

            public void VisitPageForSelectedServer()
            {
                VisitPage(GUI.SelectedServer);
            }

            private void VisitPage(Entity a_entity)
            {
                VisitPages(a_entity);
            }

            private void VisitPages(params Entity[] a_entitites)
            {
                bool error = false;

                foreach (var entity in a_entitites)
                {
                    try
                    {
                        Process.Start(entity.URL);

                        if (entity is Chapter)
                        {
                            DownloadManager.Instance.BookmarksVisited(
                                new[] { entity as Chapter });
                        }
                    }
                    catch (Exception ex)
                    {
                        Loggers.GUI.Error("Exception", ex);
                        error = true;
                    }
                }

                if (error)
                    SystemSounds.Asterisk.Play();

                GUI.UpdateAll();
            }

            public void DownloadSeriesForSelectedServer()
            {
                DownloadManager.Instance.DownloadSeries(GUI.SelectedServer, a_force: false);
                GUI.UpdateAll();
            }

            public void UpdateNowForSelectedSerie()
            {
                UpdateNowSerie(GUI.SelectedSerie);
            }

            private void UpdateNowSerie(Serie a_serie)
            {
                DownloadManager.Instance.DownloadChapters(a_serie, true);
                GUI.UpdateAll();
            }

            public void BookmarkSelectedSerie()
            {
                BookmarkSerie(GUI.SelectedSerie);
            }

            private void BookmarkSerie(Serie a_serie)
            {
                DownloadManager.Instance.Bookmarks.Add(a_serie);
                DownloadChaptersForSerie(a_serie);
                GUI.UpdateAll();
            }

            public void OpenFolderForSelectedSerie()
            {
                OpenFolder(GUI.SelectedSerie);
            }

            public void VisitPageForSelectedSerie()
            {
                VisitPage(GUI.SelectedSerie);
            }

            public void DownloadChapterForSelectedSerie()
            {
                DownloadChaptersForSerie(GUI.SelectedSerie);
            }

            public void DownloadChaptersForSerie(Serie a_serie)
            {
                DownloadManager.Instance.DownloadChapters(a_serie, a_force: false);
                GUI.UpdateAll();
            }

            public void DownloadPagesForSelectedChapters()
            {
                DownloadPages(GUI.SelectedChapters);
            }

            private void DownloadPages(IEnumerable<Chapter> a_chapters)
            {
                if (!Settings.Instance.MangaSettings.IsMangaRootDirValid)
                {
                    GUI.PulseMangaRootDirTextBox();
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

                DownloadManager.Instance.DownloadPages(a_chapters);
                GUI.UpdateAll();
            }

            public void VisitPageForSelectedChapters()
            {
                VisitPages(GUI.SelectedChapters);
            }

            public void OpenFolderForSelectedChapters()
            {
                OpenFolders(GUI.SelectedChapters);
            }

            public void ReadMangaForSelectedChapters()
            {
                ReadManga(GUI.SelectedChapters);
            }

            private void ReadManga(params Chapter[] a_chapters)
            {
                var chapters = a_chapters.Where(e => e != null);

                bool error = chapters.Count() > MAX_TO_OPEN;

                foreach (var chapter in chapters.Take(MAX_TO_OPEN))
                {
                    if (chapter.CanReadFirstPage())
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

                if (error)
                {
                    SystemSounds.Asterisk.Play();
                    GUI.UpdateButtons();
                }
            }

            public void CancelClearSelectedDownloadings()
            {
                CancelDownloadings(GUI.SelectedDownloadings);
                ClearDownloadings(GUI.SelectedDownloadings);
            }

            private void CancelDownloadings(Chapter[] a_downloadings)
            {
                foreach (var chapter in a_downloadings)
                {
                    if (chapter == null)
                        continue;

                    if (chapter.IsDownloading)
                        chapter.CancelDownloading();
                }

                GUI.UpdateAll();
            }

            private void ClearDownloadings(Chapter[] a_downloadings)
            {
                foreach (var chapter in a_downloadings)
                {
                    if (chapter == null)
                        continue;

                    if (!chapter.IsDownloading)
                        DownloadManager.Instance.Downloadings.Remove(chapter);
                }

                GUI.UpdateAll();
            }

            public void DownloadPagesForSelectedDownloadings()
            {
                DownloadPages(GUI.SelectedDownloadings);
            }

            public void CancelSelectedDownloadings()
            {
                CancelDownloadings(GUI.SelectedDownloadings);
            }

            public void ClearAllDownloadings()
            {
                ClearDownloadings(DownloadManager.Instance.Downloadings.List.Where(
                    c => !c.IsDownloading).ToArray());
            }

            public void OpenFolderForSelectedDownloadings()
            {
                OpenFolders(GUI.SelectedDownloadings);
            }

            public void VisitPageForSelectedDownloadings()
            {
                VisitPages(GUI.SelectedDownloadings);
            }

            public void ReadMangaForSelectedDownloadings()
            {
                ReadManga(GUI.SelectedDownloadings);
            }

            public void RemoveBookmarkFromBookmarks()
            {
                RemoveBookmark(GUI.SelectedBookmarkedSerie);
            }

            private void RemoveBookmark(Serie a_serie)
            {
                if (a_serie == null)
                    return;

                DownloadManager.Instance.Bookmarks.Remove(a_serie);
                GUI.UpdateAll();
            }

            public void CheckNowBookmarks()
            {
                foreach (var server in DownloadManager.Instance.Bookmarks.List.Select(s => s.Server).Distinct())
                    DownloadManager.Instance.DownloadSeries(server, true);

                foreach (var serie in DownloadManager.Instance.Bookmarks.List)
                    DownloadManager.Instance.DownloadChapters(serie, true);

                GUI.UpdateAll();
            }

            public void OpenFolderForSelectedBookmarkSerie()
            {
                OpenFolder(GUI.SelectedBookmarkedSerie);
            }

            public void VisitPageForSelectedBookmarkedSerie()
            {
                VisitPage(GUI.SelectedBookmarkedSerie);
            }

            public void VisitPagesForSelectedBookmarkedChapters()
            {
                VisitPages(GUI.SelectedBookmarkedChapters);
            }

            public void DownloadSeriesForSelectedBookmarkSerie()
            {
                DownloadManager.Instance.DownloadChapters(GUI.SelectedBookmarkedSerie, a_force: false);
                GUI.UpdateAll();
            }

            public void DownloadPagesForSelectedBookmarkedChapters()
            {
                DownloadPages(GUI.SelectedBookmarkedChapters);
            }

            public void VisitBookmarkedPagesForSelectedChapters()
            {
                VisitPages(GUI.SelectedBookmarkedChapters);

                DownloadManager.Instance.BookmarksVisited(GUI.SelectedBookmarkedChapters);

                GUI.UpdateAll();
            }

            public void OpenFolderForSelectedBookmarkedChapters()
            {
                OpenFolders(GUI.SelectedBookmarkedChapters);
            }

            public void ReadMangaForSelectedBookmarkedChapters()
            {
                ReadManga(GUI.SelectedBookmarkedChapters);
            }

            public void SaveDownloadings()
            {
                DownloadManager.Instance.Downloadings.Save();
            }

            public void CheckNewVersion()
            {
                try
                {
                    var doc = new HtmlWeb().Load(Resources.HomePage);
                    var match = Regex.Match(doc.DocumentNode.InnerText, "Manga Crawler \\d+\\.\\d+");
                    var avail_str = match.Value.Replace("Manga Crawler ", "").Replace(".", ",");
                    var avail = Double.Parse(avail_str);

                    var actual_str = System.Reflection.Assembly.GetAssembly(
                        typeof(MangaCrawlerForm)).GetName().Version;
                    var actual = Double.Parse(actual_str.Major.ToString() + "," +
                        actual_str.Minor.ToString());

                    actual += 0.00001;

                    if (avail > actual)
                        GUI.InformAboutNewVersion();
                }
                catch (Exception ex)
                {
                    Loggers.GUI.Error("Exception", ex);
                }
            }

            public void IgnoreForSelectedBookmarkedChapters()
            {
                IgnoreNew(GUI.SelectedBookmarkedChapters);
            }

            private void IgnoreNew(IEnumerable<Chapter> a_chapters)
            {
                DownloadManager.Instance.BookmarksVisited(a_chapters);
                GUI.UpdateAll();
            }

            public void UpdateAutostart()
            {
                if (Settings.Instance.MinimizeOnClose)
                {
                    if (Settings.Instance.Autostart)
                        Autostart.Enable();
                    else
                        Autostart.Disable();
                }
                else
                    Autostart.Disable();
            }
        }
    }
}