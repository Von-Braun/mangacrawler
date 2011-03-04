
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace MangaCrawlerLib
{
    public class QueuedSemaphore
    {
        private Object m_lock = new Object();
        private Queue<ManualResetEvent> m_queue = new Queue<ManualResetEvent>();
        private int m_working = 0;
        private readonly int m_count;

        public QueuedSemaphore(int a_count)
        {
            m_count = a_count;
        }

        public void WaitOne(CancellationToken a_token)
        {
            ManualResetEvent mre = null;

            lock (m_lock)
            {
                if (m_working == m_count)
                {
                    mre = new ManualResetEvent(false);
                    m_queue.Enqueue(mre);
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
                            {
                                Release();
                            }
                            else if (m_queue.Contains(mre))
                            {
                                List<ManualResetEvent> list = new List<ManualResetEvent>();
                                while (m_queue.Peek() != mre)
                                    list.Add(m_queue.Dequeue());
                                m_queue.Dequeue();
                                while (list.Count != 0)
                                {
                                    m_queue.Enqueue(list.Last());
                                    list.RemoveLast();
                                }
                            }
                        }
                        a_token.ThrowIfCancellationRequested();
                    }
                }
            }
        }

        public void WaitOne()
        {
            ManualResetEvent mre = null;

            lock (m_lock)
            {
                if (m_working == m_count)
                {
                    mre = new ManualResetEvent(false);
                    m_queue.Enqueue(mre);
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
                    m_queue.Dequeue().Set();

                if (m_queue.Count < m_working)
                    m_working = m_queue.Count;
            }
        }
    }
}
