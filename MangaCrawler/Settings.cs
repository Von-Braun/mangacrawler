using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Windows.Forms;

namespace MangaCrawler
{
    public class Settings : ConfigurationSection
    {
        private static String SECTION_NAME = "Settings";
        private static Settings s_instance;

        static Settings()
        {
            s_instance = Config.Instance.GetSection(SECTION_NAME) as Settings;
   
            if (s_instance == null)
            {
                s_instance = new Settings();
                s_instance.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
                Config.Instance.Sections.Add(SECTION_NAME, s_instance);
            }  
        }

        public static Settings Instance 
        {
            get
            {
                return s_instance;
            }
        }

        [ConfigurationProperty("directoryPath", DefaultValue = "", IsRequired = false)]
        public string DirectoryPath
        {
            get
            {
                string str = (string)base["directoryPath"];

                if (String.IsNullOrWhiteSpace(str))
                {
                    str = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
                    DirectoryPath = str;
                }

                return str;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    value = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);

                base["directoryPath"] = value;
                Config.Instance.Save();
            }
        }

        [ConfigurationProperty("seriesFilter", DefaultValue = "", IsRequired = false)]
        public string SeriesFilter
        {
            get
            {
                return (string)base["seriesFilter"];
            }
            set
            {
                base["seriesFilter"] = value;
                Config.Instance.Save();
            }
        }

        [ConfigurationProperty("splitterDistance", DefaultValue = 200, IsRequired = false)]
        public int SplitterDistance
        {
            get
            {
                return (int)base["splitterDistance"];
            }
            set
            {
                base["splitterDistance"] = value;
                Config.Instance.Save();
            }
        }

        [ConfigurationProperty("useCBZ", DefaultValue = false, IsRequired = false)]
        public bool UseCBZ
        {
            get
            {
                return (bool)base["useCBZ"];
            }
            set
            {
                base["useCBZ"] = value;
                Config.Instance.Save();
            }
        }
    }
}
