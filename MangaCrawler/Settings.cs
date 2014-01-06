using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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

        private string Version;

        public MangaSettings MangaSettings { get; private set; }

        private string m_series_filter = "";

        private int m_series_splitter_distance = 0;

        private int m_bookmarks_splitter_distance = 0;

        public FormState FormState = new FormState();

        private bool m_play_sound_when_downloaded = false;

        private bool m_minimize_on_close = false;

        private bool m_show_baloon_tips = false;

        private bool m_autostart = false;

        private TimeSpan m_check_bookmarks_period = new TimeSpan(hours: 0, minutes: 60, seconds: 0);

        private static Settings s_instance;

        private static string XML_SETTINGS = "Settings";
        private static string XML_VERSION = "Version";
        private static string XML_SERIESFILTER = "SeriesFilter";
        private static string XML_SERIESSPLITTERDISTANCE = "SeriesSplitterDistance";
        private static string XML_BOOKMARKSSPLITTERDISTANCE = "BookmarksSplitterDistance";
        private static string XML_FORMSTATE = "FormState";
        private static string XML_PLAYSOUNDWHENDOWNLOADED = "PlaySoundWhenDownloaded";
        private static string XML_MINIMIZEONCLOSE = "MinimizeOnClose";
        private static string XML_SHOWBALOONTIPS = "ShowBaloonTips";
        private static string XML_AUTOSTART = "Autostart";
        private static string XML_CHECKBOOKMARKSPERIOD = "CheckBookmarksPeriod";
        private static string XML_MANGASETTINGS = "MangaSettings";

        static Settings()
        {
            try
            {
                s_instance = Load(SettingsFile);
            }
            catch
            {
                s_instance = new Settings();
            }
        }

        private static Settings Load(string a_settings_file)
        {
            if (!File.Exists(a_settings_file))
                return new Settings();

            var root = XDocument.Load(a_settings_file).Root;

            if (root.Name != XML_SETTINGS)
                throw new Exception();

            Settings settings = new Settings() 
            {
                Version = root.Attribute(XML_VERSION).Value,
                SeriesFilter = root.Element(XML_SERIESFILTER).Value,
                SeriesSplitterDistance = Int32.Parse(root.Element(XML_SERIESSPLITTERDISTANCE).Value),
                BookmarksSplitterDistance = Int32.Parse(root.Element(XML_BOOKMARKSSPLITTERDISTANCE).Value),
                FormState = FormState.Load(root.Element(XML_FORMSTATE)),
                PlaySoundWhenDownloaded = Boolean.Parse(root.Element(XML_PLAYSOUNDWHENDOWNLOADED).Value),
                MinimizeOnClose = Boolean.Parse(root.Element(XML_MINIMIZEONCLOSE).Value),
                ShowBaloonTips = Boolean.Parse(root.Element(XML_SHOWBALOONTIPS).Value),
                Autostart = Boolean.Parse(root.Element(XML_AUTOSTART).Value),
                CheckBookmarksPeriod = TimeSpan.Parse(root.Element(XML_CHECKBOOKMARKSPERIOD).Value),
                MangaSettings = MangaSettings.Load(root.Element(XML_MANGASETTINGS))
            };

            settings.FormState.Changed += () => settings.Save();
            settings.MangaSettings.Changed += () => settings.Save();

            return settings;
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

            XElement root = new XElement(XML_SETTINGS, 
                new XAttribute(XML_VERSION, Version), 
                new XElement(XML_SERIESFILTER, SeriesFilter), 
                new XElement(XML_SERIESSPLITTERDISTANCE, SeriesSplitterDistance), 
                new XElement(XML_BOOKMARKSSPLITTERDISTANCE, BookmarksSplitterDistance),
                FormState.GetAsXml(), 
                new XElement(XML_PLAYSOUNDWHENDOWNLOADED, PlaySoundWhenDownloaded), 
                new XElement(XML_MINIMIZEONCLOSE, MinimizeOnClose), 
                new XElement(XML_SHOWBALOONTIPS, ShowBaloonTips), 
                new XElement(XML_AUTOSTART, Autostart), 
                new XElement(XML_CHECKBOOKMARKSPERIOD, CheckBookmarksPeriod.ToString("hh\\:mm\\:ss")), 
                MangaSettings.GetAsXml());

            root.Save(SettingsFile);
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

        public int SeriesSplitterDistance
        {
            get
            {
                return m_series_splitter_distance;
            }
            set
            {
                m_series_splitter_distance = value;
                Save();
            }
        }

        public int BookmarksSplitterDistance
        {
            get
            {
                return m_bookmarks_splitter_distance;
            }
            set
            {
                m_bookmarks_splitter_distance = value;
                Save();
            }
        }

        public bool MinimizeOnClose
        {
            get
            {
                return m_minimize_on_close;
            }
            set
            {
                m_minimize_on_close = value;
                Save();
            }
        }

        public bool ShowBaloonTips
        {
            get
            {
                return m_show_baloon_tips;
            }
            set
            {
                m_show_baloon_tips = value;
                Save();
            }
        }

        public bool Autostart
        {
            get
            {
                return m_autostart;
            }
            set
            {
                m_autostart = value;
                Save();
            }
        }

        public TimeSpan CheckBookmarksPeriod
        {
            get
            {
                return m_check_bookmarks_period;
            }
            set
            {
                m_check_bookmarks_period = value;
                Save();
            }
        }
    }
}
