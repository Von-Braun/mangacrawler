using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Reflection;

namespace MangaCrawler
{
    public class Autostart
    {
        private const string RUN_LOCATION = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private static readonly string KEY_VALUE = Assembly.GetExecutingAssembly().Location + " " + MINIMIZE_ARGUMENT;
        private static readonly string KEY_NAME = Application.ProductName;
        public const string MINIMIZE_ARGUMENT = "minimized";

        public static void Enable()
        {
            try
            {
                Registry.CurrentUser.CreateSubKey(RUN_LOCATION).SetValue(KEY_NAME, KEY_VALUE);
            }
            catch
            {
            }
        }
        
        public static bool Enabled
        {
            get
            {
                try
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_LOCATION);
                    if (key == null)
                        return false;

                    string value = (string)key.GetValue(KEY_NAME);
                    if (value == null)
                        return false;

                    return (value == KEY_VALUE);
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void Disable()
        {
            try
            {
                Registry.CurrentUser.CreateSubKey(RUN_LOCATION).DeleteValue(KEY_NAME, false);
            }
            catch
            {
            }
        }
    }
}
