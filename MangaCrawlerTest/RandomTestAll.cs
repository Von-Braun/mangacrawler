using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MangaCrawlerLib;
using HtmlAgilityPack;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using TomanuExtensions.Utils;
using MangaCrawlerLib.Crawlers;
using MangaCrawler;
using TomanuExtensions;
using System.Drawing;
using TomanuExtensions.TestUtils;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace MangaCrawlerTest
{
    [TestClass]
    public class RandomTestAll : TestBase
    {
        public static string EXCEPTIONS_NO_CHAPTERS = "_Exceptions - no chapters.txt";
        public static string EXCEPTIONS_NO_CHAPTERS_CANDIDATES = "_Exceptions - no chapters - candidates.txt";
        public static string EXCEPTIONS_NO_PAGES = "_Exceptions - no pages.txt";
        public static string EXCEPTIONS_NO_PAGES_CANDIDATES = "_Exceptions - no pages - candidates.txt";
        public static string EXCEPTIONS_NO_IMAGES = "_Exceptions - no images.txt";
        public static string EXCEPTIONS_NO_IMAGES_CANDIDATES = "_Exceptions - no images - candidates.txt";

        private ProgressIndicator m_pi;
        private bool m_error = false;

        [TestCleanup]
        public void CheckError()
        {
            Assert.IsTrue(m_error == false);
        }

        protected override void WriteLine(string a_str, params object[] a_args)
        {
            base.WriteLine(a_str, a_args);
            String str = String.Format(a_str, a_args);
            m_pi.AddLine(str);
        }

        private static IEnumerable<T> TakeRandom<T>(IEnumerable<T> a_enum, double a_percent)
        {
            List<T> list = a_enum.ToList();
            Random random = new Random();

            for (int i = 0; i < list.Count * a_percent; i++)
            {
                int r = random.Next(list.Count);
                T el = list[r];
                list.RemoveAt(r);
                yield return el;
            }
        }

        [TestMethod, Timeout(24 * 60 * 60 * 1000)]
        public void RandomTestAll_()
        {
            Dictionary<Server, int> serie_chapters = new Dictionary<Server, int>();
            Dictionary<Server, int> chapter_pageslist = new Dictionary<Server, int>();
            Dictionary<Server, int> chapter_images = new Dictionary<Server, int>();
            DateTime last_report = DateTime.Now;
            TimeSpan report_delta = new TimeSpan(0, 15, 0);
            int errors = 0;
            int warnings = 0;
            m_pi = new ProgressIndicator("RandomTestAll");
            Object locker = new Object();

            Func<string, List<string>> load_exceptions = file_name =>
            {
                if (File.Exists(Path.Combine(GetTestDataDir(), file_name)))
                {
                    var result = File.ReadAllLines(Path.Combine(GetTestDataDir(), file_name)).ToList();
                    WriteLine("{0} entries in {1}", result.Count, file_name);
                    return result;
                }
                else
                {
                    WriteLine("File doesn't exists: {0}", EXCEPTIONS_NO_CHAPTERS);
                    return new List<string>();
                }
            };

            Action<string, string> add_candidate_exception = (file, line) =>
            {
                using (FileStream fs = File.OpenWrite(Path.Combine(GetTestDataDir(), file)))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(line);
                };
            };

            var exceptions_no_series = load_exceptions(EXCEPTIONS_NO_CHAPTERS);
            var exceptions_no_chapters = load_exceptions(EXCEPTIONS_NO_PAGES);
            var exceptions_no_pages = load_exceptions(EXCEPTIONS_NO_PAGES);
            var exceptions_no_images = load_exceptions(EXCEPTIONS_NO_IMAGES);

            if (File.Exists(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_CHAPTERS_CANDIDATES)))
                File.Delete(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_CHAPTERS_CANDIDATES));
            if (File.Exists(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_PAGES_CANDIDATES)))
                File.Delete(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_PAGES_CANDIDATES));
            if (File.Exists(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_IMAGES)))
                File.Delete(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_IMAGES_CANDIDATES));

            new DirectoryInfo(Catalog.CatalogDir).DeleteContent();

            foreach (var server in DownloadManager.Instance.Servers)
            {
                serie_chapters[server] = 0;
                chapter_pageslist[server] = 0;
                chapter_images[server] = 0;
            }

            Action<bool> report = (force) =>
            {
                lock (locker)
                {
                    if (!force)
                    {
                        if (DateTime.Now - last_report < report_delta)
                            return;
                    }

                    last_report = DateTime.Now;
                }

                WriteLine("");
                WriteLine("Report ({0}):", DateTime.Now);

                foreach (var server in DownloadManager.Instance.Servers)
                {
                    WriteLine("Server: {0}, Serie chapters: {1}, Chapters pages: {2}, Chapter images: {3}",
                        server.Name, serie_chapters[server], chapter_pageslist[server], chapter_images[server]);
                }

                WriteLine("Errors: {0}, Warnings: {1}", errors, warnings);
                WriteLine("");
            };

            Parallel.ForEach(DownloadManager.Instance.Servers,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = DownloadManager.Instance.Servers.Count()
                },
                server =>
                {
                    server.State = ServerState.Waiting;
                    server.DownloadSeries();

                    if (server.State == ServerState.Error)
                    {
                        WriteLineError("ERROR - {0} {1} - Error while downloading series from server",
                            server.Name, server.URL);
                        Assert.Fail();
                    }
                    else if (server.Series.Count == 0)
                    {
                        WriteLineError("ERROR - {0} {1} - Server have no series", 
                            server.Name, server.URL);
                        Assert.Fail();
                    }

                    Parallel.ForEach(
                        TakeRandom(server.Series, 0.1),
                        new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer
                        },
                        serie =>
                        {
                            serie.State = SerieState.Waiting;
                            serie.DownloadChapters();
                            serie_chapters[server]++;

                            if (serie.State == SerieState.Error)
                            {
                                if (!exceptions_no_chapters.Contains(serie.ToString()))
                                {
                                    WriteLineError("ERROR - {0} {1} - Error while downloading chapters from serie",
                                        serie, serie.URL);
                                    errors++;
                                }
                            }
                            else if (serie.Chapters.Count == 0)
                            {
                                if (!exceptions_no_chapters.Contains(serie.ToString()))
                                {
                                    add_candidate_exception(EXCEPTIONS_NO_CHAPTERS_CANDIDATES, serie.ToString());
                                    WriteLineWarning("WARN - {0} {1} - Serie have no chapters",
                                        serie, serie.URL);
                                    warnings++;
                                }
                            }
                            else
                            {
                                if (exceptions_no_chapters.Contains(serie.ToString()))
                                {
                                    WriteLineWarning("WARN - {0} {1} - Serie have chapters, remove from exceptions",
                                        serie, serie.URL);
                                    warnings++;
                                }
                            }

                            Parallel.ForEach(TakeRandom(serie.Chapters, 0.1),
                                new ParallelOptions()
                                {
                                    MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer
                                },
                                (chapter) =>
                                {
                                    try
                                    {
                                        chapter.State = ChapterState.Waiting;

                                        Limiter.BeginChapter(chapter);

                                        try
                                        {
                                            chapter.DownloadPagesList();
                                        }
                                        finally
                                        {
                                            Limiter.EndChapter(chapter);
                                        }

                                        chapter_pageslist[server]++;

                                        if (chapter.Pages.Count == 0)
                                        {
                                            if (!exceptions_no_pages.Contains(chapter.ToString()))
                                            {
                                                add_candidate_exception(EXCEPTIONS_NO_PAGES_CANDIDATES, chapter.ToString());
                                                WriteLineWarning("WARN - {0} {1} - Chapter have no pages",
                                                    chapter, chapter.URL);
                                                warnings++;
                                            }
                                        }
                                        else
                                        {
                                            if (exceptions_no_pages.Contains(chapter.ToString()))
                                            {
                                                WriteLineWarning("WARN - {0} {1} - Chapter have pages, remove from exceptions",
                                                    chapter, chapter.URL);
                                                warnings++;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        WriteLineError("ERROR - {0} {1} - Exception while downloading pages from chapter", 
                                            chapter, chapter.URL);
                                        errors++;
                                    }

                                    Parallel.ForEach(TakeRandom(chapter.Pages, 0.1),
                                        new ParallelOptions()
                                        {
                                            MaxDegreeOfParallelism = chapter.Crawler.MaxConnectionsPerServer
                                        },
                                        (page) =>
                                        {
                                            Limiter.BeginChapter(chapter);

                                            try
                                            {
                                                MemoryStream stream = null;

                                                try
                                                {
                                                    page.GetImageURL();

                                                    try
                                                    {
                                                        stream = page.GetImageStream();

                                                        if (stream.Length == 0)
                                                        {
                                                            WriteLineError("ERROR - {0} {1} - Image stream is zero size for page",
                                                                page, page.URL);
                                                            errors++;
                                                        }
                                                        else
                                                        {
                                                            try
                                                            {
                                                                System.Drawing.Image.FromStream(stream);

                                                                if (exceptions_no_images.Contains(page.Chapter.ToString()))
                                                                {
                                                                    WriteLineWarning("WARN - {0} {1} - Page has image, remove from exceptions",
                                                                        page, page.URL);
                                                                    warnings++;
                                                                }
                                                            }
                                                            catch
                                                            {
                                                                WriteLineError("ERROR - {0} {1} - Exception while creating image from stream for page",
                                                                    page, page.URL);
                                                                errors++;
                                                            }
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        WriteLineError("ERROR - {0} {1} - Exception while downloading image from page",
                                                            page, page.URL);
                                                        errors++;
                                                    }
                                                }
                                                catch
                                                {
                                                    if (!exceptions_no_images.Contains(page.Chapter.ToString()))
                                                    {
                                                        add_candidate_exception(EXCEPTIONS_NO_IMAGES_CANDIDATES, page.Chapter.ToString());
                                                        WriteLineError("ERROR - {0} {1} - Exception while detecting image url",
                                                            page, page.URL);
                                                        errors++;
                                                    }
                                                }
                                            }
                                            finally
                                            {
                                                Limiter.EndChapter(chapter);
                                            }

                                            chapter_images[server]++;
                                            report(false);
                                        });
                                });
                        });
                });
        }

                [TestMethod]
        public void FindEmpties()
        {
            var xmls = Directory.GetFiles(GetTestDataDir(), "*.xml");
            m_pi = new ProgressIndicator("RandomTestAll");

            ConcurrentBag<string> empty_no_chapters = new ConcurrentBag<string>(
                File.ReadAllLines(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_CHAPTERS)));
            ConcurrentBag<string> empty_no_pages = new ConcurrentBag<string>(
                File.ReadAllLines(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_PAGES)));
            ConcurrentBag<string> empty_no_images = new ConcurrentBag<string>(
                File.ReadAllLines(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_IMAGES)));

            Parallel.ForEach(
                DownloadManager.Instance.Servers, 
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = DownloadManager.Instance.Servers.Count()
                }, 
                server =>
            {
                WriteLine(server.ToString());

                server.State = ServerState.Waiting;
                server.DownloadSeries();

                Parallel.ForEach(
                    server.Series,
                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer
                    },
                    serie =>
                    {
                        if (empty_no_chapters.Contains(serie.ToString()))
                            return;

                        serie.State = SerieState.Waiting;
                        serie.DownloadChapters();

                        if (serie.Chapters.Count == 0)
                        {
                            empty_no_chapters.Add(serie.ToString());
                            m_pi.AddLine("NO CHAPTERS:" + serie.ToString());
                        }
                        else
                        {
                            Parallel.ForEach(
                                serie.Chapters,
                                new ParallelOptions()
                                {
                                    MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer
                                },
                                chapter =>
                                {
                                    if (empty_no_pages.Contains(chapter.ToString()))
                                        return;

                                    chapter.State = ChapterState.Waiting;
                                    chapter.DownloadPages();

                                    if (chapter.Pages.Count == 0)
                                    {
                                        empty_no_pages.Add(chapter.ToString());
                                        m_pi.AddLine("NO PAGES:" + chapter.ToString());
                                    }
                                    else
                                    {
                                        if (empty_no_images.Contains(chapter.ToString()))
                                            return;

                                        Page page = null;
                                        if (chapter.Pages.Count == 1)
                                            page = chapter.Pages[0];
                                        else
                                            page = chapter.Pages[1];

                                        Limiter.BeginChapter(chapter);

                                        try
                                        {
                                            try
                                            {
                                                page.GetImageURL();
                                            }
                                            catch
                                            {
                                                empty_no_images.Add(chapter.ToString());
                                                m_pi.AddLine("NO IMAGES:" + chapter.ToString());
                                            }
                                        }
                                        finally
                                        {
                                            Limiter.EndChapter(chapter);
                                        }
                                    }
                                });
                        }
                    });
            });

            File.WriteAllLines(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_CHAPTERS), 
                empty_no_chapters.OrderBy(el => el).Distinct().ToArray());

            File.WriteAllLines(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_CHAPTERS), 
                empty_no_pages.OrderBy(el => el).Distinct().ToArray());

            File.WriteAllLines(Path.Combine(GetTestDataDir(), EXCEPTIONS_NO_CHAPTERS),
                empty_no_images.OrderBy(el => el).Distinct().ToArray());
        }
    }
}
