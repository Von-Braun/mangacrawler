using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    public enum PageNamingStrategy
    {
        DoNothing, 
        PrefixWithIndexWhenNotOrdered, 
        PrefixWithIndex,
        OnlyIndex
    }
}
