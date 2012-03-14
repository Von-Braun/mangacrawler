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
        public Action UpdateGUI { get; private set; }

        private List<Entity> m_downloading = new List<Entity>();
        private Server[] m_servers;
        private List<Chapter> m_works = new List<Chapter>();

        public static DownloadManager Instance { get; private set; }

        public static void Create(MangaSettings a_manga_settings, string a_settings_dir, Action a_update_gui)
        {
            Instance = new DownloadManager(a_manga_settings, a_settings_dir, a_update_gui);
            Instance.Initialize();
        }

        private void Initialize()
        {
            m_servers = Catalog.LoadCatalog();

            IEnumerable<Chapter> works = from work in Catalog.LoadWorks(Servers)
                                         orderby work.LimiterOrder
                                         select work;
            DownloadPages(works);
        }

        private  DownloadManager(MangaSettings a_manga_settings, string a_settings_dir, Action a_update_gui)
        {
            SettingsDir = a_settings_dir;
            MangaSettings = a_manga_settings;
            UpdateGUI = a_update_gui;

            HtmlWeb.UserAgent_Actual = a_manga_settings.UserAgent;
        }

        public bool IsDownloading()
        {
            bool result = m_downloading.Any();

            m_downloading = (from entity in m_downloading
                             where entity.IsWorking
                             select entity).ToList();
            return result;
        }

        public void DownloadSeries(Server a_server)
        {
            if (a_server == null)
                return;

            if (!a_server.DownloadRequired)
                return;

            m_downloading.Add(a_server);
            a_server.State = ServerState.Waiting;
            a_server.LimiterOrder = Catalog.NextID();
            UpdateGUI();

            new Task(() =>
            {
                a_server.DownloadSeries();

            }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
        }

        public void DownloadChapters(Serie a_serie)
        {
            if (a_serie == null)
                return;

            if (!a_serie.DownloadRequired)
                return;

            m_downloading.Add(a_serie);
            a_serie.State = SerieState.Waiting;
            a_serie.LimiterOrder = Catalog.NextID();
            UpdateGUI();

            new Task(() =>
            {
                a_serie.DownloadChapters();
            }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
        }

        public void DownloadPages(IEnumerable<Chapter> a_chapters)
        {
            foreach (var chapter in a_chapters)
            {
                if (chapter.IsWorking)
                    continue;

                lock (m_works)
                {
                    if (m_works.Contains(chapter))
                        m_works.Remove(chapter);
                    m_works.Add(chapter);
                }

                m_downloading.Add(chapter);
                chapter.State = ChapterState.Waiting;
                chapter.LimiterOrder = Catalog.NextID();
                UpdateGUI();

                Chapter chapter_sync = chapter;

                new Task(() =>
                {
                    chapter_sync.DownloadPages();
                }, TaskCreationOptions.LongRunning).Start(Limiter.Scheduler);
            }
        }

        public IEnumerable<Chapter> Works
        {
            get
            {
                lock (m_works)
                {
                    return m_works.ToArray();
                }
            }
        }

        public void Save()
        {
            Catalog.SaveCatalog();
        }

        public IEnumerable<Server> Servers
        {
            get
            {
                return m_servers;
            }
        }

        public void SaveWorks()
        {
            IEnumerable<Chapter> copy; 

            lock (m_works)
            {
                copy = m_works.ToArray();
            }

            copy = copy.Where(c => c.IsWorking);

            Catalog.SaveWorks(copy);
        }

        public void RemoveWork(Chapter a_work)
        {
            lock (m_works)
            {
                if (!m_works.Remove(a_work))
                    Loggers.MangaCrawler.WarnFormat("Chapter not in s_works: {0}", a_work);
            }
        }
    }
}
