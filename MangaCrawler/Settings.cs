using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YAXLib;
using System.Reflection;
using System.IO;

namespace MangaCrawler
{
    public class Settings
    {
        private static string SETTINGS_XML = "settings.xml";
        private static string SETTINGS_DIR = "MangaCrawler";

        private string m_images_base_dir =
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        private static Settings s_instance;

        static Settings()
        {
            try
            {
                s_instance = YAXSerializer.LoadFromFile<Settings>(GetSettingsDir() + SETTINGS_XML);
            }
            catch
            {
                s_instance = new Settings();
            }
        }

        public static string GetSettingsDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + 
                "\\" + SETTINGS_DIR + "\\";
        }

        public void Save()
        {
            Directory.CreateDirectory(GetSettingsDir());
            YAXSerializer.SaveToFile<Settings>(GetSettingsDir() + SETTINGS_XML, this);
        }

        public static Settings Instance 
        {
            get
            {
                return s_instance;
            }
        }

        protected Settings()
        {
        }

        [YAXNode]
        public string ImagesBaseDir
        {
            get
            {
                return m_images_base_dir;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    value = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                m_images_base_dir = value;
            }
        }

        [YAXNode]
        public string SeriesFilter = "";

        [YAXNode]
        public int SplitterDistance = 200;

        [YAXNode]
        public bool UseCBZ = false;
    }
}
