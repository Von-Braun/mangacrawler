using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Diagnostics;
using System.Threading;
using TomanuExtensions.Utils;

namespace MangaCrawlerLib
{
    public class Page
    {
        private string m_imageURL;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string m_name;

        private string m_imageFilePath;

        internal Work Work { get; private set; }
        internal int Index { get; private set; }
        internal string URL { get; private set; }
        internal bool Downloaded { get; private set; }

        internal Page(Work a_work, string a_url, int a_index, string a_name = null)
        {
            Work = a_work;
            URL = HttpUtility.HtmlDecode(a_url);
            Index = a_index;

            if (a_name != null)
            {
                a_name = a_name.Trim();
                a_name = a_name.Replace("\t", " ");
                while (a_name.IndexOf("  ") != -1)
                    a_name = a_name.Replace("  ", " ");
                m_name = HttpUtility.HtmlDecode(m_name);
                m_name = FileUtils.RemoveInvalidFileDirectoryCharacters(a_name);
            }
        }

        internal string Name
        {
            get
            {
                if (m_name == null)
                    return Index.ToString();
                else
                    return m_name;
            }
        }

        internal string GetImageURL()
        {
            if (m_imageURL == null)
                m_imageURL = HttpUtility.HtmlDecode(Work.Chapter.Serie.Server.Crawler.GetImageURL(this));

            return m_imageURL;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}/{2}",
                    Work, Index, Work.Pages.Count());
        }

        internal MemoryStream GetImageStream()
        {
            return Work.Chapter.Serie.Server.Crawler.GetImageStream(this);  
        }

        public string GetImageFilePath()
        {
            return m_imageFilePath;
        }

        public void DownloadAndSavePageImage()
        {
            if (Work.Token.IsCancellationRequested)
            {
                Loggers.Cancellation.InfoFormat(
                    "#2 cancellation requested, work: {0} state: {1}",
                    this, Work.State);

                Work.Token.ThrowIfCancellationRequested();
            }

            m_imageFilePath = Work.ChapterDir +
                FileUtils.RemoveInvalidFileDirectoryCharacters(Name) +
                FileUtils.RemoveInvalidFileDirectoryCharacters(
                    Path.GetExtension(GetImageURL()).ToLower());

            new DirectoryInfo(Work.ChapterDir).Create();

            FileInfo temp_file = new FileInfo(Path.GetTempFileName());

            try
            {

                using (FileStream file_stream = new FileStream(temp_file.FullName, FileMode.Create))
                {
                    Stream ims = null;

                    try
                    {
                        ims = GetImageStream();
                    }
                    catch (WebException)
                    {
                        // Some images are unavailable, if we get null pernamently tests
                        // will detect this.
                        return;
                    }

                    try
                    {
                        System.Drawing.Image.FromStream(ims);
                        ims.Position = 0;
                    }
                    catch
                    {
                        // Some junks.
                        return;
                    }

                    ims.CopyTo(file_stream);
                }

                FileInfo image_file = new FileInfo(m_imageFilePath);

                if (image_file.Exists)
                    image_file.Delete();

                temp_file.MoveTo(image_file.FullName);
            }
            catch 
            {
                temp_file.Delete();
                throw;
            }

            Downloaded = true;
        }
    }
}
