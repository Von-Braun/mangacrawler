using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MangaCrawlerLib
{
    public class UpdateLimiter
    {
        private int m_freq;
        private int m_last;
        private object m_lock = new Object();
        private bool m_queued = false;

        public UpdateLimiter(int a_freq)
        {
            m_freq = a_freq;
        }

        public void Update(Action a_callback)
        {
            lock (m_lock)
            {
                if (System.Environment.TickCount - m_last < m_freq)
                {
                    if (!m_queued)
                    {
                        m_queued = true;
                        Queue(a_callback);
                    }
                }
                else
                {
                    m_queued = false;
                    m_last = System.Environment.TickCount;
                    a_callback();
                }
            }
        }

        private void Queue(Action a_callback)
        {
            int sleep = m_last + m_freq - System.Environment.TickCount;

            if (sleep > 0)
            {
                new Task(() =>
                {
                    Thread.Sleep(sleep);
                    Update(a_callback);
                }).Start();
            }
        }
    }

}
