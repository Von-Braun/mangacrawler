using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    public enum ItemState
    {
        Initial,
        Waiting,
        Downloading,
        Deleting, 
        Zipping,
        Downloaded,
        Error
    }
}
