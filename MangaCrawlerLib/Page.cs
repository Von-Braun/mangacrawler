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
    public class Page : Entity
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private PageState m_state;

        public Chapter Chapter { get; protected set; }
        public int Index { get; protected set; }
        public byte[] Hash { get; protected set; }
        public string ImageURL { get; protected set; }
        public string Name { get; protected set; }
        public string ImageFilePath { get; protected set; }

        internal Page(Chapter a_chapter, string a_url, int a_index, string a_name)
            : this(a_chapter, a_url, a_index, Catalog.NextID(), a_name, null, null, PageState.Initial)
        {
        }

        internal Page(Chapter a_chapter, string a_url, int a_index, ulong a_id, string a_name, byte[] a_hash, 
            string a_image_file_path, PageState a_state) : base(a_id)
        {
            Hash = a_hash;
            ImageFilePath = a_image_file_path;
            m_state = a_state;

            Chapter = a_chapter;
            URL = HttpUtility.HtmlDecode(a_url);
            Index = a_index;

            if (a_name != "")
            {
                a_name = a_name.Trim();
                a_name = a_name.Replace("\t", " ");
                while (a_name.IndexOf("  ") != -1)
                    a_name = a_name.Replace("  ", " ");
                a_name = HttpUtility.HtmlDecode(a_name);
                Name = FileUtils.RemoveInvalidFileDirectoryCharacters(a_name);
            }
            else
                Name = Index.ToString();
        }

        internal override Crawler Crawler
        {
            get
            {
                return Chapter.Crawler;
            }
        }

        public Server Server
        {
            get
            {
                return Chapter.Server;
            }
        }

        public Serie Serie
        {
            get
            {
                return Chapter.Serie;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}/{2}",
                    Chapter, Index, Chapter.Pages.Count);
        }

        internal MemoryStream GetImageStream()
        {
            if (ImageURL == null)
                ImageURL = HttpUtility.HtmlDecode(Crawler.GetImageURL(this));

            return Crawler.GetImageStream(this);  
        }

        internal void DownloadAndSavePageImage(string a_chapter_dir, bool a_cbz)
        {
            try
            {
                new DirectoryInfo(a_chapter_dir).Create();

                FileInfo temp_file = new FileInfo(Path.GetTempFileName());

                try
                {

                    using (FileStream file_stream = new FileStream(temp_file.FullName, FileMode.Create))
                    {
                        MemoryStream ms = null;

                        try
                        {
                            ms = GetImageStream();
                        }
                        catch (WebException ex)
                        {
                            Loggers.MangaCrawler.Fatal("Exception #1", ex);
                            throw;
                        }

                        try
                        {
                            System.Drawing.Image.FromStream(ms);
                            ms.Position = 0;
                        }
                        catch (Exception ex)
                        {
                            Loggers.MangaCrawler.Fatal("Exception #2", ex);
                            throw;
                        }

                        ms.CopyTo(file_stream);

                        ms.Position = 0;
                        byte[] hash;
                        TomanuExtensions.Utils.Hash.CalculateSHA256(ms, out hash);
                        Hash = hash;
                    }

                    ImageFilePath = a_chapter_dir +
                        FileUtils.RemoveInvalidFileDirectoryCharacters(Name) +
                        FileUtils.RemoveInvalidFileDirectoryCharacters(
                            Path.GetExtension(ImageURL).ToLower());

                    FileInfo image_file = new FileInfo(ImageFilePath);

                    if (image_file.Exists)
                        image_file.Delete();

                    temp_file.MoveTo(image_file.FullName);

                    State = PageState.Downloaded;
                }
                finally
                {
                    if (temp_file.Exists)
                        if (temp_file.FullName != ImageFilePath)
                            temp_file.Delete();
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Fatal("Exception #3", ex);
                State = PageState.Error;
            }
        }

        internal bool RequiredDownload(string a_chapter_dir)
        {
            try
            {
                if (ImageFilePath == null)
                    return true;
                if (new FileInfo(ImageFilePath).Directory.FullName + "\\" != a_chapter_dir)
                    return true;
                if (!new FileInfo(ImageFilePath).Exists)
                    return true;
                if (Hash == null)
                    return true;

                byte[] hash;
                TomanuExtensions.Utils.Hash.CalculateSHA256(new FileInfo(ImageFilePath).OpenRead(), out hash);

                if (!Hash.SequenceEqual(hash))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Loggers.MangaCrawler.Fatal("Exception", ex);
                return true;
            }
        }

        public PageState State
        {
            get
            {
                return m_state;
            }
            internal set
            {
                switch (value)
                {
                    case PageState.Downloaded:
                    {
                        Debug.Assert(State == PageState.Downloading);
                        break;
                    }
                    case PageState.Downloading:
                    {
                        Debug.Assert(State == PageState.Waiting);
                        break;
                    }
                    case PageState.Error:
                    {
                        Debug.Assert(State == PageState.Downloading);
                        break;
                    }
                    
                    case PageState.Waiting:
                    {
                        Debug.Assert((State == PageState.Downloaded) ||
                                     (State == PageState.Initial) ||
                                     (State == PageState.Error));
                        break;
                    }

                    default:
                    {
                        throw new InvalidOperationException(String.Format("Unknown state: {0}", value));
                    }
                }

                m_state = value;
            }
        }
    }
}
