using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace MangaCrawlerLib
{
    public class QueueChapter
    {
        public ChapterInfo ChapterInfo;
        private string m_directory;
        public volatile bool Deleted;
        public volatile bool Processing;

        public string Directory
        {
            get 
            { 
                return m_directory; 
            }
        }

        public string DirectoryBase
        {
            set
            {
                if (value.Last() == Path.DirectorySeparatorChar)
                    value = value.RemoveFromRight(1);

                m_directory = value + Path.DirectorySeparatorChar + Crawler.RemoveInvalidFileDirectoryCharacters(ChapterInfo.SerieInfo.ServerInfo.Name) +
                    Path.DirectorySeparatorChar + Crawler.RemoveInvalidFileDirectoryCharacters(ChapterInfo.SerieInfo.Name) + Path.DirectorySeparatorChar +
                    Crawler.RemoveInvalidFileDirectoryCharacters(ChapterInfo.Name) + Path.DirectorySeparatorChar;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} - {2} ({3})", ChapterInfo.SerieInfo.ServerInfo.Name, ChapterInfo.SerieInfo.Name, 
                ChapterInfo.Name, Directory);
        }

        public void DownloadAndSavePageImage(PageInfo a_info)
        {
            a_info.ChapterInfo.DownloadedPages++;    

            FileInfo image_file = new FileInfo(Directory +
                Crawler.RemoveInvalidFileDirectoryCharacters(a_info.Name) + 
                Crawler.RemoveInvalidFileDirectoryCharacters(Path.GetExtension(a_info.ImageURL)));

            new DirectoryInfo(Directory).Create();
            
            FileInfo temp_file = new FileInfo(Path.GetTempFileName());

            try
            {
                using (FileStream file_stream = new FileStream(temp_file.FullName, FileMode.Create))
                    a_info.ImageStream.CopyTo(file_stream);

                if (image_file.Exists)
                    image_file.Delete();
                temp_file.MoveTo(image_file.FullName);
            }
            catch
            {
                temp_file.Delete();
                throw;
            }
        }
    }
}
