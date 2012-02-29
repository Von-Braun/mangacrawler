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
    public abstract class Crawler
    {
        public abstract string Name { get; }

        internal abstract void DownloadSeries(Server a_server, Action<int, IEnumerable<Serie>> a_progress_callback);
        internal abstract void DownloadChapters(Serie a_serie, Action<int, IEnumerable<Chapter>> a_progress_callback);
        internal abstract IEnumerable<Page> DownloadPages(Chapter a_chapter);
        public abstract string GetServerURL();
        internal abstract string GetImageURL(Page a_page);

        internal static T DownloadWithRetry<T>(Func<T> a_func)
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

        internal HtmlDocument DownloadDocument(Server a_server)
        {
            return DownloadDocument(a_server, a_server.URL, CancellationToken.None, 
                () => NH.TransactionLockUpdate(a_server, () => a_server.DownloadingStarted()));
        }

        internal HtmlDocument DownloadDocument(Server a_server, string a_url)
        {
            return DownloadDocument(a_server, a_url, CancellationToken.None,
                () => NH.TransactionLockUpdate(a_server, () => a_server.DownloadingStarted()));
        }

        internal HtmlDocument DownloadDocument(Serie a_serie)
        {
            return DownloadDocument(a_serie.Server, a_serie.URL, CancellationToken.None,
                () => NH.TransactionLockUpdate(a_serie, () => a_serie.DownloadingStarted()));
        }

        internal HtmlDocument DownloadDocument(Page a_page)
        {
            return DownloadDocument(a_page.Server, a_page.URL, CancellationToken.None, null);
        }

        internal HtmlDocument DownloadDocument(Chapter a_chapter)
        {
            return DownloadDocument(a_chapter.Server, a_chapter.URL, CancellationToken.None,
                () => NH.TransactionLockUpdate(a_chapter, () => a_chapter.DownloadingStarted()));
        }

        internal virtual HtmlDocument DownloadDocument(Server a_server, string a_url, CancellationToken a_token, 
            Action a_started)
        {
            return DownloadWithRetry(() =>
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

                if (a_started != null)
                    a_started();

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

        internal virtual MemoryStream GetImageStream(Page a_page)
        {
            return DownloadWithRetry(() =>
            {
                try
                {
                    ConnectionsLimiter.Aquire(a_page.Server, a_page.Chapter.Token, Priority.Image);

                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(
                        a_page.ImageURL);

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

                            if (a_page.Chapter.Token.IsCancellationRequested)
                            {
                                Loggers.Cancellation.InfoFormat(
                                    "cancellation requested, work: {0} state: {1}",
                                    this, a_page.Chapter.State);

                                a_page.Chapter.Token.ThrowIfCancellationRequested();
                            }

                            mem_stream.Write(buffer, 0, readed);
                        }

                        mem_stream.Position = 0;
                        return mem_stream;
                    }
                }
                finally
                {
                    ConnectionsLimiter.Release(a_page.Server);
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
