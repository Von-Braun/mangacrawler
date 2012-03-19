using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    public class Bookmarks
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Serie> m_bookmarks = new List<Serie>();

        public void Load()
        {
            m_bookmarks = Catalog.LoadBookmarks();
        }

        public void Save()
        {
            Catalog.SaveBookmarks();
        }

        public void Add(Serie a_serie)
        {
            var copy = m_bookmarks.ToList();
            copy.Add(a_serie);
            m_bookmarks = copy;

            foreach (var chapter in a_serie.Chapters)
                chapter.BookmarkIgnored = true;

            Save();
        }

        public IEnumerable<Serie> List
        {
            get
            {
                return m_bookmarks;
            }
        }

        public void Remove(Serie a_serie)
        {
            var copy = m_bookmarks.ToList();
            copy.Remove(a_serie);
            m_bookmarks = copy;

            Save();
        }
    }
}