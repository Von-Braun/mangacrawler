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

namespace MangaCrawler
{
	partial class MangaCrawlerForm
	{
        private class MangaCrawlerCommands
        {
            public MangaCrawlerGUI GUI;

            private void CheckNowServer(Server a_server)
            {
                DownloadManager.Instance.DownloadSeries(GUI.SelectedServer, true);
                GUI.UpdateAll();
            }

            public void UpdateNowForSelectedServer()
            {
                CheckNowServer(GUI.SelectedServer);
            }

            private void OpenFolder(Entity a_entity)
            {
                OpenFolders(a_entity);
            }

            private void OpenFolders(params Entity[] a_entities)
            {
                var entities = a_entities.Where(e => e != null);

                bool error = false;

                foreach (var entity in entities)
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
                var entitites = a_entitites.Where(e => e != null);

                bool error = false;

                foreach (var entity in entitites)
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

            public void DownloadSeriesForSelectedServer()
            {
                DownloadManager.Instance.DownloadSeries(GUI.SelectedServer, a_force: false);
                GUI.UpdateAll();
            }

            public void UpdateNowForSelectedSerie()
            {
                UpdateNowSerie(GUI.SelectedSerie);
            }

            private void UpdateNowSerie(Entity entity)
            {
                throw new NotImplementedException();
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

            private void BookmarkSerie(Entity entity)
            {
                throw new NotImplementedException();
            }

            private void BookmarkSerie(Serie a_serie)
            {
                if ((a_serie == null) || a_serie.IsDownloading ||
                    DownloadManager.Instance.Bookmarks.List.Contains(a_serie))
                {
                    return;
                }

                DownloadManager.Instance.Bookmarks.Add(a_serie);
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
                DownloadManager.Instance.DownloadChapters(GUI.SelectedSerie, a_force: false);
                GUI.UpdateAll();
            }

            public void DownloadPagesForSelectedChapters()
            {
                DownloadPages(GUI.SelectedChapters);
            }

            private void DownloadPages(IEnumerable<Chapter> a_chapters)
            {
                if (a_chapters.Count() == 0)
                    return;

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
                GUI.PlaySoundWhenDownloaded = true;
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

            public void CancelClearSelectedWorks()
            {
                CancelWorks(GUI.SelectedWorks);
                ClearWorks(GUI.SelectedWorks);
            }

            private void CancelWorks(Chapter[] a_works)
            {
                foreach (var work in a_works)
                {
                    if (work == null)
                        continue;

                    if (work.IsDownloading)
                        work.CancelWork();
                }

                GUI.UpdateAll();
            }

            private void ClearWorks(Chapter[] a_works)
            {
                foreach (var work in a_works)
                {
                    if (work == null)
                        continue;

                    if (!work.IsDownloading)
                        DownloadManager.Instance.Works.Remove(work);
                }

                GUI.UpdateAll();
            }

            public void DownloadPagesForSelectedWorks()
            {
                DownloadPages(GUI.SelectedWorks);
            }

            public void CancelSelectedWorks()
            {
                CancelWorks(GUI.SelectedWorks);
            }

            public void ClearAllWorks()
            {
                ClearWorks(DownloadManager.Instance.Works.List.Where(
                    c => !c.IsDownloading).ToArray());
            }

            public void OpenFolderForSelectedWorks()
            {
                OpenFolders(GUI.SelectedWorks);
            }

            public void VisitPageForSelectedWorks()
            {
                VisitPages(GUI.SelectedWorks);
            }

            public void ReadMangaForSelectedWorks()
            {
                ReadManga(GUI.SelectedWorks);
            }

            public void RemoveBookmarkFromBookmarks()
            {
                RemoveBookmark(GUI.SelectedSerieForBookmarks);
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
                OpenFolder(GUI.SelectedSerieForBookmarks);
            }

            public void VisitPageForSelectedBookmarkedSerie()
            {
                VisitPage(GUI.SelectedSerieForBookmarks);
            }

            public void VisitPagesForSelectedBookmarkedChapters()
            {
                VisitPages(GUI.SelectedChaptersForBookmarks);
            }

            public void DownloadSeriesForSelectedBookmarkSerie()
            {
                DownloadManager.Instance.DownloadChapters(GUI.SelectedSerieForBookmarks, a_force: false);
                GUI.UpdateAll();
            }

            public void DownloadPagesForSelectedBookmarkedChapters()
            {
                DownloadPages(GUI.SelectedChaptersForBookmarks);
            }

            public void VisitBookmarkedPagesForSelectedChapters()
            {
                VisitPages(GUI.SelectedChaptersForBookmarks);

                DownloadManager.Instance.BookmarksIgnored(GUI.SelectedChaptersForBookmarks, true);

                GUI.UpdateAll();
            }

            public void OpenFolderForSelectedBookmarkedChapters()
            {
                OpenFolders(GUI.SelectedChaptersForBookmarks);
            }

            public void ReadMangaForSelectedBookmarkedChapters()
            {
                ReadManga(GUI.SelectedChaptersForBookmarks);
            }

            public void ClearCache()
            {
                DownloadManager.Instance.ClearCache();
                GC.Collect();
            }

            public void SaveWorks()
            {
                DownloadManager.Instance.Works.Save();
            }

            public void CheckNewVersion()
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
                        GUI.InformAboutNewVersion();
                }
                catch (Exception ex)
                {
                    Loggers.GUI.Error("Exception", ex);
                }
            }
        }
	}
}