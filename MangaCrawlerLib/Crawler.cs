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
        public abstract void DownloadSeries(ServerInfo a_info, Action<int, IEnumerable<SerieInfo>> a_progress_callback);
        public abstract void DownloadChapters(SerieInfo a_info, Action<int, IEnumerable<ChapterInfo>> a_progress_callback);
        public abstract IEnumerable<PageInfo> DownloadPages(TaskInfo a_info);
        public abstract string GetImageURL(PageInfo a_info);

        public abstract string GetServerURL();

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

        public HtmlDocument DownloadDocument(ServerInfo a_info)
        {
            return DownloadDocument(a_info, a_info.URL, CancellationToken.None);
        }

        public HtmlDocument DownloadDocument(ServerInfo a_info, string a_url)
        {
            return DownloadDocument(a_info, a_url, CancellationToken.None);
        }

        public HtmlDocument DownloadDocument(SerieInfo a_info)
        {
            return DownloadDocument(a_info.Server, a_info.URL, CancellationToken.None);
        }

        public HtmlDocument DownloadDocument(PageInfo a_info)
        {
            return DownloadDocument(a_info.TaskInfo.Server, a_info.URL, CancellationToken.None);
        }

        public HtmlDocument DownloadDocument(TaskInfo a_info)
        {
            return DownloadDocument(a_info.Server, a_info.URL, CancellationToken.None);
        }

        public virtual HtmlDocument DownloadDocument(ServerInfo a_info, string a_url, CancellationToken a_token)
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

                ConnectionsLimiter.Aquire(a_info, a_token, Priority.Series);

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
                    ConnectionsLimiter.Release(a_info);
                }
            });
        }

        public virtual MemoryStream GetImageStream(PageInfo a_info)
        {
            return DownloadWithRetry(() =>
            {
                try
                {
                    ConnectionsLimiter.Aquire(a_info.TaskInfo.Server, a_info.TaskInfo.Token, Priority.Image);

                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(
                        a_info.GetImageURL());

                    myReq.UserAgent = DownloadManager.UserAgent;
                    myReq.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    myReq.Referer = a_info.URL;

                    using (Stream image_stream = myReq.GetResponse().GetResponseStream())
                    {
                        MemoryStream mem_stream = new MemoryStream();
                        image_stream.CopyTo(mem_stream);
                        mem_stream.Position = 0;
                        return mem_stream;
                    }
                }
                finally
                {
                    ConnectionsLimiter.Release(a_info.TaskInfo.Server);
                }
            });
        }

        public virtual string GetSerieURL(SerieInfo a_info)
        {
            return a_info.URLPart;
        }

        public virtual string GetChapterURL(ChapterInfo a_info)
        {
            return a_info.URLPart;
        }

        public virtual string GetPageURL(PageInfo a_info)
        {
            return a_info.URLPart;
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
