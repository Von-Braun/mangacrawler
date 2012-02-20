using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using HtmlAgilityPack;

namespace MangaCrawlerLib
{
    internal abstract class Crawler
    {
        public abstract string Name { get; }

        public abstract void DownloadSeries(Server a_server, Action<int, IEnumerable<Serie>> a_progress_callback);
        public abstract void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback);
        public abstract IEnumerable<Page> DownloadPages(Chapter a_chapter);
        public abstract string GetServerURL();
        public abstract string GetImageURL(Page a_page);

        public static T DownloadWithRetry<T>(Func<T> a_func)
        {
            WebException ex1 = null;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    return a_func();
                }
                catch (WebException ex)
                {
                    Loggers.MangaCrawler.Info("exception, {0}", ex);

                    ex1 = ex;
                    continue;
                }
            }

            throw ex1;
        }

        public HtmlDocument DownloadDocument(Server a_server)
        {
            return DownloadDocument(a_server, a_server.URL, CancellationToken.None);
        }

        public HtmlDocument DownloadDocument(Server a_server, string a_url)
        {
            return DownloadDocument(a_server, a_url, CancellationToken.None);
        }

        public HtmlDocument DownloadDocument(Serie a_serie)
        {
            return DownloadDocument(a_serie.Server, a_serie.URL, CancellationToken.None);
        }

        public HtmlDocument DownloadDocument(Page a_page)
        {
            return DownloadDocument(a_page.Chapter.Work.Chapter.Serie.Server, a_page.URL, CancellationToken.None);
        }

        public HtmlDocument DownloadDocument(Chapter a_chapter)
        {
            return DownloadDocument(a_chapter.Serie.Server, a_chapter.URL, CancellationToken.None);
        }

        public virtual HtmlDocument DownloadDocument(Server a_server, string a_url, CancellationToken a_token)
        {
            return Crawler.DownloadWithRetry(() =>
            {
                if (a_token != CancellationToken.None)
                {
                    if (a_token.IsCancellationRequested)
                    {
                        Loggers.Cancellation.InfoFormat(
                            "Page - #1 token cancelled, a_url: {0}",
                            a_url);

                        a_token.ThrowIfCancellationRequested();
                    }
                }

                ConnectionsLimiter.Aquire(a_server, a_token, Priority.Series);

                try
                {
                    var web = new HtmlWeb();
                    var page = web.Load(a_url);

                    if (web.StatusCode == HttpStatusCode.NotFound)
                    {
                        Loggers.MangaCrawler.InfoFormat(
                            "ConnectionsLimiter.DownloadDocument - series - page was not found, url: {0}",
                            a_url);

                        return null;
                    }

                    return page;
                }
                finally
                {
                    ConnectionsLimiter.Release(a_server);
                }
            });
        }

        public virtual MemoryStream GetImageStream(Page a_page)
        {
            return DownloadWithRetry(() =>
            {
                try
                {
                    ConnectionsLimiter.Aquire(a_page.Chapter.Serie.Server, a_page.Chapter.Work.Token, Priority.Image);

                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(
                        a_page.GetImageURL());

                    myReq.UserAgent = DownloadManager.UserAgent;
                    myReq.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    myReq.Referer = a_page.URL;

                    byte[] buffer = new byte[4*1024];

                    using (Stream image_stream = myReq.GetResponse().GetResponseStream())
                    {
                        MemoryStream mem_stream = new MemoryStream();

                        for (;;)
                        {
                            int readed = image_stream.Read(buffer, 0, buffer.Length);

                            if (readed == 0)
                                break;

                            if (a_page.Chapter.Work.Token.IsCancellationRequested)
                            {
                                Loggers.Cancellation.InfoFormat(
                                    "cancellation requested, work: {0} state: {1}",
                                    this, a_page.Chapter.Work.State);

                                a_page.Chapter.Work.Token.ThrowIfCancellationRequested();
                            }

                            mem_stream.Write(buffer, 0, readed);
                        }

                        mem_stream.Position = 0;
                        return mem_stream;
                    }
                }
                finally
                {
                    ConnectionsLimiter.Release(a_page.Chapter.Serie.Server);
                }
            });
        }

        public virtual int MaxConnectionsPerServer
        {
            get
            {
                return ConnectionsLimiter.MAX_CONNECTIONS_PER_SERVER;
            }
        }
    }
}
