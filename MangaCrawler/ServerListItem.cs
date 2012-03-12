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
    public class ServerListItem : ListItem<Server>
    {
        public Server Server
        {
            get
            {
                return m_entity;
            }
        }

        public ServerListItem(Server a_server)
            : base(a_server)
        {
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

        public override void DrawItem(DrawItemEventArgs a_args)
        {
            if (a_args.Index == -1)
                return;

            Action<Rectangle, Font> draw_tip = (rect, font) =>
            {
                switch (Server.State)
                {
                    case ServerState.Error:

                        a_args.Graphics.DrawString(Resources.Error, font,
                            Brushes.Red, rect, StringFormat.GenericDefault);
                        break;

                    case ServerState.Downloaded:

                        a_args.Graphics.DrawString(
                            String.Format(Resources.Series, Server.Series.Count),
                            font, Brushes.Green, rect, StringFormat.GenericDefault);
                        break;

                    case ServerState.Waiting:

                        a_args.Graphics.DrawString(Resources.Waiting, font,
                            Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;

                    case ServerState.Downloading:

                        a_args.Graphics.DrawString(
                            String.Format("({0}%)", Server.DownloadProgress),
                            font, Brushes.Blue, rect, StringFormat.GenericDefault);
                        break;

                    case ServerState.Initial: break;

                      default: throw new NotImplementedException();
                }
            };

            DrawItem(a_args, Server.Name, draw_tip);
        }
    }
}
