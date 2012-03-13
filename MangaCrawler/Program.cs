﻿using System;
using System.Linq;
using System.Windows.Forms;
using MangaCrawlerLib;
using log4net.Appender;
using log4net.Config;
using log4net.Core;

namespace MangaCrawler
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MangaCrawlerForm());
        }
    }
}
