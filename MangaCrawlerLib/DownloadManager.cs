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
        public Works Works { get; private set; }
        
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
            Works.Load();
        }

        private  DownloadManager(MangaSettings a_manga_settings, string a_settings_dir)
        {
            SettingsDir = a_settings_dir;
            MangaSettings = a_manga_settings;
            Bookmarks = new Bookmarks();
            Works = new Works();

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

            }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
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
            }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
        }

        public void DownloadPages(IEnumerable<Chapter> a_chapters)
        {
            if (!a_chapters.Any())
                return;

            DownloadPages(a_chapters.First());

            new Task(() =>
            {
                foreach (var chapter in a_chapters.Skip(1))
                    DownloadPages(chapter);
            }).Start();
        }

        private void DownloadPages(Chapter a_chapter)
        {
            if (a_chapter.IsDownloading)
                return;

            Works.Add(a_chapter);

            lock (m_downloading)
            {
                m_downloading.Add(a_chapter);
            }
            a_chapter.State = ChapterState.Waiting;
            a_chapter.LimiterOrder = Catalog.NextID();

            Catalog.SaveChapterPages(a_chapter);

            Chapter chapter_sync = a_chapter;

            new Task(() =>
            {
                chapter_sync.DownloadPages();
            }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
        }

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

        public void Debug_LoadAllFromCatalog(ref int a_servers, ref int a_series, ref int a_chapters, ref int a_pages)
        {
            foreach (var server in Servers)
                server.Debug_LoadAllFromCatalog(ref a_servers, ref a_series, ref a_chapters, ref a_pages);
        }

        public void BookmarksIgnored(IEnumerable<Chapter> a_chapters, bool a_state)
        {
            var chapters_grouped_by_serie = from ch in a_chapters
                     group ch by ch.Serie;

            foreach (var chapters_group in chapters_grouped_by_serie)
            {
                foreach (var chapter in chapters_group)
                    chapter.BookmarkIgnored = a_state;

                Catalog.Save(chapters_group.First().Serie);
            }
        }
    }
}
