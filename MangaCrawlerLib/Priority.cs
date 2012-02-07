using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    [Flags]
    internal enum Priority
    {
        Series = -2,
        Chapters = -1,
        Pages = 0,
        Image = 0,
        
    }
}
