using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MangaCrawlerLib
{
    public class RestrictedFrequencyAction
    {
        private TimeSpan m_update_delta;
        private DateTime LastPerform;
        private volatile bool m_scheduled;
        private Object m_lock = new Object();
        private volatile Action m_action;

        public RestrictedFrequencyAction(int a_update_delta_ms)
        {
            m_update_delta = new TimeSpan(0, 0, 0, 0, a_update_delta_ms);
            LastPerform = DateTime.Now - m_update_delta - m_update_delta;
        }

        public void Perform(Action a_action)
        {
            int t = (int)(m_update_delta - (DateTime.Now - LastPerform)).TotalMilliseconds;

            if (t < 0)
            {
                lock (m_lock)
                {
                    LastPerform = DateTime.Now;
                    a_action();
                }
            }
            else
            {
                lock (m_lock)
                {
                    m_action = a_action;

                    if (!m_scheduled)
                    {
                        m_scheduled = true;

                        new Task(() =>
                        {
                            Thread.Sleep(t);
                            lock (m_lock)
                            {
                                LastPerform = DateTime.Now;
                                m_scheduled = false;
                                m_action();
                            }

                        }).Start();
                    }
                }
            }
        }
    }
}
