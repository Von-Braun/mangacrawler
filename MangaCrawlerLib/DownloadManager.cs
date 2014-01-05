using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Ionic.Zip;
using System.Resources;
using System.Diagnostics;
using System.Collections.Concurrent;
using HtmlAgilityPack;
using TomanuExtensions;
using System.Collections.ObjectModel;
using MangaCrawlerLib.Crawlers;

namespace MangaCrawlerLib
{
    public class DownloadManager
    {
        public string SettingsDir { get; private set; }
        public MangaSettings MangaSettings { get; private set; }
        public Bookmarks Bookmarks { get; private set; }
        public Downloading Downloadings { get; private set; }
        
        private List<Entity> m_downloading = new List<Entity>();
        private Server[] m_servers;

        public static DownloadManager Instance { get; private set; }

        public static void Create(MangaSettings a_manga_settings, string a_settings_dir)
        {
            Instance = new DownloadManager(a_manga_settings, a_settings_dir);
            Instance.Initialize();
        }

        private void Initialize()
        {
            m_servers = Catalog.LoadCatalog();

            Bookmarks.Load();
            Downloadings.Load();
        }

        private DownloadManager(MangaSettings a_manga_settings, string a_settings_dir)
        {
            SettingsDir = a_settings_dir;
            MangaSettings = a_manga_settings;
            Bookmarks = new Bookmarks();
            Downloadings = new Downloading();

            HtmlWeb.UserAgent_Actual = a_manga_settings.UserAgent;
        }

        public bool NeedGUIRefresh(bool a_reset_state)
        {
            lock (m_downloading)
            {
                bool result = m_downloading.Any();

                if (a_reset_state)
                {
                    m_downloading = (from entity in m_downloading
                                     where entity.IsDownloading
                                     select entity).ToList();
                }

                return result;
            }
        }

        public void DownloadSeries(Server a_server, bool a_force)
        {
            if (a_server == null)
                return;

            if (!a_server.IsDownloadRequired(a_force))
                return;

            lock (m_downloading)
            {
                m_downloading.Add(a_server);
            }

            a_server.State = ServerState.Waiting;
            a_server.LimiterOrder = Catalog.NextID();

            new Task(() =>
            {
                a_server.DownloadSeries();

            }, TaskCreationOptions.LongRunning).Start();
        }

        public void DownloadChapters(Serie a_serie, bool a_force)
        {
            if (a_serie == null)
                return;

            if (!a_serie.IsDownloadRequired(a_force))
                return;

            lock (m_downloading)
            {
                m_downloading.Add(a_serie);
            }
            a_serie.State = SerieState.Waiting;
            a_serie.LimiterOrder = Catalog.NextID();

            new Task(() =>
            {
                a_serie.DownloadChapters();
            }, TaskCreationOptions.LongRunning).Start();
        }

        public void DownloadPages(IEnumerable<Chapter> a_chapters)
        {
            a_chapters = a_chapters.Where(ch => !ch.IsDownloading).ToList();

            if (!a_chapters.Any())
                return;

            foreach (var chapter in a_chapters)
            {
                chapter.State = ChapterState.Waiting;
                lock (m_downloading)
                {
                    m_downloading.Add(chapter);
                }
                Downloadings.Add(chapter);
                chapter.LimiterOrder = Catalog.NextID();
                Catalog.SaveChapterPages(chapter);
            }

            new Task(() =>
            {
                foreach (var chapter in a_chapters)
                {
                    Chapter chapter_sync = chapter;

                    new Task(() =>
                    {
                        chapter_sync.DownloadPagesAndImages();
                    }, TaskCreationOptions.LongRunning).Start();
                }
            }).Start();
        }

        /// <summary>
        /// Thread safe - immutable.
        /// </summary>
        public IEnumerable<Server> Servers
        {
            get
            {
                return m_servers;
            }
        }

        public void Debug_ResetCheckDate()
        {
            foreach (var server in Servers)
                server.ResetCheckDate();
        }

        public void Debug_InsertSerie(int a_index, Server a_server)
        {
            (a_server.Crawler as TestServerCrawler).Debug_InsertSerie(a_index);
        }

        public void Debug_RemoveSerie(Server a_server, Serie SelectedSerie)
        {
            (a_server.Crawler as TestServerCrawler).Debug_RemoveSerie(SelectedSerie);
        }

        public void Debug_InsertChapter(int a_index, Serie a_serie)
        {
            (a_serie.Crawler as TestServerCrawler).Debug_InsertChapter(a_serie, a_index);
        }

        public void Debug_RemoveChapter(Chapter a_chapter)
        {
            (a_chapter.Crawler as TestServerCrawler).Debug_RemoveChapter(a_chapter);
        }

        public void Debug_RenameSerie(Serie a_serie)
        {
            (a_serie.Crawler as TestServerCrawler).Debug_RenameSerie(a_serie);
        }

        public void Debug_RenameChapter(Chapter a_chapter)
        {
            (a_chapter.Crawler as TestServerCrawler).Debug_RenameChapter(a_chapter);
        }

        public void Debug_ChangeSerieURL(Serie a_serie)
        {
            (a_serie.Crawler as TestServerCrawler).Debug_ChangeSerieURL(a_serie);
        }

        public void Debug_ChangeChapterURL(Chapter a_chapter)
        {
            (a_chapter.Crawler as TestServerCrawler).Debug_ChangeChapterURL(a_chapter);
        }

        public void BookmarksVisited(IEnumerable<Chapter> a_chapters)
        {
            var chapters_grouped_by_serie = from ch in a_chapters
                     group ch by ch.Serie;

            foreach (var chapters_group in chapters_grouped_by_serie)
            {
                foreach (var chapter in chapters_group)
                    chapter.Visited = true;

                Catalog.Save(chapters_group.First().Serie);
            }
        }

        public void Debug_DuplicateSerieName(Serie a_serie)
        {
            (a_serie.Crawler as TestServerCrawler).Debug_DuplicateSerieName(a_serie);
        }

        public void Debug_DuplicateChapterName(Chapter a_chapter)
        {
            (a_chapter.Crawler as TestServerCrawler).Debug_DuplicateChapterName(a_chapter);
        }

        public void Debug_DuplicateSerieURL(Serie a_serie)
        {
            (a_serie.Crawler as TestServerCrawler).Debug_DuplicateSerieURL(a_serie);
        }

        public void Debug_DuplicateChapterURL(Chapter a_chapter)
        {
            (a_chapter.Crawler as TestServerCrawler).Debug_DuplicateChapterURL(a_chapter);
        }

        public void Debug_MakeSerieError(Serie a_serie)
        {
            (a_serie.Crawler as TestServerCrawler).Debug_MakeSerieError(a_serie);
        }

        public void Debug_MakeChapterError(Chapter a_chapter)
        {
            (a_chapter.Crawler as TestServerCrawler).Debug_MakeChapterError(a_chapter);
        }
    }
}
