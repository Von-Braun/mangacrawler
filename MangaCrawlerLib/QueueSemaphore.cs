
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace MangaCrawlerLib
{
    public class QueuedSemaphore
    {
        private Object m_lock = new Object();
        private OrderedList<Priority, ManualResetEvent> m_queue =
            new OrderedList<Priority, ManualResetEvent>();
        private int m_working = 0;
        private readonly int m_count;

        public QueuedSemaphore(int a_count)
        {
            m_count = a_count;
        }

        public void WaitOne(CancellationToken a_token, Priority a_priority)
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
                while (!mre.WaitOne(100))
                {
                    if (a_token.IsCancellationRequested)
                    {
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
            }
        }

        public void WaitOne(Priority a_priority)
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
                mre.WaitOne();
        }

        public void Release()
        {
            lock (m_lock)
            {
                if (m_queue.Count != 0)
                {
                    m_queue.RemoveFirst().Set();
                }

                if (m_queue.Count < m_working)
                    m_working = m_queue.Count;
            }
        }
    }
}
