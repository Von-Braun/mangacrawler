using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;
using System.Diagnostics;

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

        public override bool Equals(object a_obj)
        {
            if (a_obj == null)
                return false;
            ServerListItem sli = a_obj as ServerListItem;
            if (sli == null)
                return false;
            return Server.Equals(sli.Server);
        }

        public override int GetHashCode()
        {
            return Server.GetHashCode();
        }
    }
}
