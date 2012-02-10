﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    public class SerieInfo
    {
        private string m_url;
        private IEnumerable<ChapterInfo> m_chapters;
        private Object m_lock = new Object();

        public ServerInfo Server { get; private set; }
        internal string URLPart { get; private set; }
        public int DownloadProgress { get; private set; }
        public string Title { get; private set; }
        public SerieState State;

        internal SerieInfo(ServerInfo a_server, string a_url_part, string a_title)
        {
            URLPart = a_url_part;
            Server = a_server;

            Title = a_title.Trim();
            Title = Title.Replace("\t", " ");
            while (Title.IndexOf("  ") != -1)
                Title = Title.Replace("  ", " ");
            Title = HttpUtility.HtmlDecode(Title);
        }

        public IEnumerable<ChapterInfo> Chapters
        {
            [DebuggerStepThrough]
            get
            {
                if (m_chapters == null)
                    return new ChapterInfo[0];

                return from ch in m_chapters
                       select ch;
            }
        }

        public string URL
        {
            get
            {
                if (m_url == null)
                    m_url = HttpUtility.HtmlDecode(Server.Crawler.GetSerieURL(this));

                return m_url;
            }
        }

        internal void DownloadChapters()
        {
            try
            {
                DownloadProgress = 0;

                Server.Crawler.DownloadChapters(this, (progress, result) =>
                {
                    var chapters = result.ToList();

                    if (m_chapters != null)
                    {
                        foreach (var chapter in m_chapters)
                        {
                            var el = chapters.Find(s => (s.Title == chapter.Title) && (s.URL == chapter.URL));
                            if (el != null)
                                chapters[chapters.IndexOf(el)] = chapter;
                        }
                    }

                    m_chapters = chapters;

                    DownloadProgress = progress;
                });

                State = SerieState.Downloaded;

            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                State = SerieState.Error;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Server.Name, Title);
        }

        internal bool DownloadRequired
        {
            get
            {
                lock (m_lock)
                {
                    return (State == SerieState.Error) || (State == SerieState.Initial);
                }
            }
        }
    }
}
