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
        internal string URLPart;
        private string m_imageURL;
        private string m_name;
        private string m_url;

        public int Index;
        public ChapterInfo ChapterInfo;

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
                    m_url = HttpUtility.HtmlDecode(Crawler.GetPageURL(this));

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
            return String.Format("{0}/{1}", Index, ChapterInfo.Pages.Count);
        }

        public MemoryStream ImageStream
        {
            get
            {
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
                catch
                {
                    Debug.WriteLine(ImageURL);
                    return null;
                }
            }
        }
    }
}
