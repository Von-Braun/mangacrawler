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
        private string m_directory;
        private ChapterInfo m_chapterInfo;

        public volatile bool Deleted;
        public volatile bool Processing;

        public QueueChapter(ChapterInfo a_chapterInfo, string a_directoryBase)
        {
            m_chapterInfo = a_chapterInfo;

            if (a_directoryBase.Last() == Path.DirectorySeparatorChar)
                a_directoryBase = a_directoryBase.RemoveFromRight(1);

            m_directory =
                a_directoryBase +
                Path.DirectorySeparatorChar +
                Crawler.RemoveInvalidFileDirectoryCharacters(m_chapterInfo.SerieInfo.ServerInfo.Name) +
                Path.DirectorySeparatorChar +
                Crawler.RemoveInvalidFileDirectoryCharacters(m_chapterInfo.SerieInfo.Name) +
                Path.DirectorySeparatorChar +
                Crawler.RemoveInvalidFileDirectoryCharacters(m_chapterInfo.Name) +
                Path.DirectorySeparatorChar;
        }

        public ChapterInfo ChapterInfo
        {
            get
            {
                return m_chapterInfo;
            }
        }

        public string Directory
        {
            get 
            { 
                return m_directory; 
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} - {2} ({3})", m_chapterInfo.SerieInfo.ServerInfo.Name, 
                m_chapterInfo.SerieInfo.Name, m_chapterInfo.Name, Directory);
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
