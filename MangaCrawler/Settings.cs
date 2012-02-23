using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YAXLib;
using System.Reflection;
using System.IO;
using System.Drawing;

namespace MangaCrawler
{
    public class Settings
    {
        private static string SETTINGS_XML = "settings.xml";
        private static string SETTINGS_DIR = "MangaCrawler";

        [YAXNode("MangaRootDir")]
        private string m_manga_root_dir = Environment.GetFolderPath(
            Environment.SpecialFolder.DesktopDirectory) +
            Path.DirectorySeparatorChar + Application.ProductName;

        [YAXNode("SeriesFilter")]
        private string m_series_filter = "";

        [YAXNode("SplitterDistance")]
        private int m_splitter_distance = 200;

        [YAXNode("UseCBZ")]
        private bool m_use_cbs = false;

        [YAXNode]
        public FormState FormState = new FormState();

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
                Path.DirectorySeparatorChar + SETTINGS_DIR + Path.DirectorySeparatorChar;
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
            FormState.Changed += () => Save();
        }

        [YAXOnDeserialized]
        private void OnDeserialized()
        {
            FormState.Changed += () => Save();
        }

        public string MangaRootDir
        {
            get
            {
                return m_manga_root_dir;
            }
            set
            {
                m_manga_root_dir = value;
                Save();
            }
        }

        public string SeriesFilter
        {
            get
            {
                return m_series_filter;
            }
            set
            {
                m_series_filter = value;
                Save();
            }
        }

        public int SplitterDistance
        {
            get
            {
                return m_splitter_distance;
            }
            set
            {
                m_splitter_distance = value;
                Save();
            }
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
                Save();
            }
        }
    }
}
