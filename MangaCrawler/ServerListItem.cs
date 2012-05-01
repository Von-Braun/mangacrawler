using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using MangaCrawler.Properties;

namespace MangaCrawler
{
    public class ServerListItem : ListItem
    {
        public Server Server { get; private set; }

        public ServerListItem(Server a_server)
        {
            Server = a_server;
        }

        public override string ToString()
        {
            return Server.Name;
        }

        public override ulong ID
        {
            get
            {
                return Server.ID;
            }
        }

        private void DrawCount(Graphics a_graphics, Rectangle a_rect, Font a_font)
        {
            a_graphics.DrawString(Server.Series.Count.ToString(),
                a_font, Brushes.Green, a_rect, StringFormat.GenericDefault);
        }

        public override void DrawItem(DrawItemEventArgs a_args)
        {
            if (a_args.Index == -1)
                return;

            Action<Rectangle, Font> draw_tip = (rect, font) =>
            {
                switch (Server.State)
                {
                    case ServerState.Error:
                    {
                        a_args.Graphics.DrawString(Resources.Error, font,
                            Brushes.Red, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case ServerState.Downloaded:
                    {
                        DrawCount(a_args.Graphics, rect, font);
                        break;
                    }
                    case ServerState.Waiting:
                    {
                        a_args.Graphics.DrawString(Resources.Waiting, font,
                            Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case ServerState.Downloading:
                    {
                        a_args.Graphics.DrawString(
                            String.Format("({0}%)", Server.DownloadProgress),
                            font, Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;
                    }
                    case ServerState.Initial:
                    {
                        if (Server.Series.Count != 0)
                            DrawCount(a_args.Graphics, rect, font);
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
    }
}
