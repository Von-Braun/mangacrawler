using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using TomanuExtensions;

namespace MangaCrawlerLib.Crawlers
{
    #if TEST_SERVERS
    internal class TestServerCrawler : Crawler
    {
        private const int MIN_SERVER_DELAY = 250;

        private string m_name;
        private int m_max_server_delay;
        private bool m_slow_series;
        private bool m_slow_chapters;
        private int m_series_per_page;
        private List<Serie> m_series = new List<Serie>();
        private int m_seed;
        private Random m_random = new Random();
        private int m_max_con;

        private class Serie
        {
            public string Title;
            public int Seed;
        }

        private class Chapter
        {
            public string Title;
            public int Seed;
        }

        public TestServerCrawler(string a_name, int a_max_server_delay, 
            bool a_slow_series, bool a_slow_chapters, bool a_empty, int a_max_con)
        {
            m_name = a_name;
            m_seed = a_name.GetHashCode();
            Random random = new Random(m_seed);
            m_slow_series = a_slow_series;
            m_slow_chapters = a_slow_chapters;
            Debug.Assert(a_max_server_delay > MIN_SERVER_DELAY);
            m_max_server_delay = a_max_server_delay;
            m_series_per_page = random.Next(4, 9) * 5;
            m_max_con = a_max_con;

            int maxs = (int)Math.Pow(random.Next(10, 70), 2);
            for (int s = 1; s <= maxs; s++)
            {
                Serie serie = new Serie();
                serie.Title = a_name + " - Serie " + s.ToString();
                serie.Seed = random.Next();
                m_series.Add(serie);
            }

            {
                Serie serie = new Serie();
                serie.Title = a_name + " - empty chapters ";
                m_series.Add(serie);
            }

            {
                Serie serie = new Serie();
                serie.Title = a_name + " - empty pages ";
                m_series.Add(serie);
            }

            if (a_empty)
                m_series.Clear();
        }

        public override int MaxConnectionsPerServer
        {
            get
            {
                if (m_max_con != 0)
                    return m_max_con;
                else
                    return base.MaxConnectionsPerServer;
            }
        }

        public override string Name
        {
            get 
            {
                return m_name;
            }
        }

        public override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            Debug.Assert(a_info.Name == m_name);

            var toreport = (from serie in m_series
                            select new SerieInfo(a_info, "fake_serie_url", serie.Title)).ToArray();
            List<SerieInfo> result = new List<SerieInfo>();

            int total = toreport.Length;

            if (m_slow_series)
            {
                while (toreport.Any())
                {
                    result.AddRange(toreport.Take(m_series_per_page));
                    toreport = toreport.Skip(m_series_per_page).ToArray();

                    Thread.Sleep(NextInt(MIN_SERVER_DELAY, m_max_server_delay));

                    a_progress_callback(
                        result.Count * 100 / total,
                        result);
                }
            }
            else
            {
                Thread.Sleep(NextInt(MIN_SERVER_DELAY, m_max_server_delay));

                a_progress_callback(100, toreport);
            }
        }

        private int NextInt(int a_inclusive_min, int a_exlusive_max)
        {
            lock (m_random)
            {
                return m_random.Next(a_inclusive_min, a_exlusive_max);
            }
        }

        IEnumerable<Chapter> GenerateChapters(Serie a_serie)
        {
            if (a_serie.Seed == 0)
                yield break;

            Random random = new Random(a_serie.Seed);

            int maxc = (int)Math.Pow(random.Next(4, 15), 2);
            for (int c = 1; c <= maxc; c++)
            {
                Chapter chapter = new Chapter();
                chapter.Title = a_serie.Title + " - Chapter " + c.ToString();
                chapter.Seed = random.Next();

                yield return chapter;
            }

            {
                Chapter chapter = new Chapter();
                chapter.Title = a_serie.Title + " - empty pages";
                yield return chapter;
            }
        }

        IEnumerable<string> GeneratePages(Chapter a_chapter)
        {
            if (a_chapter.Seed == 0)
                yield break;

            Random random = new Random(a_chapter.Seed);

            int maxp = (int)Math.Pow(random.Next(4, 12), 2);
            for (int p = 1; p <= maxp; p++)
                yield return a_chapter.Title + " - Page " + p.ToString();

        }

        public override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            Debug.Assert(a_info.Server.Name == m_name);

            var serie = m_series.First(s => s.Title == a_info.Title);


            var toreport = (from chapter in GenerateChapters(serie)
                            select new ChapterInfo(a_info, "fakse_chapter_url", chapter.Title)).ToArray();
            List<ChapterInfo> result = new List<ChapterInfo>();

            int total = toreport.Length;

            if (m_slow_series)
            {
                while (toreport.Any())
                {
                    result.AddRange(toreport.Take(m_series_per_page));
                    toreport = toreport.Skip(m_series_per_page).ToArray();

                    Thread.Sleep(NextInt(MIN_SERVER_DELAY, m_max_server_delay));

                    a_progress_callback(
                        result.Count * 100 / total,
                        result);
                }
            }
            else
            {
                Thread.Sleep(NextInt(MIN_SERVER_DELAY, m_max_server_delay));

                a_progress_callback(100, toreport);
            }
        }

        public override IEnumerable<PageInfo> DownloadPages(TaskInfo a_info)
        {
            Debug.Assert(a_info.Server.Name == m_name);

            var serie = m_series.First(s => s.Title == a_info.Serie);
            var chapter = GenerateChapters(serie).First(c => c.Title == a_info.Chapter);
            var pages = GeneratePages(chapter).ToList();

            var result = from page in pages
                         select new PageInfo(a_info, "fakse_page_url",
                             pages.IndexOf(page) + 1, page);

            Thread.Sleep(NextInt(MIN_SERVER_DELAY, m_max_server_delay));

            return result;
        }

        public override MemoryStream GetImageStream(PageInfo a_info)
        {
            Bitmap bmp = new Bitmap(NextInt(600, 2000), NextInt(600, 2000));
            using (Graphics g = Graphics.FromImage(bmp))
            {
                string str = "server: " + a_info.TaskInfo.ServerName + Environment.NewLine +
                             "serie: " + a_info.TaskInfo.Serie + Environment.NewLine +
                             "chapter: " + a_info.TaskInfo.Chapter + Environment.NewLine +
                             "page: " + a_info.Name;

                g.DrawString(
                    str, 
                    new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold),
                    Brushes.White,
                    new RectangleF(10, 10, bmp.Width - 20, bmp.Height - 20)
                );
            }

            Thread.Sleep(NextInt(MIN_SERVER_DELAY, m_max_server_delay));

            MemoryStream ms = new MemoryStream();
            bmp.SaveJPEG(ms, 75);
            return ms;
        }

        public override string GetImageURL(PageInfo a_info)
        {
            return "fake_image_url.jpg";
        }

        public override string GetServerURL()
        {
            return "fake_server_url";
        }
    }
    #endif
}
