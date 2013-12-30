using System;
using System.Linq;
using System.Windows.Forms;
using MangaCrawlerLib;
using log4net.Appender;
using System.Threading;
using System.Security.AccessControl;
using System.Diagnostics;

namespace MangaCrawler
{
    static class Program
    {
        private static string SETUP_MUTEX_NAME = "Manga Crawler 5324532532";
        private static string EVENT_NAME = "Manga Crawler 4354634654";
        private static Mutex s_setup_mutex;
        public static EventWaitHandle RestoreEvent;

        [STAThread]
        static void Main()
        {
            if (!OnlyOneCopy())
                return;

            log4net.Config.XmlConfigurator.Configure();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MangaCrawlerForm());
        }

        private static bool OnlyOneCopy()
        {
            bool isnew;
            s_setup_mutex = new Mutex(true, SETUP_MUTEX_NAME, out isnew);

            RestoreEvent = null;
            try
            {
#if RELEASE
                RestoreEvent = AutoResetEvent.OpenExisting(EVENT_NAME);
                RestoreEvent.Set();
                return false;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
#endif
                string user = Environment.UserDomainName + "\\" + Environment.UserName;
                EventWaitHandleSecurity evh_sec = new EventWaitHandleSecurity();

                EventWaitHandleAccessRule rule = new EventWaitHandleAccessRule(user,
                    EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify,
                    AccessControlType.Allow);
                evh_sec.AddAccessRule(rule);

                bool was_created;
                RestoreEvent = new EventWaitHandle(false, EventResetMode.AutoReset, 
                    EVENT_NAME, out was_created, evh_sec);
            }
            catch (Exception)
            {
            }

            return true;
        }
    }
}
