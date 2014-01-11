using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Drawing;
using MangaCrawlerLib;
using TomanuExtensions;
using System.Xml.Linq;

namespace MangaCrawlerLib
{
    public class MangaSettings
    {
        private string m_manga_root_dir = Environment.GetFolderPath(
            Environment.SpecialFolder.DesktopDirectory) +
            Path.DirectorySeparatorChar + "Manga Crawler";

        private bool m_use_cbz = false;

        private bool m_delete_dir_with_images_when_cbz = false;

        private TimeSpan m_check_time_period = new TimeSpan(hours: 0, minutes: 15, seconds: 0);

        private PageNamingStrategy m_page_naming_strategy = PageNamingStrategy.DoNotChange;

        private bool m_pad_page_names_with_zeros;

        private string m_user_agent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:26.0) Gecko/20100101 Firefox/26.0";

        // Sync with MangaCrawler/app.config
        public int MaximumConnections { get; set; }

        public int MaximumConnectionsPerServer { get; set; }

        public int SleepAfterEachDownloadMS { get; set; }

        public event Action Changed;

        private static string XML_MANGASETTINGS = "MangaSettings";
        private static string XML_MANGAROOTDIR = "MangaRootDir";
        private static string XML_USECBZ = "UseCBZ";
        private static string XML_DELETEDIRWITHIMAGESWHENCBZ = "DeleteDirWithImagesWhenCBZ";
        private static string XML_CHECKTIMEPERIOD = "CheckTimePeriod";
        private static string XML_PAGENAMINGSTRATEGY = "PageNamingStrategy";
        private static string XML_PADPAGENAMESWITHZEROS = "PadPageNamesWithZeros";

        public MangaSettings()
        {
            MaximumConnections = 100;
            MaximumConnectionsPerServer = 4;
            SleepAfterEachDownloadMS = 0;
        }

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
                return m_use_cbz;
            }
            set
            {
                m_use_cbz = value;
                OnChanged();
            }
        }

        public bool DeleteDirWithImagesWhenCBZ
        {
            get
            {
                return m_delete_dir_with_images_when_cbz;
            }
            set
            {
                m_delete_dir_with_images_when_cbz = value;
                OnChanged();
            }
        }

        public TimeSpan CheckTimePeriod
        {
            get
            {
                return m_check_time_period;
            }
            private set
            {
                m_check_time_period = value;
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

        public bool PadPageNamesWithZeros
        {
            get
            {
                return m_pad_page_names_with_zeros;
            }
            set
            {
                m_pad_page_names_with_zeros = value;
                OnChanged();
            }
        }

        public string UserAgent
        {
            get
            {
                return m_user_agent;
            }
        }

        public bool IsMangaRootDirValid
        {
            get
            {
                try
                {
                    new DirectoryInfo(GetMangaRootDir(false));
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static MangaSettings Load(XElement a_node)
        {
            if (a_node.Name != XML_MANGASETTINGS)
                throw new Exception();

            return new MangaSettings()
            {
                m_manga_root_dir = a_node.Element(XML_MANGAROOTDIR).Value, 
                UseCBZ = Boolean.Parse(a_node.Element(XML_USECBZ).Value), 
                DeleteDirWithImagesWhenCBZ = Boolean.Parse(a_node.Element(XML_DELETEDIRWITHIMAGESWHENCBZ).Value), 
                CheckTimePeriod = TimeSpan.Parse(a_node.Element(XML_CHECKTIMEPERIOD).Value), 
                PageNamingStrategy = (PageNamingStrategy)Enum.Parse(
                    typeof(PageNamingStrategy), a_node.Element(XML_PAGENAMINGSTRATEGY).Value),
                PadPageNamesWithZeros = Boolean.Parse(a_node.Element(XML_PADPAGENAMESWITHZEROS).Value)
            };
        }

        public XElement GetAsXml()
        {
            return new XElement(XML_MANGASETTINGS,
                new XElement(XML_MANGAROOTDIR, m_manga_root_dir), 
                new XElement(XML_USECBZ, UseCBZ), 
                new XElement(XML_DELETEDIRWITHIMAGESWHENCBZ, DeleteDirWithImagesWhenCBZ), 
                new XElement(XML_CHECKTIMEPERIOD, CheckTimePeriod.ToString("hh\\:mm\\:ss")), 
                new XElement(XML_PAGENAMINGSTRATEGY, PageNamingStrategy), 
                new XElement(XML_PADPAGENAMESWITHZEROS, PadPageNamesWithZeros));
        }
    }
}
