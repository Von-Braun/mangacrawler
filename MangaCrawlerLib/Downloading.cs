using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    /// <summary>
    /// Thread safe, copy on write semantic.
    /// </summary>
    public class Downloading
    {
        private List<Chapter> m_downloading = new List<Chapter>();

        internal void Load()
        {
            IEnumerable<Chapter> downloading = from chapter in Catalog.LoadDownloadings()
                                               orderby chapter.LimiterOrder
                                               select chapter;

            DownloadManager.Instance.DownloadPages(downloading.ToList());
        }

        /// <summary>
        /// Thread safe.
        /// </summary>
        public IEnumerable<Chapter> List
        {
            get
            {
                m_downloading = (from chapter in m_downloading
                                 where chapter.State != ChapterState.Cancelled
                                 select chapter).ToList();

                return m_downloading;
            }
        }  

        public void Save()
        {
            Catalog.SaveDownloading(m_downloading.Where(c => c.IsDownloading).ToList());
        }

        public void Remove(Chapter a_chapter)
        {
            m_downloading = m_downloading.Except(a_chapter).ToList();
        }

        internal void Add(Chapter a_chapter)
        {
            if (m_downloading.Contains(a_chapter))
                return;

            var copy = m_downloading.ToList();
            copy.Add(a_chapter);
            m_downloading = copy;
        }
    }
}
