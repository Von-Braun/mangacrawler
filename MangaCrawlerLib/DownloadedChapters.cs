using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YAXLib;
using System.Collections.Concurrent;
using System.Diagnostics;
using TomanuExtensions.Utils;
using System.IO;

namespace MangaCrawlerLib
{
    internal class DownloadedChapters
    {
        private class DownloadedChapter
        {
            public string Server;
            public string Serie;
            public string Chapter;
            public DateTime Date;
        }

        private static readonly string DOWNLOADED_TXT = "downloaded.txt";
        private static readonly string VERSION = "version 2012-02-08";
        private static readonly string DATETIME_FORMAT = "yyyy.MM.dd HH:mm:ss";

        private HashSet<string> m_set = new HashSet<string>();
        private List<DownloadedChapter> m_downloaded_chapters = new List<DownloadedChapter>();
        private string m_file_path;

        public DownloadedChapters(string a_file_path)
        {
            m_file_path += a_file_path + DOWNLOADED_TXT;

            FileInfo fi = new FileInfo(m_file_path);
            if (!fi.Exists)
                return;

            try
            {
                using (StreamReader sr = fi.OpenText())
                {
                    for (; ; )
                    {
                        string version = sr.ReadLine();
                        string date_time = sr.ReadLine();
                        string server = sr.ReadLine();
                        string serie = sr.ReadLine();
                        string chapter = sr.ReadLine();

                        if (chapter == null)
                            break;
   
                        if (version != VERSION)
                        {
                            Loggers.Settings.Error("Error when loading downloaded chapters: Unknown version");
                            continue;
                        }

                        DateTime dt = DateTime.Parse(date_time);

                        DownloadedChapter dc = new DownloadedChapter()
                        {
                            Server = server,
                            Serie = serie,
                            Chapter = chapter,
                            Date = dt
                        };

                        m_downloaded_chapters.Add(dc);
                        m_set.Add(ChapterKey(dc));

                        sr.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.Settings.Error("Error when loading downloaded chapters: exception");
                Loggers.Settings.Error(ex);
            }
        }

        private static string ChapterKey(string a_server, string a_serie, string a_chapter)
        {
            return String.Format("{0} - {1} - {2}", a_server, a_serie, a_chapter);
        }

        private static string ChapterKey(ChapterInfo a_info)
        {
            return String.Format("{0} - {1} - {2}", a_info.Serie.Server.Name,
                    a_info.Serie.Title, a_info.Title);
        }

        private static string ChapterKey(DownloadedChapter a_dc)
        {
            return String.Format("{0} - {1} - {2}", a_dc.Server, a_dc.Serie, a_dc.Chapter);
        }

        public bool WasDownloaded(ChapterInfo a_info)
        {
            lock (m_set)
            {
                return m_set.Contains(ChapterKey(a_info));
            }
        }

        public void AddDownloaded(ChapterInfo a_info)
        {
            lock (m_set)
            {
                if (WasDownloaded(a_info))
                    return;

                string k = ChapterKey(a_info);
                m_set.Add(k);

                DownloadedChapter dc = new DownloadedChapter()
                {
                    Server = a_info.Serie.Server.Name,
                    Serie = a_info.Serie.Title,
                    Chapter = a_info.Title,
                    Date = DateTime.Now
                };

                m_downloaded_chapters.Add(dc);

                SaveNewDownloadedChapter(dc);
            }
        }

        private void SaveNewDownloadedChapter(DownloadedChapter a_dc)
        {
            try
            {
                using (FileStream fs = new FileStream(m_file_path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {

                    fs.Seek(0, SeekOrigin.End);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine("");
                    sw.WriteLine(VERSION);
                    sw.WriteLine(a_dc.Date.ToString(DATETIME_FORMAT));
                    sw.WriteLine(a_dc.Server);
                    sw.WriteLine(a_dc.Serie);
                    sw.WriteLine(a_dc.Chapter);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                Loggers.Settings.Error("Error when saving downloaded chapters: exception");
                Loggers.Settings.Error(ex);
            }
        }
    }
}
