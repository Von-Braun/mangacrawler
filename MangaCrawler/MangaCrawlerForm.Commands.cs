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

namespace MangaCrawler
{
	partial class MangaCrawlerForm
	{
        private void CheckNowServer(Server a_server)
        {
            if (a_server == null)
                return;

                DownloadManager.Instance.DownloadSeries(SelectedServer, true);
                UpdateAll();
        }

        private void CheckNowSelectedServer()
        {
            CheckNowServer(SelectedServer);
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

        private void OpenFolderForSelectedServer()
        {
            OpenFolder(SelectedServer);
        }

        private void VisitPageForSelectedServer()
        {
            VisitPage(SelectedServer);
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

        private void DownloadSeriesForSelectedServer()
        {
            DownloadManager.Instance.DownloadSeries(SelectedServer, a_force: false);
            UpdateAll();
        }

        private void CheckNowForSelectedSerie()
        {
            CheckNowSerie(SelectedSerie);
        }

        private void CheckNowSerie(Serie a_serie)
        {
            if (a_serie == null)
                return;

            DownloadManager.Instance.DownloadChapters(a_serie, true);
            UpdateAll();
        }

        private void BookmarkSelectedSerie()
        {
            BookmarkSerie(SelectedSerie);
        }

        private void BookmarkSerie(Serie a_serie)
        {
            if ((a_serie == null) || a_serie.IsDownloading ||
                DownloadManager.Instance.Bookmarks.List.Contains(a_serie))
            {
                return;
            }

            DownloadManager.Instance.Bookmarks.Add(a_serie);
            UpdateAll();
        }

        private void OpenFolderForSelectedSerie()
        {
            OpenFolder(SelectedSerie);
        }

        private void VisitPageForSelectedSerie()
        {
            VisitPage(SelectedSerie);
        }

        private void DownloadChapterForSelectedSerie()
        {
            DownloadManager.Instance.DownloadChapters(SelectedSerie, a_force: false);
            UpdateAll();
        }

        private void DownloadPagesForSelectedChapters()
        {
            DownloadPages(SelectedChapters);
        }

        private void DownloadPages(IEnumerable<Chapter> a_chapters)
        {
            if (a_chapters.Count() == 0)
                return;

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

            DownloadManager.Instance.DownloadPages(a_chapters);
            m_play_sound_when_downloaded = true;
            UpdateAll();
        }

        private void VisitPageForSelectedChapters()
        {
            VisitPages(SelectedChapters);
        }

        private void OpenFolderForSelectedChapters()
        {
            OpenFolders(SelectedChapters);
        }

        private void ViewPagesForSelectedChapters()
        {
            ViewPages(SelectedChapters);
        }

        private void ViewPages(params Chapter[] a_chapters)
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

        private void CancelClearSelectedWorks()
        {
            CancelWorks(SelectedWorks);
            ClearWorks(SelectedWorks);
        }

        private void CancelWorks(Chapter[] a_works)
        {
            foreach (var work in a_works)
            {
                if (work.IsDownloading)
                {
                    work.CancelWork();
                    m_play_sound_when_downloaded = false;
                }
            }

            UpdateAll();
        }

        private void ClearWorks(Chapter[] a_works)
        {
            foreach (var work in a_works)
            {
                if (!work.IsDownloading)
                    DownloadManager.Instance.Works.Remove(work);
            }

            UpdateAll();
        }

        private void DownloadPagesForSelectedWorks()
        {
            DownloadPages(SelectedWorks);
        }

        private void CancelSelectedWorks()
        {
            CancelWorks(SelectedWorks);
        }

        private void ClearAllWorks()
        {
            ClearWorks(DownloadManager.Instance.Works.List.Where(
                c => !c.IsDownloading).ToArray());
        }

        private void OpenFolderForSelectedWorks()
        {
            OpenFolders(SelectedWorks);
        }

        private void VisitPageForSelectedWorks()
        {
            VisitPages(SelectedWorks);
        }

        private void ViewPagesForSelectedWorks()
        {
            ViewPages(SelectedWorks);
        }

        private void RemoveSelectedBookmark()
        {
            RemoveBookmark(SelectedSerieForBookmark);
        }

        private void RemoveBookmark(Serie a_serie)
        {
            if (a_serie == null)
                return;

            DownloadManager.Instance.Bookmarks.Remove(a_serie);
            UpdateAll();
        }
	}
}
