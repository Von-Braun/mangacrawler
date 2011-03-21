using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Diagnostics;
using System.Threading;

namespace MangaCrawlerLib
{
    [DebuggerDisplay("PageInfo, {ToString()}")]
    public class PageInfo
    {
        private string m_imageURL;
        private string m_name;
        private string m_url;
        private string m_urlPart;
        private int m_index;
        private string m_imageFilePath;
        private ChapterInfo m_chapterInfo;

        internal PageInfo(ChapterInfo a_chapterInfo, string a_urlPart, int a_index, string a_name = null)
        {
            m_chapterInfo = a_chapterInfo;
            m_urlPart = a_urlPart;
            m_index = a_index;

            if (a_name != null)
            {
                a_name = a_name.Trim();
                a_name = a_name.Replace("\t", " ");
                while (a_name.IndexOf("  ") != -1)
                    a_name = a_name.Replace("  ", " ");


                m_name = FileUtils.RemoveInvalidFileDirectoryCharacters(a_name);
            }
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

        internal string GetImageURL(CancellationToken a_token)
        {
            if (m_imageURL == null)
                m_imageURL = HttpUtility.HtmlDecode(Crawler.GetImageURL(this, a_token));

            return m_imageURL;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}/{2}",
                    ChapterInfo, m_index, m_chapterInfo.Pages.Count());
        }

        internal MemoryStream GetImageStream(CancellationToken a_token)
        {
            return ConnectionsLimiter.GetImageStream(this, a_token);
        }

        internal MemoryStream GetImageStream()
        {
            return GetImageStream(new CancellationTokenSource().Token);
        }

        public string GetImageFilePath()
        {
            return m_imageFilePath;
        }

        public bool DownloadAndSavePageImage(CancellationToken a_token, string a_dir)
        {
            m_imageFilePath = a_dir +
                FileUtils.RemoveInvalidFileDirectoryCharacters(Name) +
                FileUtils.RemoveInvalidFileDirectoryCharacters(
                    Path.GetExtension(GetImageURL(a_token)));

            FileInfo image_file = new FileInfo(m_imageFilePath);

            new DirectoryInfo(a_dir).Create();

            FileInfo temp_file = new FileInfo(Path.GetTempFileName());

            try
            {
                using (FileStream file_stream = new FileStream(temp_file.FullName, FileMode.Create))
                {
                    Stream ims = null;

                    try
                    {
                        ims = GetImageStream(a_token);
                    }
                    catch (WebException)
                    {
                        // Some images are unavailable, if we get null pernamently tests
                        // will detect this.
                        return false;
                    }

                    try
                    {
                        System.Drawing.Image.FromStream(ims);
                        ims.Position = 0;
                    }
                    catch
                    {
                        // Some junks.
                        return false;
                    }

                    ims.CopyTo(file_stream);
                }

                if (image_file.Exists)
                    image_file.Delete();
                temp_file.MoveTo(image_file.FullName);
            }
            catch
            {
                temp_file.Delete();
                throw;
            }

            return true;
        }
    }
}
