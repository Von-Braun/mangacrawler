using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace MangaCrawlerLib
{
    public class QueuedMutex
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
                    a_token.ThrowIfCancellationRequested();
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
