using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;

namespace MangaCrawler
{
    public class WorkGridRow
    {
        public Chapter Chapter { get; private set; }

        public WorkGridRow(Chapter a_chapter)
        {
            Chapter = a_chapter;
        }

        public string Progress
        {
            get
            {
                return NH.TransactionLockWithResult(Chapter, () =>
                {
                    switch (Chapter.State)
                    {
                        case ChapterState.Error:
                            return MangaCrawler.Properties.Resources.WorkProgressError;
                        case ChapterState.Aborted:
                            return MangaCrawler.Properties.Resources.WorkProgressAborted;
                        case ChapterState.Waiting:
                            return MangaCrawler.Properties.Resources.WorkProgressWaiting;
                        case ChapterState.Deleting:
                            return MangaCrawler.Properties.Resources.WorkProgressDeleting;
                        case ChapterState.Downloaded:
                            return MangaCrawler.Properties.Resources.WorkProgressDownloaded;
                        case ChapterState.Zipping:
                            return MangaCrawler.Properties.Resources.WorkProgressZipping;
                        case ChapterState.DownloadingPagesList:
                            return MangaCrawler.Properties.Resources.WorkProgressDownloading;
                        case ChapterState.DownloadingPages:
                            return String.Format("{0}/{1}", Chapter.PagesDownloaded, Chapter.GetPages().Count());
                        default: throw new NotImplementedException();
                    }
                });
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
