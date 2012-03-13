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
        Checking,
        Checked,
        Error
    }

    public enum SerieState
    {
        Initial,
        Waiting,
        Checking,
        Checked,
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
        Waiting,
        Downloading, 
        Downloaded, 
        Error
    }
}
