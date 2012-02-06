using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    public enum ItemState
    {
        Initial = 0,
        Waiting,
        Downloading,
        Deleting, 
        Zipping,
        Downloaded,
        DownloadedMissingPages,
        Error
    }
}
