using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib.Crawlers;
using System.Xml.Linq;
using TomanuExtensions;
using System.IO;
using TomanuExtensions.Utils;
using System.Xml;
using System.Threading;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    public static class Catalog
    {
        private static string CATALOG_XML = "catalog.xml";
        private static string WORKS_XML = "works.xml";
        private static string BOOKMARKS_XML = "bookmarks.xml";

        #region XML Nodes

        private static string CATALOG_NODE = "Catalog";

        private static string GLOBAL_ID_COUNTER_NODE = "IDCounter";

        private static string CATALOG_SERVERS_NODE = "Servers";
        private static string SERVER_NODE = "Server";
        private static string SERVER_ID_NODE = "ID";
        private static string SERVER_NAME_NODE = "Name";
        private static string SERVER_STATE_NODE = "State";
        private static string SERVER_URL_NODE = "URL";

        private static string SERVER_SERIES_NODE = "ServerSeries";
        private static string SERIES_NODE = "Series";
        private static string SERIE_ID_NODE = "ID";
        private static string SERIE_NODE = "Serie";
        private static string SERIE_TITLE_NODE = "Title";
        private static string SERIE_STATE_NODE = "State";
        private static string SERIE_URL_NODE = "URL";

        private static string SERIE_CHAPTERS_NODE = "SerieChapters";
        private static string SERIE_SERVER_ID_NODE = "ServerID";
        private static string CHAPTERS_NODE = "Chapters";
        private static string CHAPTER_NODE = "Chapter";
        private static string CHAPTER_ID_NODE = "ID";
        private static string CHAPTER_STATE_NODE = "State";
        private static string CHAPTER_TITLE_NODE = "Title";
        private static string CHAPTER_LIMITER_ORDER_NODE = "LimiterOrder";
        private static string CHAPTER_URL_NODE = "URL";
        private static string CHAPTER_BOOKMARK_IGNORED_NODE = "BookmarkIgnored";

        private static string CHAPTER_PAGES_NODE = "ChapterPages";
        private static string CHAPTER_SERIE_ID_NODE = "SerieID";
        private static string PAGES_NODE = "Pages";
        private static string PAGE_NODE = "Page";
        private static string PAGE_ID_NODE = "ID";
        private static string PAGE_INDEX_NODE = "Index";
        private static string PAGE_NAME_NODE = "Name";
        private static string PAGE_URL_NODE = "URL";
        private static string PAGE_HASH_NODE = "Hash";
        private static string PAGE_STATE_NODE = "State";
        private static string PAGE_IMAGEFILEPATH_NODE = "ImageFilePath";

        private static string WORKS_NODE = "Works";
        private static string WORK_CHAPTER_ID_NODE = "ChapterID";

        private static string BOOKMARKS_NODE = "Bookmarks";
        private static string BOOKMARK_SERIE_ID_NODE = "SerieID";

        #endregion

        public const double COMPACT_RATIO = 0.75;
        private static Object m_lock = new Object();

        #if TEST_SERVERS
        private static string CATALOG_DIR = "Catalog_Test\\";
        #else
        private static string CATALOG_DIR = "Catalog\\";
        #endif

        private static ulong IDCounter = 0;

        internal static ulong NextID()
        {
            lock (m_lock)
            {
                IDCounter++;
                return IDCounter;
            }
        }

        private static string CatalogFile
        {
            get
            {
                return CatalogDir + CATALOG_XML;
            }
        }

        private static string WorksFile
        {
            get
            {
                return CatalogDir + WORKS_XML;
            }
        }

        private static string BookmarksFile
        {
            get
            {
                return CatalogDir + BOOKMARKS_XML;
            }
        }

        private static string CatalogDir
        {
            get
            {
                return DownloadManager.Instance.SettingsDir + CATALOG_DIR;
            }
        }

        private static IEnumerable<Server> GetServers()
        {
            return from c in CrawlerList.Crawlers
                   select new Server(c.GetServerURL(), c.Name);
        }

        internal static Server[] LoadCatalog()
        {
            var doc = LoadXml(CatalogFile);

            if (doc == null)
            {
                ClearCatalog();
                IDCounter = 0;
                return GetServers().ToArray();
            }

            Debug.Assert(IDCounter == 0);
            List<Server> servers = GetServers().ToList();

            try
            {
                var root = doc.Element(CATALOG_NODE);

                IDCounter = UInt64.Parse(root.Element(GLOBAL_ID_COUNTER_NODE).Value);

                var catalog_servers = (from server in root.Element(CATALOG_SERVERS_NODE).Elements(SERVER_NODE)
                                       select new Server(
                                           server.Element(SERVER_URL_NODE).Value, 
                                           server.Element(SERVER_NAME_NODE).Value,
                                           UInt64.Parse(server.Element(SERVER_ID_NODE).Value), 
                                           EnumExtensions.Parse<ServerState>(
                                            server.Element(SERVER_STATE_NODE).Value)
                                       )).ToList();

                if (!catalog_servers.Select(s => s.ID).Unique())
                    throw new XmlException();

                

                catalog_servers = (from server in catalog_servers
                                   where servers.Select(s => s.ID).Contains(server.ID)
                                   select server).ToList();

                for (int i=0; i<servers.Count; i++)
                {
                    Server cs = catalog_servers.FirstOrDefault(s => s.ID == servers[i].ID);

                    if (cs == null)
                        continue;

                    servers[i] = new Server(servers[i].URL, servers[i].Name, servers[i].ID, cs.State);
                }

                return servers.ToArray();
            }
            catch (Exception ex1)
            {
                Loggers.MangaCrawler.Error("Exception #1", ex1);
                ClearCatalog();
                IDCounter = 0;
                return GetServers().ToArray();
            }
        }

        private static void ClearCatalog()
        {
            try
            {
                new DirectoryInfo(CatalogDir).DeleteContent();
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
            }
        }

        internal static void SaveCatalog()
        {
            lock (m_lock)
            {
                try
                {
                    new DirectoryInfo(CatalogDir).Create();

                    var xml =
                        new XElement(CATALOG_NODE,
                            new XElement(GLOBAL_ID_COUNTER_NODE, IDCounter),
                            new XElement(CATALOG_SERVERS_NODE, from s in DownloadManager.Instance.Servers
                                                       select new XElement(SERVER_NODE,
                                                           new XElement(SERVER_ID_NODE, s.ID),
                                                           new XElement(SERVER_NAME_NODE, s.Name),
                                                           new XElement(SERVER_STATE_NODE, s.State),
                                                           new XElement(SERVER_URL_NODE, s.URL))));
                    xml.Save(CatalogFile);
                }
                catch (Exception ex)
                {
                    Loggers.MangaCrawler.Error("Exception", ex);
                }
            }
        }

        private static string GetCatalogFile(ulong a_id)
        {
            return CatalogDir + a_id.ToString() + ".xml";
        }

        private static void DeleteCatalogFile(ulong a_id)
        {
            DeleteFile(GetCatalogFile(a_id));
        }

        private static void DeleteFile(string a_path)
        {
            try
            {
                new FileInfo(a_path).Delete();
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
            }
        }

        private static XDocument LoadCatalogXml(ulong a_id)
        {
            if (!new FileInfo(GetCatalogFile(a_id)).Exists)
                return null;

            try
            {
                return XDocument.Load(GetCatalogFile(a_id));
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Fatal("Exception", ex);

                DeleteCatalogFile(a_id);
                return null;
            }
        }

        private static XDocument LoadXml(string a_path)
        {
            if (!new FileInfo(a_path).Exists)
                return null;

            try
            {
                return XDocument.Load(a_path);
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Fatal("Exception", ex);

                DeleteFile(a_path);
                return null;
            }
        }

        internal static List<Serie> LoadServerSeries(Server a_server)
        {
            XDocument xml = LoadCatalogXml(a_server.ID);

            if (xml == null)
                return new List<Serie>();

            try
            {
                XElement root = xml.Element(SERVER_SERIES_NODE);

                var series = from serie in root.Element(SERIES_NODE).Elements(SERIE_NODE)
                             select new
                             {
                                 ID =  UInt64.Parse(serie.Element(SERIE_ID_NODE).Value),
                                 Title = serie.Element(SERIE_TITLE_NODE).Value,
                                 URL = serie.Element(SERIE_URL_NODE).Value,
                                 State = EnumExtensions.Parse<SerieState>(
                                    serie.Element(SERIE_STATE_NODE).Value)
                             };

                return (from serie in series
                        select new Serie(a_server, serie.URL, serie.Title, serie.ID, serie.State)).ToList();
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);

                DeleteCatalogFile(a_server.ID);
                return new List<Serie>();
            }
        }

        private static void SaveServerSeries(Server a_server)
        {
            try
            {
                lock (m_lock)
                {
                    new DirectoryInfo(CatalogDir).Create();

                    var xml = new XElement(SERVER_SERIES_NODE,
                        new XElement(SERIES_NODE,
                            from s in a_server.Series
                            select new XElement(SERIE_NODE,
                                new XElement(SERIE_ID_NODE, s.ID),
                                new XElement(SERIE_TITLE_NODE, s.Title),
                                new XElement(SERIE_STATE_NODE, s.State),
                                new XElement(SERIE_URL_NODE, s.URL))));

                    xml.Save(GetCatalogFile(a_server.ID));
                }
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
            }
        }

        internal static List<Chapter> LoadSerieChapters(Serie a_serie)
        {
            XDocument xml = LoadCatalogXml(a_serie.ID);

            if (xml == null)
                return new List<Chapter>();

            try
            {
                XElement root = xml.Element(SERIE_CHAPTERS_NODE);

                var chapters = from chapter in root.Element(CHAPTERS_NODE).Elements(CHAPTER_NODE)
                               select new
                               {
                                   ID = UInt64.Parse(chapter.Element(CHAPTER_ID_NODE).Value),
                                   Title = chapter.Element(CHAPTER_TITLE_NODE).Value,
                                   LimiterOrder = UInt64.Parse(chapter.Element(CHAPTER_LIMITER_ORDER_NODE).Value), 
                                   URL = chapter.Element(CHAPTER_URL_NODE).Value,
                                   BookmarkIgnore = Boolean.Parse(chapter.Element(CHAPTER_BOOKMARK_IGNORED_NODE).Value),
                                   State = EnumExtensions.Parse<ChapterState>(
                                       chapter.Element(CHAPTER_STATE_NODE).Value)
                               };

                return (from chapter in chapters
                        select new Chapter(a_serie, chapter.URL, chapter.Title,
                            chapter.ID, chapter.State, chapter.LimiterOrder, chapter.BookmarkIgnore)).ToList();
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);

                DeleteCatalogFile(a_serie.ID);
                return new List<Chapter>();
            }
        }

        private static void SaveSerieChapters(Serie a_serie)
        {
            try
            {
                lock (m_lock)
                {
                    new DirectoryInfo(CatalogDir).Create();

                    var xml = new XElement(SERIE_CHAPTERS_NODE,
                        new XElement(SERIE_SERVER_ID_NODE, a_serie.Server.ID), 
                        new XElement(CHAPTERS_NODE, 
                            from c in a_serie.Chapters
                            select new XElement(CHAPTER_NODE,
                                new XElement(CHAPTER_ID_NODE, c.ID),
                                new XElement(CHAPTER_TITLE_NODE, c.Title),
                                new XElement(CHAPTER_LIMITER_ORDER_NODE, c.LimiterOrder),
                                new XElement(CHAPTER_STATE_NODE, c.State),
                                new XElement(CHAPTER_BOOKMARK_IGNORED_NODE, c.BookmarkIgnored),
                                new XElement(CHAPTER_URL_NODE, c.URL))));

                    xml.Save(GetCatalogFile(a_serie.ID));
                }
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
            }
        }

        internal static List<Page> LoadChapterPages(Chapter a_chapter)
        {
            XDocument xml = LoadCatalogXml(a_chapter.ID);

            if (xml == null)
                return new List<Page>();

            try
            {
                XElement root = xml.Element(CHAPTER_PAGES_NODE);

                var pages = from page in root.Element(PAGES_NODE).Elements(PAGE_NODE)
                            select new
                            {
                                ID = UInt64.Parse(page.Element(PAGE_ID_NODE).Value),
                                Name = page.Element(PAGE_NAME_NODE).Value,
                                Index = page.Element(PAGE_INDEX_NODE).Value.ToInt(),
                                URL = page.Element(PAGE_URL_NODE).Value,
                                Hash = ConvertHexStringToBytes(page.Element(PAGE_HASH_NODE).Value),
                                ImageFilePath = page.Element(PAGE_IMAGEFILEPATH_NODE).Value,
                                State = EnumExtensions.Parse<PageState>(
                                    page.Element(PAGE_STATE_NODE).Value)
                            };

                return (from page in pages
                        select new Page(a_chapter, page.URL, page.Index, page.ID, page.Name, 
                            page.Hash, page.ImageFilePath, page.State)).ToList();
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);

                DeleteCatalogFile(a_chapter.ID);
                return new List<Page>();
            }
        }

        private static byte[] ConvertHexStringToBytes(string a_hash)
        {
            if (a_hash.Length == 0)
                return null;

            return Converters.ConvertHexStringToBytes(a_hash);
        }

        public static void SaveChapterPages(Chapter a_chapter)
        {
            try
            {
                lock (m_lock)
                {
                    new DirectoryInfo(CatalogDir).Create();

                    var xml = new XElement(CHAPTER_PAGES_NODE,
                        new XElement(CHAPTER_SERIE_ID_NODE, a_chapter.Serie.ID), 
                        new XElement(PAGES_NODE,
                            from p in a_chapter.Pages
                            select new XElement(PAGE_NODE,
                                new XElement(PAGE_ID_NODE, p.ID),
                                new XElement(PAGE_NAME_NODE, p.Name),
                                new XElement(PAGE_INDEX_NODE, p.Index),
                                new XElement(PAGE_HASH_NODE, 
                                    (p.Hash != null) ? Converters.ConvertBytesToHexString(p.Hash, true) : ""),
                                new XElement(PAGE_IMAGEFILEPATH_NODE, p.ImageFilePath),
                                new XElement(PAGE_STATE_NODE, p.State),
                                new XElement(PAGE_URL_NODE, p.URL))));

                    xml.Save(GetCatalogFile(a_chapter.ID));
                }
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
            }
        }

        internal static void Save(Serie a_serie)
        {
            SaveCatalog();
            SaveSerieChapters(a_serie);
            SaveServerSeries(a_serie.Server);

        }

        internal static void Save(Server a_server)
        {
            SaveCatalog();
            SaveServerSeries(a_server);

        }

        internal static void Save(Chapter a_chapter)
        {
            SaveCatalog();
            SaveSerieChapters(a_chapter.Serie);
            SaveChapterPages(a_chapter);
        }

        internal static void Save(Page a_page)
        {
            SaveCatalog();
            SaveChapterPages(a_page.Chapter);

        }

        private static IEnumerable<FileInfo> GetCatalogFiles()
        {
            if (!new FileInfo(CatalogDir).Exists)
                return new FileInfo[] { };

            return new DirectoryInfo(CatalogDir).EnumerateFiles("*.xml");
        }

        public static long GetCatalogSize()
        {
            return GetCatalogFiles().Sum(f => f.Length);
        }

        private static bool IsCompacted(long a_max_catalog_size)
        {
            long desire_catalog_size = (long)(a_max_catalog_size * COMPACT_RATIO);
            long catalog_size = GetCatalogSize();

            return (catalog_size < desire_catalog_size) ;
        }

        private static void DeleteOrphans(List<ulong> a_chapters = null)
        {
            Server[] servers = LoadCatalog();
            List<ulong> ids = new List<ulong>();

            foreach (var file in GetCatalogFiles())
            {
                ulong id;
                if (UInt64.TryParse(Path.GetFileNameWithoutExtension(file.Name), out id))
                    ids.Add(id);
            }

            foreach (Server server in servers)
            {
                ids.Remove(server.ID);

                foreach (var serie in LoadServerSeries(server))
                {
                    ids.Remove(serie.ID);

                    foreach (var chapter in LoadSerieChapters(serie))
                    {
                        ids.Remove(chapter.ID);

                        if (a_chapters != null)
                            a_chapters.Add(chapter.ID);
                    }
                }
            }

            foreach (var id in ids)
                DeleteCatalogFile(id);
        }

        public static void Compact(long a_max_catalog_size, Func<bool> a_cancel)
        {
            if (HasWorks())
                return;
            
            try
            {
                List<ulong> ids = new List<ulong>();

                foreach (var file in GetCatalogFiles())
                {
                    ulong id;
                    if (!UInt64.TryParse(Path.GetFileNameWithoutExtension(file.Name), out id))
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            Loggers.MangaCrawler.Error("Exception", ex);
                        }
                    }
                }

                List<ulong> chapters = new List<ulong>();
                DeleteOrphans(chapters);
                DeleteChaptersWithoutImages(chapters);
                DeleteSeriesWithoutChapters();
                CompactBruteForce(a_max_catalog_size);
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
            }
        }

        private static void CompactBruteForce(long a_max_catalog_size)
        {
            List<ulong> ids = new List<ulong>();

            foreach (var file in GetCatalogFiles())
            {
                ulong id;
                if (UInt64.TryParse(Path.GetFileNameWithoutExtension(file.Name), out id))
                    ids.Add(id);
            }

            for (;;)
            {
                if (IsCompacted(a_max_catalog_size))
                    break;

                var oldest = from id in ids
                             orderby new FileInfo(GetCatalogFile(id)).LastWriteTimeUtc
                             select id;
                var to_remove = ids.Take(100).ToList();
                ids.RemoveRange(to_remove);

                foreach (var id in to_remove)
                    DeleteCatalogFile(id);
                DeleteOrphans();
            }
        }

        private static void DeleteSeriesWithoutChapters()
        {
            Server[] servers = LoadCatalog();
            List<ulong> ids = new List<ulong>();

            foreach (var file in GetCatalogFiles())
            {
                ulong id;
                if (UInt64.TryParse(Path.GetFileNameWithoutExtension(file.Name), out id))
                    ids.Add(id);
            }

            foreach (Server server in servers)
            {
                foreach (var serie in LoadServerSeries(server))
                {
                    var chapters = LoadSerieChapters(serie);

                    bool empty_serie = chapters.All(ch => !ids.Contains(ch.ID));

                    if (empty_serie)
                        DeleteCatalogFile(serie.ID);
                }
            }
        }

        private static void DeleteChaptersWithoutImages(List<ulong> a_chapters)
        {
            var oldest = from id in a_chapters
                         orderby new FileInfo(GetCatalogFile(id)).LastWriteTimeUtc
                         select id;

            foreach (var old in oldest)
            {
                var image_files = LoadCatalogXml(old).Element(CHAPTER_PAGES_NODE).Element(PAGES_NODE).
                    Elements(PAGE_NODE).Select(el => el.Element(PAGE_IMAGEFILEPATH_NODE).Value);

                Func<bool> no_images = () =>
                {
                    foreach (var image in image_files)
                    {
                        try
                        {
                            if (new FileInfo(image).Exists)
                                return false;
                        }
                        catch (Exception ex)
                        {
                            Loggers.MangaCrawler.Error("Exception", ex);
                            return true;
                        }
                    }

                    return true;
                };

                if (no_images())
                    DeleteCatalogFile(old);
            }
        }

        internal static List<Chapter> LoadWorks()
        {
            if (!new FileInfo(WorksFile).Exists)
                return new List<Chapter>();

            try
            {
                XElement root = XDocument.Load(WorksFile).Element(WORKS_NODE);

                List<Chapter> works = new List<Chapter>();

                foreach (var work in root.Elements(WORK_CHAPTER_ID_NODE))
                {
                    ulong chapter_id = UInt64.Parse(work.Value);
                    Chapter chapter = LoadChapter(chapter_id);

                    if (chapter == null)
                        continue;

                    works.Add(chapter);
                }

                return works;
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
                DeleteFile(WorksFile);
                return new List<Chapter>();
            }
        }

        private static bool HasWorks()
        {
            XDocument doc = LoadXml(WorksFile);

            if (doc == null)
                return false;

            try
            {
                return doc.Element(WORKS_NODE).Elements(WORK_CHAPTER_ID_NODE).Any();
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
                return false;
            }
        }

        internal static void SaveWorks(IEnumerable<Chapter> a_works)
        {
            try
            {
                lock (m_lock)
                {
                    new DirectoryInfo(CatalogDir).Create();

                    var xml = new XElement(WORKS_NODE,
                        from work in a_works
                        select new XElement(WORK_CHAPTER_ID_NODE, work.ID));
                       
                    xml.Save(WorksFile);
                }
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
            }
        }

        private static Serie LoadSerie(ulong a_serie_id)
        {
            XDocument doc = LoadCatalogXml(a_serie_id);

            if (doc == null)
                return null;

            ulong server_id;

            try
            {
                server_id = UInt64.Parse(doc.Element(SERIE_CHAPTERS_NODE).Element(SERIE_SERVER_ID_NODE).Value);

            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
                DeleteCatalogFile(a_serie_id);
                return null;
            }

            Server server = LoadServer(server_id);

            if (server == null)
                return null;

            return server.Series.FirstOrDefault(s => s.ID == a_serie_id);
        }

        private static Chapter LoadChapter(ulong a_chapter_id)
        {
            XDocument doc = LoadCatalogXml(a_chapter_id);

            if (doc == null)
                return null;

            ulong serie_id;

            try
            {
                serie_id = UInt64.Parse(doc.Element(CHAPTER_PAGES_NODE).Element(CHAPTER_SERIE_ID_NODE).Value);

            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
                DeleteCatalogFile(a_chapter_id);
                return null;
            }

            Serie serie = LoadSerie(serie_id);

            if (serie == null)
                return null;

            return serie.Chapters.FirstOrDefault(c => c.ID == a_chapter_id);
        }

        private static Server LoadServer(ulong a_server_id)
        {
            return DownloadManager.Instance.Servers.FirstOrDefault(s => s.ID == a_server_id);
        }

        internal static List<Serie> LoadBookmarks()
        {
            if (!new FileInfo(BookmarksFile).Exists)
                return new List<Serie>();

            try
            {
                XElement root = XDocument.Load(BookmarksFile).Element(BOOKMARKS_NODE);

                List<Serie> bookmarks = new List<Serie>();

                foreach (var bookmark in root.Elements(BOOKMARK_SERIE_ID_NODE))
                {
                    ulong serie_id = UInt64.Parse(bookmark.Value);
                    Serie serie = LoadSerie(serie_id);

                    if (serie == null)
                        continue;

                    bookmarks.Add(serie);
                }

                return bookmarks;
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
                DeleteFile(BookmarksFile);
                return new List<Serie>();
            }
        }

        internal static void SaveBookmarks()
        {
            try
            {
                lock (m_lock)
                {
                    new DirectoryInfo(CatalogDir).Create();

                    var xml = new XElement(BOOKMARKS_NODE,
                        from serie in DownloadManager.Instance.Bookmarks.List
                        select new XElement(BOOKMARK_SERIE_ID_NODE, serie.ID));

                    xml.Save(BookmarksFile);
                }
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Error("Exception", ex);
            }
        }
    }
}
