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
        public virtual int ID { get; protected internal set; }
        public virtual Chapter Chapter { get; protected internal set; }
        public virtual DateTime LastChange { get; protected internal set; }
        public virtual int Index { get; protected internal set; }
        public virtual string URL { get; protected internal set; }
        public virtual bool Downloaded { get; protected internal set; }
        public virtual string ImageFilePath { get; protected internal set; }
        public virtual string Name { get; protected internal set; }
        public virtual string ImageURL { get; protected internal set; }
        public virtual byte[] Hash { get; protected internal set; }

        protected internal Page()
        {
        }

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

        private void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<Page>(m =>
            {
                m.Id(c => c.ID, mapper => mapper.Generator(Generators.Native));
                m.Version(c => c.LastChange, mapper => { });
                m.Property(c => c.Index);
                m.Property(c => c.URL, mapping => mapping.NotNullable(true));
                m.Property(c => c.Downloaded);
                m.Property(c => c.ImageFilePath, mapping => mapping.NotNullable(true));
                m.Property(c => c.Name, mapping => mapping.NotNullable(true));
                m.Property(c => c.ImageURL, mapping => mapping.NotNullable(true));
                m.Property(c => c.Hash, mapping => mapping.NotNullable(true));
                m.ManyToOne(c => c.Chapter, mapping => mapping.NotNullable(true));
            });
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}/{2}",
                    Chapter, Index, Chapter.Pages.Count());
        }

        protected internal virtual MemoryStream GetImageStream()
        {
            if (ImageURL == null)
                ImageURL = HttpUtility.HtmlDecode(Chapter.Serie.Server.Crawler.GetImageURL(this));

            return Chapter.Serie.Server.Crawler.GetImageStream(this);  
        }

        protected internal virtual void DownloadAndSavePageImage()
        {
            if (Chapter.Token.IsCancellationRequested)
            {
                Loggers.Cancellation.InfoFormat(
                    "#2 cancellation requested, work: {0} state: {1}",
                    this, Chapter.State);

                Chapter.Token.ThrowIfCancellationRequested();
            }

            ImageURL = HttpUtility.HtmlDecode(Chapter.Serie.Server.Crawler.GetImageURL(this));

            ImageFilePath = Chapter.ChapterDir +
                FileUtils.RemoveInvalidFileDirectoryCharacters(Name) +
                FileUtils.RemoveInvalidFileDirectoryCharacters(
                    Path.GetExtension(ImageURL).ToLower());

            LastChange = DateTime.Now;

            new DirectoryInfo(Chapter.ChapterDir).Create();

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
