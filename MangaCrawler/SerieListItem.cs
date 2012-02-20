using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;

namespace MangaCrawler
{
    public class SerieListItem
    {
        public Serie Serie { get; private set; }

        public SerieListItem(Serie a_serie)
        {
            Serie = a_serie;
        }

        public override string ToString()
        {
            return Serie.Title;
        }

        public override bool Equals(object a_obj)
        {
            if (a_obj == null)
                return false;
            SerieListItem sli = a_obj as SerieListItem;
            if (sli == null)
                return false;
            return Serie.Equals(sli.Serie);
        }

        public override int GetHashCode()
        {
            return Serie.GetHashCode();
        }
    }
}
