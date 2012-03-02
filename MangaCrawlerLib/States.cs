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
        Aborted,
        Waiting,
        DownloadingPagesList,
        DownloadingPages,
        Deleting,
        Zipping,
        Downloaded,
        Error
    }

    public enum PageState
    {
        Initial, 
        WaitingForDownloading,
        WaitingForVerifing, 
        Veryfing,
        Veryfied, 
        Downloading, 
        Downloaded, 
        Error
    }
}
