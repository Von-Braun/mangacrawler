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
        private SpinLock m_spinLock = new SpinLock();
        private ConcurrentQueue<ManualResetEvent> m_queue = new ConcurrentQueue<ManualResetEvent>();

        public void WaitOne(CancellationToken a_token)
        {
            ManualResetEvent mre = null;

            lock (m_lock)
            {
                if (m_spinLock.IsHeld && !m_spinLock.IsHeldByCurrentThread)
                {
                    mre = new ManualResetEvent(false);
                    m_queue.Enqueue(mre);
                }
                else
                {
                    bool taken = false;
                    m_spinLock.Enter(ref taken);
                }
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
                m_spinLock.Exit();

                ManualResetEvent mre;
                if (m_queue.TryDequeue(out mre))
                    mre.Set();
            }
        }
    }
}
