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
    public class SerieBookmarkListItem : ListItem
    {
        public Serie Serie { get; private set; } 

        public SerieBookmarkListItem(Serie a_serie)
        {
            Serie = a_serie;
        }

        public override string ToString()
        {
            return String.Format("{0} [{1}]", Serie.Title, Serie.Server.Name);
        }

        public override ulong ID
        {
            get
            {
                return Serie.ID;
            }
        }

        private void DrawCount(Graphics a_graphics, Rectangle a_rect, Font a_font)
        {
            a_graphics.DrawString(
                Serie.Chapters.Count.ToString(),
                a_font, Brushes.Green, a_rect, StringFormat.GenericDefault);
        }

        public override void DrawItem(DrawItemEventArgs a_args)
        {
            if (a_args.Index == -1)
                return;

            Action<Rectangle, Font> draw_tip = (rect, font) =>
            {
                switch (Serie.State)
                {
                    case SerieState.Error:
                    {
                        if (Serie.GetNewChapters().Any())
                        {
                            a_args.Graphics.DrawString(Resources.New, font,
                                Brushes.Red, rect, StringFormat.GenericDefault);
                        }
                        else
                        {
                            a_args.Graphics.DrawString(Resources.Error, font,
                                Brushes.Red, rect, StringFormat.GenericDefault);
                        }
                        break;
                    }
                    case SerieState.Downloaded:
                    {
                        if (Serie.GetNewChapters().Any())
                        {
                            a_args.Graphics.DrawString(Resources.New, font,
                                Brushes.Red, rect, StringFormat.GenericDefault);
                        }
                        else
                        {
                            if (Serie.Chapters.Count != 0)
                                DrawCount(a_args.Graphics, rect, font);
                        }
                        break;
                    }
                    case SerieState.Waiting:
                    {
                        a_args.Graphics.DrawString(Resources.Waiting, font,
                            Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case SerieState.Downloading:
                    {
                        a_args.Graphics.DrawString(
                            String.Format("({0}%)", Serie.DownloadProgress),
                            font, Brushes.Blue, rect, StringFormat.GenericDefault);

                        break;
                    }
                    case SerieState.Initial:
                    {
                        break;
                    }
                    default: 
                         
                         throw new NotImplementedException();
                }
            };

            DrawItem(a_args, draw_tip);
        }
    }
}
