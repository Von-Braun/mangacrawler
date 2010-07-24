using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace MangaCrawler
{
    public class Config
    {
        private static Configuration s_config;

        public static Configuration Instance
        {
            get
            {
                if (s_config == null)
                {
                    s_config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                }

                return s_config;
            }
        }
    }
}
