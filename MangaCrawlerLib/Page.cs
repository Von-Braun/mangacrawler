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
using NHibernate.Mapping.ByCode;

namespace MangaCrawlerLib
{
    public class Page : IClassMapping
    {
        public virtual int ID { get; private set; }
        public virtual Chapter Chapter { get; private set; }
        public virtual DateTime LastChange { get; private set; }
        public virtual int Index { get; private set; }
        public virtual string URL { get; private set; }
        public virtual bool Downloaded { get; private set; }
        public virtual string ImageFilePath { get; private set; }
        public virtual string Name { get; private set; }
        public virtual string ImageURL { get; private set; }
        public virtual byte[] Hash { get; private set; }

        internal Page(Chapter a_chapter, string a_url, int a_index, string a_name = null)
        {
            ID = IDGenerator.Next();
            Chapter = a_chapter;
            URL = HttpUtility.HtmlDecode(a_url);
            Index = a_index;
            LastChange = DateTime.Now;

            if (a_name != null)
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

        public void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<Page>(m =>
            {
                m.Lazy(true);
                m.Id(c => c.ID);
                m.Property(c => c.Chapter);
                m.Property(c => c.LastChange);
                m.Property(c => c.Index);
                m.Property(c => c.URL);
                m.Property(c => c.Downloaded);
                m.Property(c => c.ImageFilePath);
                m.Property(c => c.Name);
                m.Property(c => c.ImageURL);
                m.Property(c => c.Hash);
            });
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}/{2}",
                    Chapter, Index, Chapter.Pages.Count());
        }

        internal MemoryStream GetImageStream()
        {
            return Chapter.Serie.Server.Crawler.GetImageStream(this);  
        }

        public void DownloadAndSavePageImage()
        {
            if (Chapter.Work.Token.IsCancellationRequested)
            {
                Loggers.Cancellation.InfoFormat(
                    "#2 cancellation requested, work: {0} state: {1}",
                    this, Chapter.State);

                Chapter.Work.Token.ThrowIfCancellationRequested();
            }

            ImageURL = HttpUtility.HtmlDecode(Chapter.Serie.Server.Crawler.GetImageURL(this));

            ImageFilePath = Chapter.Work.ChapterDir +
                FileUtils.RemoveInvalidFileDirectoryCharacters(Name) +
                FileUtils.RemoveInvalidFileDirectoryCharacters(
                    Path.GetExtension(ImageURL).ToLower());

            LastChange = DateTime.Now;

            new DirectoryInfo(Chapter.Work.ChapterDir).Create();

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
                    catch (WebException)
                    {
                        // Some images are unavailable, if we get null pernamently tests
                        // will detect this.
                        return;
                    }

                    try
                    {
                        System.Drawing.Image.FromStream(ms);
                        ms.Position = 0;
                    }
                    catch
                    {
                        // Some junks.
                        return;
                    }

                    ms.CopyTo(file_stream);

                    ms.Position = 0;
                    byte[] hash;
                    TomanuExtensions.Utils.Hash.CalculateSHA256(ms, out hash);
                    Hash = hash;
                }

                FileInfo image_file = new FileInfo(ImageFilePath);

                if (image_file.Exists)
                    image_file.Delete();

                temp_file.MoveTo(image_file.FullName);
            }
            catch 
            {
                temp_file.Delete();
                throw;
            }

            LastChange = DateTime.Now;
            Downloaded = true;
        }
    }
}
