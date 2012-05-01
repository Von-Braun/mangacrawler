using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;
using System.Windows.Forms;
using System.Drawing;
using MangaCrawler.Properties;

namespace MangaCrawler
{
    public class ChapterBookmarkListItem : ListItem
    {
        public Chapter Chapter { get; private set; }

        public ChapterBookmarkListItem(Chapter a_chapter)
        {
            Chapter = a_chapter;
        }

        public override string ToString()
        {
            return Chapter.Title;
        }

        public override void DrawItem(DrawItemEventArgs a_args)
        {
            Action<Rectangle, Font> draw_tip = (rect, font) =>
            {
                switch (Chapter.State)
                {
                    case ChapterState.Error:
                    {
                        a_args.Graphics.DrawString(AlsoNew() + Resources.Error, font,
                            Brushes.Red, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case ChapterState.Cancelled:
                    {
                        a_args.Graphics.DrawString(AlsoNew() + Resources.Cancelled, font,
                            Brushes.Red, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case ChapterState.Downloaded:
                    {
                        a_args.Graphics.DrawString(Resources.Downloaded, font,
                            Brushes.Green, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case ChapterState.Waiting:
                    {
                        a_args.Graphics.DrawString(Resources.Waiting, font,
                            Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case ChapterState.Cancelling:
                    {
                        a_args.Graphics.DrawString(Resources.Cancelling, font,
                            Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case ChapterState.DownloadingPagesList:
                    {
                        a_args.Graphics.DrawString(Resources.Downloading, font,
                            Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case ChapterState.DownloadingPages:
                    {
                        a_args.Graphics.DrawString(
                            String.Format("{0}/{1}", Chapter.PagesDownloaded, Chapter.Pages.Count), 
                            font, Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case ChapterState.Zipping:
                    {
                        a_args.Graphics.DrawString(Resources.Zipping, font,
                            Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case ChapterState.Initial:
                    {
                        if (!Chapter.Visited)
                        {
                            a_args.Graphics.DrawString(Resources.New, font,
                                Brushes.Red, rect, StringFormat.GenericDefault);
                        }
                        break;
                    }
                    default:
                    {
                        throw new NotImplementedException();
                    }
                }
            };

            DrawItem(a_args, draw_tip);
        }

        private string AlsoNew()
        {
            if (Chapter.Visited)
                return "";
            else
                return Resources.New + ", ";
        }

        public override ulong ID
        {
            get 
            {
                return Chapter.ID;
            }
        }
    }
}
