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
    public class Works
    {
        private List<Chapter> m_works = new List<Chapter>();

        internal void Load()
        {
            IEnumerable<Chapter> works = from work in Catalog.LoadWorks()
                                         orderby work.LimiterOrder
                                         where work.State == ChapterState.Initial
                                         select work;

            DownloadManager.Instance.DownloadPages(works);
        }

        public IEnumerable<Chapter> List
        {
            get
            {
                m_works = (from work in m_works
                           where work.State != ChapterState.Deleted
                           select work).ToList();

                return m_works;
            }
        }  

        public void Save()
        {
            Catalog.SaveWorks(m_works.Where(c => c.IsDownloading));
        }

        public void Remove(Chapter a_work)
        {
            m_works = m_works.Except(a_work).ToList();
        }

        internal void Add(Chapter a_chapter)
        {
            if (m_works.Contains(a_chapter))
                return;

            var copy = m_works.ToList();
            copy.Add(a_chapter);
            m_works = copy;
        }
    }
}
