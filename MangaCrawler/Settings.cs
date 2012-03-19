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
using System.Xml.Linq;

namespace MangaCrawler
{
    public class Settings
    {
        private static string SETTINGS_XML = "settings.xml";
        private static string SETTINGS_DIR = "MangaCrawler";

        private static string VERSION = "1.2";

        [YAXAttributeForClass]
        private string Version;

        [YAXNode]
        public MangaSettings MangaSettings { get; private set; }

        [YAXNode("SeriesFilter")]
        private string m_series_filter = "";

        [YAXNode("SplitterDistance")]
        private int m_splitter_distance = 200;

        [YAXNode("SplitterBookmarksDistance")]
        private int m_splitter_bookmarks_distance = 300;

        [YAXNode]
        public FormState FormState = new FormState();

        [YAXNode("PlaySoundWhenDownloaded")]
        private bool m_play_sound_when_downloaded = true;

        private static Settings s_instance;

        static Settings()
        {
            try
            {
                s_instance = YAXSerializer.LoadFromFile<Settings>(SettingsFile);

                if (s_instance == null)
                    s_instance = new Settings();
            }
            catch
            {
                s_instance = new Settings();
            }
        }

        private static string SettingsFile
        {
            get
            {
                return GetSettingsDir() + SETTINGS_XML;
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
            YAXSerializer.SaveToFile<Settings>(SettingsFile, this);
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
            MangaSettings = new MangaSettings();
            MangaSettings.Changed += () => Save();
            Version = VERSION;
        }

        [YAXOnDeserialized]
        private void OnDeserialized()
        {
            FormState.Changed += () => Save();
            MangaSettings.Changed += () => Save();
        }

        public bool PlaySoundWhenDownloaded
        {
            get
            {
                return m_play_sound_when_downloaded;
            }
            set
            {
                m_play_sound_when_downloaded = value;
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

        public int SplitterBookmarksDistance
        {
            get
            {
                return m_splitter_bookmarks_distance;
            }
            set
            {
                m_splitter_bookmarks_distance = value;
                Save();
            }
        }
    }
}
