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

namespace MangaCrawlerTest
{
    [TestClass]
    public class RandomTestAll : TestBase
    {
        private static string EXCEPTIONS = "Exceptions.txt";
        private static string EXCEPTIONS_CANDIDATES = "Exceptions-candidates.txt";

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
            var exceptions = new List<string>();
            var exceptions_candidates = new List<string>();
            m_pi = new ProgressIndicator("RandomTestAll");
            Object locker = new Object();

            if (File.Exists(Path.Combine(GetTestDataDir(), EXCEPTIONS)))
            {
                exceptions = File.ReadAllLines(Path.Combine(GetTestDataDir(), EXCEPTIONS)).ToList();
                WriteLine("{0} entries in {1}", exceptions.Count, EXCEPTIONS);
            }
            else
            {
                WriteLine("File doesn't exists: {0}", EXCEPTIONS);
            }

            if (File.Exists(Path.Combine(GetTestDataDir(), EXCEPTIONS_CANDIDATES)))
            {
                exceptions_candidates = File.ReadAllLines(Path.Combine(GetTestDataDir(), EXCEPTIONS_CANDIDATES)).ToList();
                WriteLine("{0} entries in {1}", exceptions_candidates.Count, EXCEPTIONS_CANDIDATES);
            }
            else
            {
                WriteLine("File doesn't exists: {0}", EXCEPTIONS_CANDIDATES);
            }

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
                    MaxDegreeOfParallelism = DownloadManager.Instance.Servers.Count(),
                    TaskScheduler = Limiter.Scheduler
                },
                server =>
                {
                    server.State = ServerState.Waiting;
                    server.DownloadSeries();

                    if (server.State == ServerState.Error)
                    {
                        WriteLineError("ERROR - {0} - Error while downloading series from server",
                            server.Name);
                        Assert.Fail();
                    }
                    else if (server.Series.Count == 0)
                    {
                        WriteLineError("ERROR - {0} - Server have no series", server.Name);
                        Assert.Fail();
                    }

                    Parallel.ForEach(TakeRandom(server.Series, 0.1),
                        new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer,
                            TaskScheduler = Limiter.Scheduler
                        },
                        serie =>
                        {
                            serie.State = SerieState.Waiting;
                            serie.DownloadChapters();
                            serie_chapters[server]++;

                            if (serie.State == SerieState.Error)
                            {
                                WriteLineError(
                                    "ERROR - {0} - Error while downloading chapters from serie", serie);
                                errors++;
                            }
                            else if (serie.Chapters.Count == 0)
                            {
                                if (!exceptions.Contains(serie.ToString()))
                                {
                                    exceptions_candidates.Add(serie.ToString());
                                    File.WriteAllLines(Path.Combine(GetTestDataDir(), EXCEPTIONS_CANDIDATES), exceptions_candidates);
                                    WriteLineWarning("WARN - {0} - Serie has no chapters", serie);
                                    warnings++;
                                }
                            }

                            Parallel.ForEach(TakeRandom(serie.Chapters, 0.1),
                                new ParallelOptions()
                                {
                                    MaxDegreeOfParallelism = server.Crawler.MaxConnectionsPerServer,
                                    TaskScheduler = Limiter.Scheduler
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
                                            if (!exceptions.Contains(chapter.ToString()))
                                            {
                                                exceptions_candidates.Add(chapter.ToString());
                                                File.WriteAllLines(Path.Combine(GetTestDataDir(), EXCEPTIONS_CANDIDATES), 
                                                    exceptions_candidates);
                                                WriteLineWarning("WARN - {0} - Chapter have no pages", chapter);
                                                warnings++;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        WriteLineError(
                                            "ERROR - {0} - Exception while downloading pages from chapter", chapter);
                                        errors++;
                                    }

                                    Parallel.ForEach(TakeRandom(chapter.Pages, 0.1),
                                        new ParallelOptions()
                                        {
                                            MaxDegreeOfParallelism = chapter.Crawler.MaxConnectionsPerServer,
                                            TaskScheduler = Limiter.Scheduler
                                        },
                                        (page) =>
                                        {
                                            Limiter.BeginChapter(chapter);

                                            try
                                            {
                                                MemoryStream stream = null;

                                                try
                                                {
                                                    stream = page.GetImageStream();
                                                }
                                                catch
                                                {
                                                    WriteLineError(
                                                        "ERROR - {0} - Exception while downloading image from page", page);
                                                    errors++;
                                                }

                                                if (stream.Length == 0)
                                                {
                                                    WriteLineError(
                                                        "ERROR - {0} - Image stream is zero size for page", page);
                                                    errors++;
                                                }

                                                try
                                                {
                                                    System.Drawing.Image.FromStream(stream);
                                                }
                                                catch
                                                {
                                                    WriteLineError(
                                                        "ERROR - {0} - Exception while creating image from stream for page", page);
                                                    errors++;
                                                }
                                            }
                                            catch
                                            {
                                                WriteLineError(
                                                        "ERROR - {0} - EXCEPTION while downloading page", page);
                                                errors++;
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
