using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MangaCrawlerLib;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MangaCrawlerLib
{
    [DebuggerDisplay("ServerState, {ToString()}")]
    public class ServerState
    {
        private ServerInfo m_server_info;
        private Object m_lock = new Object();
        private int m_progress;
        private ItemState m_state;
        private CustomTaskScheduler m_scheduler;

        public ServerState(ServerInfo a_server_info)
        {
            m_server_info = a_server_info;
            Initialize();
        }

        public int Progress
        {
            get
            {
                return m_progress;
            }
            set
            {
                lock (m_lock)
                {
                    m_progress = value;
                }
            }
        }

        public void Initialize()
        {
            lock (m_lock)
            {
                m_progress = 0;
                m_state = ItemState.Initial;
            }
        }

        public override string ToString()
        {
            lock (m_lock)
            {
                return String.Format("name: {0}, state: {1}", m_server_info.Name, m_state);
            }
        }

        public bool DownloadRequired
        {
            get
            {
                lock (m_lock)
                {
                    return (m_state == ItemState.Error) || (m_state == ItemState.Initial);
                }
            }
        }


        internal CustomTaskScheduler Scheduler
        {
            get
            {
                if (m_scheduler == null)
                    m_scheduler = new CustomTaskScheduler(m_server_info.Crawler.MaxConnectionsPerServer);
                return m_scheduler;
            }
        }

        // TODO: ServerState
        public ItemState State
        {
            get
            {
                return m_state;
            }
            set
            {
                lock (m_lock)
                {
                    m_state = value;
                }
            }
        }
    }
}
