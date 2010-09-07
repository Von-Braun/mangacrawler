using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace MangaCrawlerLib
{
    public class PageInfo
    {
        private string m_URLPart;
        private string m_imageURL;
        private string m_name;
        private string m_url;

        internal int Index;
        internal ChapterInfo ChapterInfo;

        internal string Name
        {
            get
            {
                if (m_name == null)
                    return Index.ToString();
                else
                    return m_name;
            }
            set
            {
                m_name = Crawler.RemoveInvalidFileDirectoryCharacters(value);
            }
        }

        internal string URL
        {
            get
            {
                if (m_url == null)
                    m_url = Crawler.GetPageURL(this);

                return m_url;
            }
        }

        internal Crawler Crawler
        {
            get
            {
                return ChapterInfo.Crawler;
            }
        }

        internal string URLPart
        {
            get
            {
                return m_URLPart;
            }
            set
            {
                m_URLPart = System.Web.HttpUtility.UrlDecode(value);
            }
        }

        internal string ImageURL
        {
            get
            {
                if (m_imageURL == null)
                    m_imageURL = Crawler.GetImageURL(this);

                return m_imageURL;
            }
        }

        public override string ToString()
        {
            return String.Format("{0}/{1}", Index, ChapterInfo.Pages.Count);
        }

        public MemoryStream ImageStream
        {
            get
            {
                try
                {
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(ImageURL);

                    using (Stream image_stream = myReq.GetResponse().GetResponseStream())
                    {
                        MemoryStream mem_stream = new MemoryStream();
                        image_stream.CopyTo(mem_stream);
                        mem_stream.Position = 0;
                        return mem_stream;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
