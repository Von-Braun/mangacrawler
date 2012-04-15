using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;

namespace MangaCrawler
{
    public class DownloadingGridRow
    {
        public Chapter Chapter { get; set; }

        public DownloadingGridRow(Chapter a_chapter)
        {
            Chapter = a_chapter;
        }

        public override string ToString()
        {
            return Chapter.ToString();
        }

        public string Progress
        {
            get
            {
                switch (Chapter.State)
                {
                    case ChapterState.Error:
                        return MangaCrawler.Properties.Resources.DownloadingProgressError;
                    case ChapterState.Cancelled:
                        return MangaCrawler.Properties.Resources.DownloadingProgressCancelled;
                    case ChapterState.Waiting:
                        return MangaCrawler.Properties.Resources.DownloadingProgressWaiting;
                    case ChapterState.Cancelling:
                        return MangaCrawler.Properties.Resources.DownloadingProgressCancelling;
                    case ChapterState.Downloaded:
                        return MangaCrawler.Properties.Resources.DownloadingProgressDownloaded;
                    case ChapterState.Zipping:
                        return MangaCrawler.Properties.Resources.DownloadingProgressZipping;
                    case ChapterState.DownloadingPagesList:
                        return MangaCrawler.Properties.Resources.DownloadingProgressDownloading;
                    case ChapterState.DownloadingPages:
                        return String.Format("{0}/{1}", Chapter.PagesDownloaded, Chapter.Pages.Count);
                    default: throw new NotImplementedException();
                }
            }
        }

        public string Info
        {
            get
            {
                return String.Format(MangaCrawler.Properties.Resources.DownloadingChapterInfo,
                     Chapter.Server.Name, Chapter.Serie.Title, Chapter.Title);
            }
        }
    }
}
