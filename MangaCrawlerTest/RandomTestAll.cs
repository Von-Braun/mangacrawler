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
        private const string FILE_EXCEPTION = "_exceptions.txt";
        private const string FILE_EXCEPTION_CANDIDATES = "_exceptions-candidates.txt";

        private ProgressIndicator m_pi;
        private bool m_error = false;
        HashSet<string> m_exceptions = new HashSet<string>();

        [TestCleanup]
        public void CheckError()
        {
            Assert.IsTrue(m_error == false);
        }

        protected override void WriteLine(string a_str, params object[] a_args)
        {
            String str = String.Format(a_str, a_args);
            base.WriteLine(a_str, a_args);
            m_pi.AddLine(str);
        }

        protected override void WriteLineError(string a_str, params object[] a_args)
        {
            lock (m_exceptions)
            {
                String str = String.Format(a_str, a_args);

                if (!m_exceptions.Contains(str))
                {
                    m_exceptions.Add(str);
                    File.AppendAllText(TestBase.GetTestFilePath(FILE_EXCEPTION_CANDIDATES),
                        str + Environment.NewLine);
                }
            }

            base.WriteLineError(a_str, a_args);
        }

        private void WriteLineErrorForPage(string a_str, Page a_page)
        {
            lock (m_exceptions)
            {
                String str = String.Format(a_str, a_page.Chapter, a_page.Chapter.URL);

                if (!m_exceptions.Contains(str))
                {
                    WriteLineError(a_str, a_page, a_page.URL);
                    WriteLineError(a_str, a_page.Chapter, a_page.Chapter.URL);
                }
            }
        }

        private static IEnumerable<T> TakeRandom<T>(IEnumerable<T> a_enum, double a_percent)
        {
            List<T> list = a_enum.ToList();
            Random random = new Random();

            if (!list.Any())
                yield break;

            var el = list.RemoveFirst();
            yield return el;

            if (!list.Any())
                yield break;

            el = list.RemoveLast();
            yield return el;

            for (int i = 0; i < list.Count * a_percent - 2; i++)
            {
                int r = random.Next(list.Count);
                el = list[r];
                list.RemoveAt(r);
                yield return el;
            }
        }

        [TestMethod, Timeout(24 * 60 * 60 * 1000)]
        public void _RandomTestAll()
        {
            Dictionary<Server, int> serie_chapters = new Dictionary<Server, int>();
            Dictionary<Server, int> chapter_pageslist = new Dictionary<Server, int>();
            Dictionary<Server, int> chapter_images = new Dictionary<Server, int>();
            DateTime last_report = DateTime.Now;
            TimeSpan report_delta = new TimeSpan(0, 15, 0);
            int errors = 0;
            m_pi = new ProgressIndicator("_RandomTestAll");
            Object locker = new Object();

            WriteLine("------------------------------------------------------------------------------");

            DownloadManager.Instance.MangaSettings.MaximumConnectionsPerServer = 4;
            DownloadManager.Instance.MangaSettings.SleepAfterEachDownloadMS = 0;

            foreach (var server in DownloadManager.Instance.Servers)
            {
                serie_chapters[server] = 0;
                chapter_pageslist[server] = 0;
                chapter_images[server] = 0;
            }

            if (File.Exists(TestBase.GetTestFilePath(FILE_EXCEPTION)))
            {
                m_exceptions = new HashSet<string>(
                    File.ReadAllLines(TestBase.GetTestFilePath(FILE_EXCEPTION)));
                WriteLine("Exceptions: {0}", m_exceptions.Count);
            }
            else
            {
                WriteLine("Exceptions: no file");
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
                    WriteLine("Server: {0}, Series: {1}, Chapters: {2}, Pages and images: {3}",
                        server.Name, serie_chapters[server], chapter_pageslist[server], chapter_images[server]);
                }

                WriteLine("Errors: {0}", errors);
                WriteLine("");
            };

            Parallel.ForEach(
                DownloadManager.Instance.Servers,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = DownloadManager.Instance.Servers.Count()
                },
                (server, state) =>
                {
                    //if (!server.Name.Contains("Fox"))
                    //    return;

                    for (; ; )
                    {
                        WriteLine("{0} - Downloading series", server);

                        server.State = ServerState.Waiting;
                        server.DownloadSeries();

                        if (server.State == ServerState.Error)
                        {
                            WriteLineError("ERROR - {0} {1} - Error while downloading series from server",
                                server.Name, server.URL);
                            continue;
                        }
                        else if (server.Series.Count == 0)
                        {
                            WriteLineError("ERROR - {0} {1} - Server have no series",
                                server.Name, server.URL);
                            continue;
                        }
                        else
                        {
                            WriteLine("{0} - Downloaded series", server);
                            break;
                        }
                    }
                });

            DownloadManager.Instance.MangaSettings.MaximumConnectionsPerServer = 1;
            DownloadManager.Instance.MangaSettings.SleepAfterEachDownloadMS = 4000;

            Parallel.ForEach(
                DownloadManager.Instance.Servers,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = DownloadManager.Instance.Servers.Count()
                },
                (server, state) =>
                {
                    //if (!server.Name.Contains("Fox"))
                    //    return;

                    Parallel.ForEach(
                        TakeRandom(server.Series, 0.3),
                        new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer
                        },
                        serie =>
                        {
                            WriteLine("{0} - Downloading chapters", serie);

                            serie.State = SerieState.Waiting;
                            serie.DownloadChapters();
                            serie_chapters[server]++;

                            if (serie.State == SerieState.Error)
                            {
                                WriteLineError("ERROR - {0} {1} - Error while downloading chapters from serie",
                                    serie, serie.URL);
                                errors++;
                            }
                            else
                            {
                                WriteLine("{0} - Downloaded chapters", serie);
                            }

                            Parallel.ForEach(TakeRandom(serie.Chapters, 0.1),
                                new ParallelOptions()
                                {
                                    MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer
                                },
                                (chapter) =>
                                {
                                    WriteLine("{0} - Downloading pages list", chapter);

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

                                        WriteLine("{0} - Downloaded pages list", chapter);
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
                                            WriteLine("{0} - Downloading image", page);

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
                                                            WriteLineErrorForPage(
                                                                "ERROR - {0} {1} - Image stream is zero size for page",
                                                                page);
                                                            errors++;
                                                        }
                                                        else
                                                        {
                                                            try
                                                            {
                                                                System.Drawing.Image.FromStream(stream);

                                                                WriteLine("{0} - Downloaded image", page);
                                                            }
                                                            catch
                                                            {
                                                                WriteLineErrorForPage(
                                                                    "ERROR - {0} {1} - Exception while creating image from stream for page",
                                                                    page);
                                                                errors++;
                                                            }
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        WriteLineErrorForPage(
                                                            "ERROR - {0} {1} - Exception while downloading image from page",
                                                            page);
                                                        errors++;
                                                    }
                                                }
                                                catch
                                                {
                                                    WriteLineErrorForPage(
                                                        "ERROR - {0} {1} - Exception while detecting image url",
                                                        page);
                                                    errors++;
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
    }
}
