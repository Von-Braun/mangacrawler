using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YAXLib;
using System.Collections.Concurrent;

namespace MangaCrawlerLib
{
    internal class DownloadedChapters
    {
        private class DownloadedServer : IComparable<DownloadedServer>
        {
            public string URL;

            [YAXCollection(ElementName = "Serie", Name = "Series",
            SerializationType = YAXCollectionSerializationTypes.InnerCollectionRecursiveInElement)]
            public List<DownloadedSerie> Series = new List<DownloadedSerie>();

            [YAXOnDeserialized]
            private void OnDeserialized()
            {
                foreach (var serie in Series)
                    serie.Server = this;
            }

            public int CompareTo(DownloadedServer a_other)
            {
                return URL.CompareTo(a_other.URL);
            }

            public void SortSeries()
            {
                Series.Sort();
                foreach (var serie in Series)
                    serie.SortChaptes();
            }
        }

        private class DownloadedSerie : IComparable<DownloadedSerie>
        {
            public DownloadedServer Server;

            [YAXNode]
            public string Title;

            [YAXCollection(ElementName = "Chapter", Name = "Chapters",
            SerializationType = YAXCollectionSerializationTypes.InnerCollectionRecursiveInElement)]
            public List<DownloadedChapter> Chapters = new List<DownloadedChapter>();

            [YAXOnDeserialized]
            private void OnDeserialized()
            {
                foreach (var chapter in Chapters)
                    chapter.Serie = this;
            }

            public int CompareTo(DownloadedSerie a_other)
            {
                return Title.CompareTo(a_other.Title);
            }

            public void SortChaptes()
            {
                Chapters.Sort();
            }
        }

        private class DownloadedChapter : IComparable<DownloadedChapter>
        {
            public DownloadedSerie Serie;
            public string Title;

            public int CompareTo(DownloadedChapter a_other)
            {
                return Title.CompareTo(a_other.Title);
            }

            public string Key
            {
                get
                {
                    return Serie.Server.URL + "-" + Serie.Title + "-" + Title;
                }
            }

            public static string GetKey(ChapterInfo a_info)
            {
                return a_info.SerieInfo.ServerInfo.URL + "-" + a_info.SerieInfo.Title + "-" + a_info.Title;
            }
        }

        [YAXCollection(ElementName = "Server", Name = "Servers", 
            SerializationType = YAXCollectionSerializationTypes.InnerCollectionRecursiveInElement)]
        private List<DownloadedServer> m_tree = new List<DownloadedServer>();

        private ConcurrentDictionary<string, DownloadedChapter> m_map;
        protected string m_file_path;
        private object m_lock;

        private static DownloadedChapters Load(string a_file_path)
        {
            DownloadedChapters result;

            try
            {
                result = YAXSerializer.LoadFromFile<DownloadedChapters>(a_file_path);
            }
            catch
            {
                result = new DownloadedChapters();
            }

            result.m_file_path = a_file_path;

            return result;
        }

        protected DownloadedChapters()
        {
            m_lock = new Object();
        }

        [YAXOnDeserialized]
        private void OnDeserialized()
        {
            m_map = new ConcurrentDictionary<string, DownloadedChapter>();

            var chapters = from server in m_tree
                           from serie in server.Series
                           from chapter in serie.Chapters
                           select chapter;

            foreach (var chapter in chapters)
                m_map[chapter.Key] = chapter;
        }

        public void Save()
        {
            SortTree();
            YAXSerializer.SaveToFile<DownloadedChapters>(m_file_path, this);
        }

        private void SortTree()
        {
            m_tree.Sort();

            foreach (var server in m_tree)
                server.SortSeries();
        }

        public bool WasDownloaded(ChapterInfo a_info)
        {
            return m_map.ContainsKey(DownloadedChapter.GetKey(a_info));
        }

        public void AddDownloaded(ChapterInfo a_info)
        {
            lock (m_lock)
            {
                if (WasDownloaded(a_info))
                    return;

                var server = m_tree.FirstOrDefault(s => s.URL == a_info.SerieInfo.ServerInfo.URL);
                if (server == null)
                {
                    server = new DownloadedServer() { URL = a_info.SerieInfo.ServerInfo.URL };
                    m_tree.Add(server);
                }

                var serie = server.Series.FirstOrDefault(s => s.Title == a_info.SerieInfo.Title);
                if (serie == null)
                {
                    serie = new DownloadedSerie() { Title = a_info.SerieInfo.Title };
                    server.Series.Add(serie);
                }

                var chapter = new DownloadedChapter() { Serie = serie, Title = a_info.Title };
                serie.Chapters.Add(chapter);

                m_map[chapter.Key] = chapter;

                Save();
            }
        }
    }
}
