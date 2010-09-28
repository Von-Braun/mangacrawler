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
            }

            if (mre != null)
            {
                while (!mre.WaitOne(100))
                    a_token.ThrowIfCancellationRequested();
            }

            bool taken = false;
            m_spinLock.Enter(ref taken);
        }

        public void ReleaseMutex()
        {
            lock (m_lock)
            {
                m_spinLock.Exit();

                ManualResetEvent mre;
                if (!m_queue.TryDequeue(out mre))
                    throw new InvalidOperationException();

                mre.Set();
            }
        }
    }
}
