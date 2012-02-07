using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace MangaCrawler
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MangaCrawlerForm());
        }
    }
}
