using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;

namespace MangaCrawler
{
    public class WorkGridRow
    {
        public Work Work { get; private set; }

        public WorkGridRow(Work a_work)
        {
            Work = a_work;
        }

        public string Progress
        {
            get
            {
                var s = Work.State;

                switch (s)
                {
                    case WorkState.Error:
                    return MangaCrawler.Properties.Resources.WorkProgressError;
                    case WorkState.Aborted:
                    return MangaCrawler.Properties.Resources.WorkProgressAborted;
                    case WorkState.Waiting:
                    return MangaCrawler.Properties.Resources.WorkProgressWaiting;
                    case WorkState.Deleting:
                    return MangaCrawler.Properties.Resources.WorkProgressDeleting;
                    case WorkState.Downloaded:
                    return MangaCrawler.Properties.Resources.WorkProgressDownloaded;
                    case WorkState.Zipping:
                    return MangaCrawler.Properties.Resources.WorkProgressZipping;
                    case WorkState.Downloading:
                    return String.Format("{0}/{1}", Work.Chapter.DownloadedPages, Work.Chapter.Pages.Count());
                    default: throw new NotImplementedException();
                }
            }
        }

        public string Info
        {
            get
            {
                return String.Format(MangaCrawler.Properties.Resources.DownloadingChapterInfo,
                     Work.Chapter.Serie.Server.Name, Work.Chapter.Serie.Title, Work.Chapter.Title);
            }
        }
    }
}
