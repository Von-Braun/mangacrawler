using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    public class PageInfo
    {
        private string m_imageURL;
        private string m_name;
        private string m_url;
        private string m_urlPart;
        private int m_index;
        private ChapterInfo m_chapterInfo;

        internal PageInfo(ChapterInfo a_chapterInfo, string a_urlPart, int a_index, string a_name = null)
        {
            m_chapterInfo = a_chapterInfo;
            m_urlPart = a_urlPart;
            m_index = a_index;

            if (a_name != null)
                m_name = Crawler.RemoveInvalidFileDirectoryCharacters(a_name);
        }

        internal ChapterInfo ChapterInfo
        {
            get
            {
                return m_chapterInfo;
            }
        }

        internal int Index
        {
            get
            {
                return m_index;
            }
        }

        internal string URLPart
        {
            get
            {
                return m_urlPart;
            }
        }

        internal string Name
        {
            get
            {
                if (m_name == null)
                    return m_index.ToString();
                else
                    return m_name;
            }
        }

        internal string URL
        {
            get
            {
                if (m_url == null)
                    m_url = HttpUtility.HtmlDecode(Crawler.GetPageURL(this));

                return m_url;
            }
        }

        internal Crawler Crawler
        {
            get
            {
                return m_chapterInfo.Crawler;
            }
        }

        internal string ImageURL
        {
            get
            {
                if (m_imageURL == null)
                    m_imageURL = HttpUtility.HtmlDecode(Crawler.GetImageURL(this));

                return m_imageURL;
            }
        }

        public override string ToString()
        {
            return String.Format("{0}/{1}", m_index, m_chapterInfo.Pages.Count());
        }

        public MemoryStream ImageStream
        {
            get
            {
                try
                {
                    ConnectionsLimiter.Aquire(ChapterInfo.SerieInfo.ServerInfo);

                    try
                    {
                        HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(ImageURL);

                        myReq.UserAgent =
                            "Mozilla/5.0 (Windows; U; Windows NT 6.0; pl; rv:1.9.2.8) Gecko/20100722 Firefox/3.6.8 ( .NET CLR 3.5.30729; .NET4.0E)";
                        myReq.Referer = URL;

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
                        ConnectionsLimiter.Release(ChapterInfo.SerieInfo.ServerInfo);
                    }
                }
                catch
                {
                    Debug.WriteLine(ImageURL);
                    return null;
                }
            }
        }
    }
}
