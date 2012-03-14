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
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace MangaCrawlerLib.Crawlers
{
    internal class TestServerCrawler : Crawler
    {
        private const int MIN_SERVER_DELAY = 50;

        private string m_name;
        private int m_max_server_delay;
        private bool m_slow_series;
        private bool m_slow_chapters;
        private int m_items_per_page;
        private List<SerieData> m_series = new List<SerieData>();
        private int m_seed;
        private Random m_random = new Random();
        private int m_max_con;

        private class SerieData
        {
            public string Title;
            public int Seed;
        }

        private class ChapterData
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

            if (a_max_server_delay != 0)
                Debug.Assert(a_max_server_delay > MIN_SERVER_DELAY);
            m_max_server_delay = a_max_server_delay;

            m_items_per_page = random.Next(4, 9) * 5;
            m_max_con = a_max_con;

            int maxs = (int)Math.Pow(random.Next(10, 70), 2);
            for (int s = 1; s <= maxs; s++)
            {
                SerieData serie = new SerieData();
                serie.Title = a_name + " - Serie " + s.ToString();
                serie.Seed = random.Next();
                m_series.Add(serie);
            }

            {
                SerieData serie = new SerieData();
                serie.Title = a_name + " - empty chapters ";
                m_series.Add(serie);
            }

            {
                SerieData serie = new SerieData();
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

        private void Sleep()
        {
            if (m_max_server_delay == 0)
                return;
            Thread.Sleep(NextInt(MIN_SERVER_DELAY, m_max_server_delay));
        }

        internal override void DownloadSeries(Server a_server, Action<int, IEnumerable<Serie>> a_progress_callback)
        {
            a_server.State = ServerState.Downloading;

            Limiter.Aquire(a_server);
            try
            {
                Sleep();
            }
            finally
            {
                Limiter.Release(a_server);
            }

            Debug.Assert(a_server.Name == m_name);

            var toreport = (from serie in m_series
                            select new Serie(a_server, "fake_serie_url", serie.Title)).ToArray();

            int total = toreport.Length;

            if (m_slow_series)
            {
                List<List<Serie>> listlist = new List<List<Serie>>();
                while (toreport.Any())
                {
                    var part = toreport.Take(m_items_per_page).ToList();
                    toreport = toreport.Skip(m_items_per_page).ToArray();
                    listlist.Add(part);
                }

                ConcurrentBag<Tuple<int, int, Serie>> series = 
                    new ConcurrentBag<Tuple<int, int, Serie>>();

                Parallel.ForEach(listlist, 
                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = MaxConnectionsPerServer,
                        TaskScheduler = Limiter.Scheduler
                    }, 
                    (list) =>
                {
                    foreach (var el in list)
                        series.Add(new Tuple<int, int, Serie>(listlist.IndexOf(list), list.IndexOf(el), el));

                    Limiter.Aquire(a_server);
                    try
                    {
                      Sleep();
                    }
                    finally
                    {
                        Limiter.Release(a_server);
                    }

                    var result = (from s in series
                                  orderby s.Item1, s.Item2
                                  select s.Item3).ToList();

                    a_progress_callback(
                        result.Count * 100 / total,
                        result);
                });
            }
            else
                a_progress_callback(100, toreport);
        }

        private int NextInt(int a_inclusive_min, int a_exlusive_max)
        {
            lock (m_random)
            {
                return m_random.Next(a_inclusive_min, a_exlusive_max);
            }
        }

        IEnumerable<ChapterData> GenerateChapters(SerieData a_serie)
        {
            if (a_serie.Seed == 0)
                yield break;

            Random random = new Random(a_serie.Seed);

            int maxc = (int)Math.Pow(random.Next(4, 15), 2);
            for (int c = 1; c <= maxc; c++)
            {
                ChapterData chapter = new ChapterData();
                chapter.Title = a_serie.Title + " - Chapter " + c.ToString();
                chapter.Seed = random.Next();

                yield return chapter;
            }

            {
                ChapterData chapter = new ChapterData();
                chapter.Title = a_serie.Title + " - empty pages";
                yield return chapter;
            }
        }

        IEnumerable<string> GeneratePages(ChapterData a_chapter)
        {
            if (a_chapter.Seed == 0)
                yield break;

            Random random = new Random(a_chapter.Seed);

            int maxp = (int)Math.Pow(random.Next(4, 12), 2);
            for (int p = 1; p <= maxp; p++)
                yield return a_chapter.Title + " - Page " + p.ToString();

        }

        internal override void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback)
        {
            a_serie.State = SerieState.Downloading;

            Limiter.Aquire(a_serie);
            try
            {
                Sleep();
            }
            finally
            {
                Limiter.Release(a_serie);
            }

            Debug.Assert(a_serie.Server.Name == m_name);

            var serie = m_series.First(s => s.Title == a_serie.Title);

            var toreport = (from chapter in GenerateChapters(serie)
                            select new Chapter(a_serie, "fakse_chapter_url", chapter.Title)).ToArray();

            int total = toreport.Length;

            if (m_slow_chapters)
            {
                List<List<Chapter>> listlist = new List<List<Chapter>>();
                while (toreport.Any())
                {
                    var part = toreport.Take(m_items_per_page).ToList();
                    toreport = toreport.Skip(m_items_per_page).ToArray();
                    listlist.Add(part);
                }

                ConcurrentBag<Tuple<int, int, Chapter>> chapters =
                    new ConcurrentBag<Tuple<int, int, Chapter>>();

                Parallel.ForEach(listlist,
                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = MaxConnectionsPerServer,
                        TaskScheduler = Limiter.Scheduler
                    },
                    (list) =>
                    {
                        foreach (var el in list)
                            chapters.Add(new Tuple<int, int, Chapter>(listlist.IndexOf(list), list.IndexOf(el), el));

                        Limiter.Aquire(a_serie);
                        try
                        {
                            Sleep();
                        }
                        finally
                        {
                            Limiter.Release(a_serie);
                        }

                        var result = (from s in chapters
                                      orderby s.Item1, s.Item2
                                      select s.Item3).ToList();

                        a_progress_callback(
                            result.Count * 100 / total,
                            result);
                    });
            }
            else
                a_progress_callback(100, toreport);
        }

        internal override IEnumerable<Page> DownloadPages(Chapter a_chapter)
        {
            a_chapter.State = ChapterState.DownloadingPagesList;

            var serie = m_series.First(s => s.Title == a_chapter.Serie.Title);
            var chapter = GenerateChapters(serie).First(c => c.Title == a_chapter.Title);
            var pages = GeneratePages(chapter).ToList();

            var result = from page in pages
                         select new Page(a_chapter, "fakse_page_url",
                             pages.IndexOf(page) + 1, page);

            Limiter.Aquire(a_chapter);
            try
            {
                Sleep();
            }
            finally
            {
                Limiter.Release(a_chapter);
            }

            return result;
        }

        internal override MemoryStream GetImageStream(Page a_page)
        {
            Bitmap bmp = new Bitmap(NextInt(600, 2000), NextInt(600, 2000));
            using (Graphics g = Graphics.FromImage(bmp))
            {
                string str = "server: " + a_page.Server.Name + Environment.NewLine +
                             "serie: " + a_page.Serie.Title + Environment.NewLine +
                             "chapter: " + a_page.Chapter.Title + Environment.NewLine +
                             "page: " + a_page.Name;

                g.DrawString(
                    str, 
                    new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold),
                    Brushes.White,
                    new RectangleF(10, 10, bmp.Width - 20, bmp.Height - 20)
                );
            }

            Limiter.Aquire(a_page);
            try
            {
                Sleep();
            }
            finally
            {
                Limiter.Release(a_page);
            }

            MemoryStream ms = new MemoryStream();
            bmp.SaveJPEG(ms, 75);
            return ms;
        }

        internal override string GetImageURL(Page a_page)
        {
            a_page.State = PageState.Downloading;
            return "fake_image_url.jpg";
        }

        public override string GetServerURL()
        {
            return m_name;
        }
    }
}
