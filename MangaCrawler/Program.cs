using System;
using System.Linq;
using System.Windows.Forms;
using MangaCrawlerLib;

namespace MangaCrawler
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MangaCrawlerForm());
        }
    }
}
