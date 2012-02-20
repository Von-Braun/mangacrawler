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

        public override bool Equals(object a_obj)
        {
            if (a_obj == null)
                return false;
            ChapterListItem cli = a_obj as ChapterListItem;
            if (cli == null)
                return false;
            return Chapter.Equals(cli.Chapter);
        }

        public override int GetHashCode()
        {
            return Chapter.GetHashCode();
        }
    }
}
