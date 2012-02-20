using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;

namespace MangaCrawler
{
    public class ChapterListItem
    {
        public Chapter Chapter { get; private set; }

        public ChapterListItem(Chapter a_chapter)
        {
            Chapter = a_chapter;
        }

        public override string ToString()
        {
            return Chapter.Title;
        }
    }
}
