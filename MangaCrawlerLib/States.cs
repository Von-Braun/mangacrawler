using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    public enum ServerState
    {
        Initial,
        Waiting,
        Downloading,
        Downloaded,
        Error
    }

    public enum SerieState
    {
        Initial,
        Waiting,
        Downloading,
        Downloaded,
        Error
    }

    public enum ChapterState
    {
        Initial,
        WasDownloaded,
        Aborted,
        Waiting,
        Downloading,
        Deleting,
        Zipping,
        Downloaded,
        Error
    }

    public enum WorkState
    {
        Aborted,
        Waiting,
        Downloading,
        Deleting,
        Zipping,
        Downloaded,
        Error
    }
}
