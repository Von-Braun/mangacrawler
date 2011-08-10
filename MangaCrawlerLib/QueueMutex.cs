
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    internal class QueuedMutex
    {
        private Object m_lock = new Object();
        private Queue<ManualResetEvent> m_queue = new Queue<ManualResetEvent>();
        private bool m_firstGo = false;

        public void WaitOne(CancellationToken a_token)
        {
            ManualResetEvent mre = null;

            lock (m_lock)
            {
                if (m_firstGo)
                {
                    mre = new ManualResetEvent(false);
                    m_queue.Enqueue(mre);
                }
                else
                    m_firstGo = true;
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
                                ReleaseMutex();
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

                mre.Close();
            }
        }

        public void ReleaseMutex()
        {
            lock (m_lock)
            {
                if (m_queue.Count != 0)
                    m_queue.Dequeue().Set();

                if (m_queue.Count == 0)
                    m_firstGo = false;
            }
        }
    }
}
