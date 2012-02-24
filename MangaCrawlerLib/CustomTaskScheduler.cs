
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace MangaCrawlerLib
{
    public class CustomTaskScheduler
    {
        /// <summary>
        /// Provides control over number of threads and priorities. This is what we need.
        /// </summary>
        [DebuggerTypeProxy(typeof(CustomTaskSchedulerDebugView))]
        [DebuggerDisplay("Id={Id}, Queues={DebugQueueCount}, ScheduledTasks = {DebugTaskCount}")]
        internal class InnerCustomTaskScheduler : TaskScheduler, IDisposable
        {
            /// <summary>Debug view for the CustomTaskScheduler.</summary>
            private class CustomTaskSchedulerDebugView
            {
                /// <summary>The scheduler.</summary>
                private InnerCustomTaskScheduler _scheduler;

                /// <summary>Initializes the debug view.</summary>
                /// <param name="scheduler">The scheduler.</param>
                public CustomTaskSchedulerDebugView(InnerCustomTaskScheduler scheduler)
                {
                    if (scheduler == null) throw new ArgumentNullException("scheduler");
                    _scheduler = scheduler;
                }

                /// <summary>Gets all of the Tasks queued to the scheduler directly.</summary>
                public IEnumerable<Task> ScheduledTasks
                {
                    get
                    {
                        var tasks = (IEnumerable<Task>)_scheduler.m_blocking_task_queue;
                        return tasks.Where(t => t != null).ToList();
                    }
                }

                /// <summary>Gets the prioritized and fair queues.</summary>
                public IEnumerable<TaskScheduler> Queues
                {
                    get
                    {
                        List<TaskScheduler> queues = new List<TaskScheduler>();
                        foreach (var group in _scheduler.m_queue_groups) queues.AddRange(group.Value);
                        return queues;
                    }
                }
            }

            /// <summary>
            /// A sorted list of queue lists.  Tasks with the smallest priority value
            /// are preferred.
            /// </summary>
            private readonly SortedList<int, QueueGroup> m_queue_groups =
                new SortedList<int, QueueGroup>();

            /// <summary>Cancellation token used for disposal.</summary>
            private readonly CancellationTokenSource m_dispose_cancellation =
                new CancellationTokenSource();

            /// <summary>
            /// The maximum allowed concurrency level of this scheduler.  If custom threads are
            /// used, this represents the number of created threads.
            /// </summary>
            private readonly int m_concurrency_level;

            /// <summary>Whether we're processing tasks on the current thread.</summary>
            private static ThreadLocal<bool> m_task_processing_thread = new ThreadLocal<bool>();

            /// <summary>The threads used by the scheduler to process work.</summary>
            private readonly Thread[] m_threads;

            /// <summary>The collection of tasks to be executed on our custom threads.</summary>
            private readonly BlockingCollection<Task> m_blocking_task_queue;

            /// <summary>Initializes the scheduler.</summary>
            /// <param name="threadCount">The number of threads to create and use for processing work items.</param>
            public InnerCustomTaskScheduler(int threadCount)
                : this(threadCount, string.Empty)
            {
            }

            /// <summary>Initializes the scheduler.</summary>
            /// <param name="threadCount">The number of threads to create and use for processing work items.</param>
            /// <param name="threadName">The name to use for each of the created threads.</param>
            public InnerCustomTaskScheduler(
                int threadCount,
                string threadName = "")
            {
                // Validates arguments (some validation is left up to the Thread type itself).
                // If the thread count is 0, default to the number of logical processors.
                if (threadCount < 0) 
                    throw new ArgumentOutOfRangeException("concurrencyLevel");
                else if (threadCount == 0) 
                    m_concurrency_level = Environment.ProcessorCount;
                else 
                    m_concurrency_level = threadCount;

                // Initialize the queue used for storing tasks
                m_blocking_task_queue = new BlockingCollection<Task>();

                // Create all of the threads
                m_threads = new Thread[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    m_threads[i] = new Thread(() => ThreadBasedDispatchLoop())
                    {
                        IsBackground = true,
                    };

                    if (threadName != null) 
                        m_threads[i].Name = threadName + " (" + i + ")";
                }

                // Start all of the threads
                foreach (var thread in m_threads) 
                    thread.Start();
            }

            /// <summary>The dispatch loop run by all threads in this scheduler.</summary>
            private void ThreadBasedDispatchLoop()
            {
                m_task_processing_thread.Value = true;

                try
                {
                    // If the scheduler is disposed, the cancellation token will be set and
                    // we'll receive an OperationCanceledException.  That OCE should not crash the process.
                    try
                    {
                        // If a thread abort occurs, we'll try to reset it and continue running.
                        while (true)
                        {
                            try
                            {
                                // For each task queued to the scheduler, try to execute it.
                                foreach (var task in m_blocking_task_queue.GetConsumingEnumerable(m_dispose_cancellation.Token))
                                {
                                    // If the task is not null, that means it was queued to this scheduler directly.
                                    // Run it.
                                    if (task != null)
                                    {
                                        TryExecuteTask(task);
                                    }
                                    // If the task is null, that means it's just a placeholder for a task
                                    // queued to one of the subschedulers.  Find the next task based on
                                    // priority and fairness and run it.
                                    else
                                    {
                                        // Find the next task based on our ordering rules...
                                        Task targetTask;
                                        CustomTaskSchedulerQueue queueForTargetTask;
                                        lock (m_queue_groups)
                                            FindNextTask_NeedsLock(out targetTask, out queueForTargetTask);

                                        // ... and if we found one, run it
                                        if (targetTask != null)
                                            queueForTargetTask.ExecuteTask(targetTask);
                                    }
                                }
                            }
                            catch (ThreadAbortException)
                            {
                                // If we received a thread abort, and that thread abort was due to shutting down
                                // or unloading, let it pass through.  Otherwise, reset the abort so we can
                                // continue processing work items.
                                if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                                {
                                    Thread.ResetAbort();
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException) { }
                }
                finally
                {
                    // Run a cleanup routine if there was one
                    m_task_processing_thread.Value = false;
                }
            }

            /// <summary>Gets the number of queues currently activated.</summary>
            private int DebugQueueCount
            {
                get
                {
                    int count = 0;
                    foreach (var group in m_queue_groups)
                        count += group.Value.Count;
                    return count;
                }
            }

            /// <summary>Gets the number of tasks currently scheduled.</summary>
            private int DebugTaskCount
            {
                get
                {
                    return ((IEnumerable<Task>)m_blocking_task_queue).Where(t => t != null).Count();
                }
            }

            /// <summary>Creates and activates a new scheduling queue for this scheduler.</summary>
            /// <param name="priority">The priority level for the new queue.</param>
            /// <returns>The newly created and activated queue at the specified priority.</returns>
            public TaskScheduler ActivateNewQueue(int priority)
            {
                // Create the queue
                var createdQueue = new CustomTaskSchedulerQueue(priority, this);

                // Add the queue to the appropriate queue group based on priority
                lock (m_queue_groups)
                {
                    QueueGroup list;
                    if (!m_queue_groups.TryGetValue(priority, out list))
                    {
                        list = new QueueGroup();
                        m_queue_groups.Add(priority, list);
                    }
                    list.Add(createdQueue);
                }

                // Hand the new queue back
                return createdQueue;
            }

            private Dictionary<int, TaskScheduler> m_schedulers = new Dictionary<int, TaskScheduler>();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="a_priority">
            /// Higher priority for lower a_priority.
            /// </param>
            /// <returns></returns>
            public TaskScheduler Scheduler(Priority a_priority)
            {
                TaskScheduler sch;
                if (m_schedulers.TryGetValue((int)a_priority, out sch))
                    return sch;

                m_schedulers.Add((int)a_priority, ActivateNewQueue((int)a_priority));

                return m_schedulers[(int)a_priority];
            }

            /// <summary>Find the next task that should be executed, based on priorities and fairness and the like.</summary>
            /// <param name="targetTask">The found task, or null if none was found.</param>
            /// <param name="queueForTargetTask">
            /// The scheduler associated with the found task.  Due to security checks inside of TPL,  
            /// this scheduler needs to be used to execute that task.
            /// </param>
            private void FindNextTask_NeedsLock(out Task targetTask, out CustomTaskSchedulerQueue queueForTargetTask)
            {
                targetTask = null;
                queueForTargetTask = null;

                // Look through each of our queue groups in sorted order.
                // This ordering is based on the priority of the queues.
                foreach (var queueGroup in m_queue_groups)
                {
                    var queues = queueGroup.Value;

                    // Within each group, iterate through the queues.
                    for (int i = 0; i < queues.Count; i++)
                    {
                        queueForTargetTask = queues[i];
                        var items = queueForTargetTask._workItems;
                        if (items.Count > 0)
                        {
                            targetTask = items.Dequeue();
                            if (queueForTargetTask._disposed && items.Count == 0)
                            {
                                RemoveQueue_NeedsLock(queueForTargetTask);
                            }
                            return;
                        }
                    }
                }
            }

            /// <summary>Queues a task to the scheduler.</summary>
            /// <param name="task">The task to be queued.</param>
            protected override void QueueTask(Task task)
            {
                // If we've been disposed, no one should be queueing
                if (m_dispose_cancellation.IsCancellationRequested)
                    throw new ObjectDisposedException(GetType().Name);

                // If the target scheduler is null (meaning we're using our own threads),
                // add the task to the blocking queue
                m_blocking_task_queue.Add(task);
            }

            /// <summary>Notifies the pool that there's a new item to be executed in one of the 
            /// queues.</summary>
            private void NotifyNewWorkItem() { QueueTask(null); }

            /// <summary>Tries to execute a task synchronously on the current thread.</summary>
            /// <param name="task">The task to execute.</param>
            /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
            /// <returns>true if the task was executed; otherwise, false.</returns>
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                // If we're already running tasks on this threads, enable inlining
                return m_task_processing_thread.Value && TryExecuteTask(task);
            }

            /// <summary>Gets the tasks scheduled to this scheduler.</summary>
            /// <returns>An enumerable of all tasks queued to this scheduler.</returns>
            /// <remarks>This does not include the tasks on sub-schedulers.  Those will be retrieved by the debugger separately.</remarks>
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                // Get all of the tasks, filtering out nulls, which are just placeholders
                // for tasks in other sub-schedulers
                return m_blocking_task_queue.Where(t => t != null).ToList();

            }

            /// <summary>Gets the maximum concurrency level to use when processing tasks.</summary>
            public override int MaximumConcurrencyLevel { get { return m_concurrency_level; } }

            /// <summary>Initiates shutdown of the scheduler.</summary>
            public void Dispose()
            {
                m_dispose_cancellation.Cancel();
            }

            /// <summary>Removes a scheduler from the group.</summary>
            /// <param name="queue">The scheduler to be removed.</param>
            private void RemoveQueue_NeedsLock(CustomTaskSchedulerQueue queue)
            {
                // Find the group that contains the queue and the queue's index within the group
                var queueGroup = m_queue_groups[queue._priority];
                int index = queueGroup.IndexOf(queue);

                // Remove it
                queueGroup.RemoveAt(index);
            }

            /// <summary>A group of queues a the same priority level.</summary>
            private class QueueGroup : List<CustomTaskSchedulerQueue>
            {
            }

            /// <summary>Provides a scheduling queue associatd with a CustomTaskScheduler.</summary>
            [DebuggerDisplay("QueuePriority = {_priority}, WaitingTasks = {WaitingTasks}")]
            [DebuggerTypeProxy(typeof(CustomTaskSchedulerQueueDebugView))]
            private sealed class CustomTaskSchedulerQueue : TaskScheduler, IDisposable
            {
                /// <summary>A debug view for the queue.</summary>
                private sealed class CustomTaskSchedulerQueueDebugView
                {
                    /// <summary>The queue.</summary>
                    private readonly CustomTaskSchedulerQueue _queue;

                    /// <summary>Initializes the debug view.</summary>
                    /// <param name="queue">The queue to be debugged.</param>
                    public CustomTaskSchedulerQueueDebugView(CustomTaskSchedulerQueue queue)
                    {
                        if (queue == null) throw new ArgumentNullException("queue");
                        _queue = queue;
                    }

                    /// <summary>Gets the priority of this queue in its associated scheduler.</summary>
                    public int Priority { get { return _queue._priority; } }
                    /// <summary>Gets the ID of this scheduler.</summary>
                    public int Id { get { return _queue.Id; } }
                    /// <summary>Gets all of the tasks scheduled to this queue.</summary>
                    public IEnumerable<Task> ScheduledTasks { get { return _queue.GetScheduledTasks(); } }
                    /// <summary>Gets the CustomTaskScheduler with which this queue is associated.</summary>
                    public InnerCustomTaskScheduler AssociatedScheduler { get { return _queue._pool; } }
                }

                /// <summary>The scheduler with which this pool is associated.</summary>
                private readonly InnerCustomTaskScheduler _pool;
                /// <summary>The work items stored in this queue.</summary>
                internal readonly Queue<Task> _workItems;
                /// <summary>Whether this queue has been disposed.</summary>
                internal bool _disposed;
                /// <summary>Gets the priority for this queue.</summary>
                internal int _priority;

                /// <summary>Initializes the queue.</summary>
                /// <param name="priority">The priority associated with this queue.</param>
                /// <param name="pool">The scheduler with which this queue is associated.</param>
                internal CustomTaskSchedulerQueue(int priority, InnerCustomTaskScheduler pool)
                {
                    _priority = priority;
                    _pool = pool;
                    _workItems = new Queue<Task>();
                }

                /// <summary>Gets the number of tasks waiting in this scheduler.</summary>
                internal int WaitingTasks
                {
                    get
                    {
                        return _workItems.Count;
                    }
                }

                /// <summary>Gets the tasks scheduled to this scheduler.</summary>
                /// <returns>An enumerable of all tasks queued to this scheduler.</returns>
                protected override IEnumerable<Task> GetScheduledTasks()
                {
                    return _workItems.ToList();
                }

                /// <summary>Queues a task to the scheduler.</summary>
                /// <param name="task">The task to be queued.</param>
                protected override void QueueTask(Task task)
                {
                    if (_disposed) throw new ObjectDisposedException(GetType().Name);

                    // Queue up the task locally to this queue, and then notify
                    // the parent scheduler that there's work available
                    lock (_pool.m_queue_groups)
                        _workItems.Enqueue(task);
                    _pool.NotifyNewWorkItem();
                }

                /// <summary>Tries to execute a task synchronously on the current thread.</summary>
                /// <param name="task">The task to execute.</param>
                /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
                /// <returns>true if the task was executed; otherwise, false.</returns>
                protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
                {
                    // If we're using our own threads and if this is being called from one of them,
                    // or if we're currently processing another task on this thread, try running it inline.
                    return m_task_processing_thread.Value && TryExecuteTask(task);
                }

                /// <summary>Runs the specified ask.</summary>
                /// <param name="task">The task to execute.</param>
                internal void ExecuteTask(Task task) { TryExecuteTask(task); }

                /// <summary>Gets the maximum concurrency level to use when processing tasks.</summary>
                public override int MaximumConcurrencyLevel { get { return _pool.MaximumConcurrencyLevel; } }

                /// <summary>Signals that the queue should be removed from the scheduler as soon as the queue is empty.</summary>
                public void Dispose()
                {
                    if (!_disposed)
                    {
                        lock (_pool.m_queue_groups)
                        {
                            // We only remove the queue if it's empty.  If it's not empty,
                            // we still mark it as disposed, and the associated CustomTaskScheduler
                            // will remove the queue when its count hits 0 and its _disposed is true.
                            if (_workItems.Count == 0)
                            {
                                _pool.RemoveQueue_NeedsLock(this);
                            }
                        }
                        _disposed = true;
                    }
                }
            }
        }

        private Lazy<InnerCustomTaskScheduler> m_scheduler;

        internal CustomTaskScheduler(int a_thread_count, string a_name)
        {
            m_scheduler = new Lazy<InnerCustomTaskScheduler>(() =>
                new InnerCustomTaskScheduler(a_thread_count, a_name));
        }

        internal TaskScheduler this[Priority a_priority]
        {
            get
            {
                return m_scheduler.Value.Scheduler(a_priority);
            }
        }
    }
}