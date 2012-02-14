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

namespace MangaCrawlerLib.Crawlers
{
    internal class TestServerCrawler : Crawler
    {
        private const int MIN_SERVER_DELAY = 250;

        private string m_name;
        private List<Serie> m_series = new List<Serie>();
        private Random m_random = new Random();
        private Object m_lock = new Object();
        private int m_max_server_delay;
        private bool m_slow_series;
        private bool m_slow_chapters;
        private int m_series_per_page;

        private class Serie
        {
            public string Title;
            public List<Chapter> Chapters = new List<Chapter>();
        }

        private class Chapter
        {
            public string Title;
            public List<string> Pages = new List<string>();
        }

        public TestServerCrawler(string a_name, int a_max_server_delay, bool a_slow_series, bool a_slow_chapters)
        {
            m_name = a_name;
            m_random = new Random(a_name.GetHashCode());
            m_slow_series = a_slow_series;
            m_slow_chapters = a_slow_chapters;
            Debug.Assert(a_max_server_delay > MIN_SERVER_DELAY);
            m_max_server_delay = a_max_server_delay;
            m_series_per_page = NextInt(4, 9) * 5;

            for (int s = 0; s < NextInt(500, 4000); s++)
            {
                Serie serie = new Serie();
                serie.Title = a_name + " - Serie " + s.ToString();

                for (int c = 0; c < NextInt(10, 200); c++)
                {
                    Chapter chapter = new Chapter();
                    chapter.Title = serie.Title + " - Chapter " + c.ToString();

                    for (int p = 0; p < NextInt(20, 150); p++)
                        chapter.Pages.Add(chapter.Title + " - Page " + p.ToString());

                    serie.Chapters.Add(chapter);
                }

                m_series.Add(serie);
            }
        }

        private int NextInt(int a_min_inclusive, int a_max_exclusive)
        {
            lock (m_lock)
            {
                return m_random.Next(a_min_inclusive, a_max_exclusive);
            }
        }


        internal override string Name
        {
            get 
            {
                return m_name;
            }
        }

        internal override void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback)
        {
            Debug.Assert(a_info.Name == m_name);

            var result = (from serie in m_series
                         select new SerieInfo(a_info, "fake_serie_url", serie.Title)).ToArray();

            int reported = 0;
            int total = result.Length;

            if (m_slow_series)
            {
                while (result.Any())
                {
                    reported += m_series_per_page;
                    a_progress_callback(
                        reported * 100 / total, 
                        result.Take(m_series_per_page).ToArray());
                    result = result.Skip(m_series_per_page).ToArray();
                }
            }
            else
            {
                Thread.Sleep(NextInt(MIN_SERVER_DELAY, m_max_server_delay));
            }
          
            a_progress_callback(100, result);
        }

        internal override void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback)
        {
            Debug.Assert(a_info.Server.Name == m_name);

            var serie = m_series.First(s => s.Title == a_info.Title);
            var result = from chapter in serie.Chapters
                         select new ChapterInfo(a_info, "fakse_chapter_url", chapter.Title);

            Thread.Sleep(NextInt(MIN_SERVER_DELAY, m_max_server_delay));

            a_progress_callback(100, result);
        }

        internal override IEnumerable<PageInfo> DownloadPages(TaskInfo a_info)
        {
            Debug.Assert(a_info.Server.Name == m_name);

            var serie = m_series.First(s => s.Title == a_info.Serie);
            var chapter = serie.Chapters.First(c => c.Title == a_info.Chapter);

            var result = from page in chapter.Pages
                         select new PageInfo(a_info, "fakse_page_url", 
                             chapter.Pages.IndexOf(page) , chapter.Title);

            Thread.Sleep(NextInt(MIN_SERVER_DELAY, m_max_server_delay));

            return result;
        }

        internal override MemoryStream GetImageStream(PageInfo a_info)
        {
            Bitmap bmp = new Bitmap(NextInt(600, 2000), NextInt(600, 2000));
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawString(
                    a_info.Name.Replace("/", " / ").Replace(" \\ ", " \\ "), 
                    new Font(FontFamily.GenericMonospace, 15), 
                    Brushes.Red, 
                    new RectangleF(10, 10, bmp.Width, bmp.Height));
            }

            Thread.Sleep(NextInt(MIN_SERVER_DELAY, m_max_server_delay));

            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Bmp);
            return ms;
        }

        internal override string GetImageURL(PageInfo a_info)
        {
            throw new NotImplementedException();
        }

        internal override string GetServerURL()
        {
            return "fake_server_url";
        }
    }
}
