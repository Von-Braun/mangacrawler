using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YAXLib;
using System.Reflection;
using System.IO;
using System.Drawing;
using MangaCrawlerLib;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    public class MangaSettings
    {
        [YAXNode("MangaRootDir")]
        private string m_manga_root_dir = Environment.GetFolderPath(
            Environment.SpecialFolder.DesktopDirectory) +
            Path.DirectorySeparatorChar + Application.ProductName;

        [YAXNode("UseCBZ")]
        private bool m_use_cbs = false;

        [YAXNode("CheckTimeDelta")]
        private TimeSpan m_check_time_delta = new TimeSpan(hours: 0, minutes: 1, seconds: 0);

        [YAXNode("MaxCatalogSize")]
        private int m_max_catalog_size = 100 * 1024 * 1024;

        [YAXNode("PageNamingStrategy")]
        private PageNamingStrategy m_page_naming_strategy = PageNamingStrategy.DoNothing;

        // Sync with MangaCrawler/app.config
        [YAXNode("MaximumConnections")]
        private int m_maximum_connections = 100;

        [YAXNode("MaximumConnectionsPerServer")]
        private int m_maximum_connections_per_server = 4;

        [YAXNode("UserAgent")]
        private string m_user_agent = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:10.0) Gecko/20100101 Firefox/10.0";

        public event Action Changed;

        public string GetMangaRootDir(bool a_remove_slash_on_end)
        {
            string result = m_manga_root_dir;

            if (a_remove_slash_on_end)
            {
                if (result.Last() == Path.DirectorySeparatorChar)
                    result = result.RemoveFromRight(1);
            }

            return result;
        }

        public void SetMangaRootDir(string a_manga_root_dir)
        {
            m_manga_root_dir = a_manga_root_dir;
            OnChanged();
        }

        private void OnChanged()
        {
            if (Changed != null)
                Changed();
        }

        public bool UseCBZ
        {
            get
            {
                return m_use_cbs;
            }
            set
            {
                m_use_cbs = value;
                OnChanged();
            }
        }

        public TimeSpan CheckTimeDelta
        {
            get
            {
                return m_check_time_delta;
            }
            private set
            {
                m_check_time_delta = value;
                OnChanged();
            }
        }

        public int MaxCatalogSize
        {
            get
            {
                return m_max_catalog_size;
            }
            private set
            {
                m_max_catalog_size = value;
                OnChanged();
            }
        }

        public PageNamingStrategy PageNamingStrategy
        {
            get
            {
                return m_page_naming_strategy;
            }
            set
            {
                m_page_naming_strategy = value;
                OnChanged();
            }
        }

        public int MaximumConnections
        {
            get
            {
                return m_maximum_connections;
            }
            private set
            {
                m_maximum_connections = value;
                OnChanged();
            }
        }

        public int MaximumConnectionsPerServer
        {
            get
            {
                return m_maximum_connections_per_server;
            }
            private set
            {
                m_maximum_connections_per_server = value;
                OnChanged();
            }
        }

        public string UserAgent
        {
            get
            {
                return m_user_agent;
            }
            private set
            {
                m_user_agent = value;
                OnChanged();
            }
        }
    }
}
