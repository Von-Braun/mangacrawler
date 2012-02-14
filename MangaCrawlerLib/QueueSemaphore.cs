
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace MangaCrawlerLib
{
    internal class QueuedSemaphore<P>
    {
        private Object m_lock = new Object();
        private OrderedList<P, ManualResetEvent> m_queue =
            new OrderedList<P, ManualResetEvent>();
        private int m_working = 0;
        private int m_count;

        public QueuedSemaphore(int a_count)
        {
            m_count = a_count;
        }

        public bool Saturated
        {
            get
            {
                return m_working == m_count;
            }
        }

        public void WaitOne(P a_priority)
        {
            WaitOne(CancellationToken.None, a_priority);
        }

        public void WaitOne(CancellationToken a_token, P a_priority)
        {
            ManualResetEvent mre = null;

            lock (m_lock)
            {
                if (m_working == m_count)
                {
                    mre = new ManualResetEvent(false);
                    m_queue.Add(a_priority, mre);
                }
                else
                    m_working++;
            }

            if (mre != null)
            {
                Loggers.ConLimits.InfoFormat("waiting, {0} / {1}, queue: {2}", 
                    m_working, m_count, m_queue.Count);

                if (a_token != CancellationToken.None)
                {
                    do
                    {
                        if (a_token.IsCancellationRequested)
                        {
                            Loggers.ConLimits.InfoFormat("Cancellation requested");

                            lock (m_lock)
                            {
                                if (mre.WaitOne(0))
                                    Release();
                                else if (m_queue.Values.Contains(mre))
                                    m_queue.RemoveByValue(mre);
                            }

                            a_token.ThrowIfCancellationRequested();
                        }
                    }
                    while (!mre.WaitOne(100));
                }
                else
                    mre.WaitOne();

                mre.Close();
            }

            Loggers.ConLimits.InfoFormat("aquired, {0} / {1}, queue: {2}",
                m_working, m_count, m_queue.Count);
        }

        public void Release()
        {
            lock (m_lock)
            {
                if (m_queue.Count != 0)
                    m_queue.RemoveFirst().Set();

                if (m_queue.Count < m_working)
                    m_working = m_queue.Count;
            }
        }
    }
}
