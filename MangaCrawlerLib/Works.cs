using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    public class Works
    {
        private List<Chapter> m_works = new List<Chapter>();

        internal void Load()
        {
            IEnumerable<Chapter> works = from work in Catalog.LoadWorks()
                                         orderby work.LimiterOrder
                                         select work;
            DownloadManager.Instance.DownloadPages(works);
        }

        public IEnumerable<Chapter> List
        {
            get
            {
                lock (m_works)
                {
                    var aborted = from work in m_works
                                  where work.State == ChapterState.Aborted
                                  select work;

                    foreach (var el in aborted.ToArray())
                        m_works.Remove(el);

                    return m_works.ToArray();
                }
            }
        }  

        public void Save()
        {
            IEnumerable<Chapter> copy; 

            lock (m_works)
            {
                copy = m_works.ToArray();
            }

            copy = copy.Where(c => c.IsDownloading);

            Catalog.SaveWorks(copy);
        }

        public void Remove(Chapter a_work)
        {
            lock (m_works)
            {
                if (!m_works.Remove(a_work))
                    Loggers.MangaCrawler.WarnFormat("Chapter not in s_works: {0}", a_work);
            }
        }

        internal void Add(Chapter chapter)
        {
            lock (m_works)
            {
                if (m_works.Contains(chapter))
                    m_works.Remove(chapter);
                m_works.Add(chapter);
            }
        }
    }
}
