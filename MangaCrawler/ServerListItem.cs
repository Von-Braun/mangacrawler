using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;

namespace MangaCrawler
{
    public class ServerListItem
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
    }
}
