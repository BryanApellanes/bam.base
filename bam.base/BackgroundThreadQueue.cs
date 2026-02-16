using System.Collections.Concurrent;

namespace Bam
{
    /// <summary>
    /// A queue processing facility that processes
    /// enqueued items in a background thread.
    /// </summary>
    /// <typeparam name="T">The type of items to be enqueued and processed.</typeparam>
    public class BackgroundThreadQueue<T>
    {
        bool _warned;
        /// <summary>
        /// Initializes a new instance with no processor defined. The first enqueued item will raise the <see cref="Exception"/> event.
        /// </summary>
        public BackgroundThreadQueue()
        {
            Continue = true;
            Process = (o) =>
            {
                if (!_warned)
                {
                    _warned = true;
                    Exception?.Invoke(this, new BackgroundThreadQueueEventArgs { Exception = new InvalidOperationException("No processor defined") });
                }
            };
        }

        /// <summary>
        /// Initializes a new instance with the specified processing action.
        /// </summary>
        /// <param name="process">The action to invoke for each dequeued item.</param>
        public BackgroundThreadQueue(Action<T> process)
        {
            Continue = true;
            Process = process;
        }

        /// <summary>
        /// Gets the number of items currently waiting in the queue.
        /// </summary>
        public int WriteQueueCount => _processQueue.Count;

        readonly ConcurrentQueue<T> _processQueue = new ConcurrentQueue<T>();
        readonly object _procLock = new object();
        /// <summary>
        /// Adds an item to the queue and signals the background thread to begin processing.
        /// </summary>
        /// <param name="data">The item to enqueue for processing.</param>
        public void Enqueue(T data)
        {
            lock (_procLock)
            {
                if (Continue)
                {
                    StartProcessThread();
                }
            }
            
            _processQueue.Enqueue(data);
            if (Continue)
            {
                _waitSignal.Set();
            }
        }

        private void StartProcessThread()
        {
            if (ProcessThread.ThreadState != (ThreadState.Running | ThreadState.Background | ThreadState.WaitSleepJoin))
            {
                _processThread = null;
                ProcessThread.Start();
            }
        }

        /// <summary>
        /// Occurs when an exception is thrown during item processing or when no processor is defined.
        /// </summary>
        public event EventHandler Exception;
        bool _continue;
        /// <summary>
        /// Gets or sets whether the background thread should continue processing. Setting to true when items are queued restarts the processing thread.
        /// </summary>
        public bool Continue
        {
            get => _continue;
            set
            {
                _continue = value;
                if(_continue && _processQueue.Count > 0)
                {
                    StartProcessThread();
                }
            }
        }
        /// <summary>
        /// Occurs when the background thread is waiting for new items to be enqueued.
        /// </summary>
        public event EventHandler Waiting;

        /// <summary>
        /// Occurs when the background thread begins processing enqueued items.
        /// </summary>
        public event EventHandler Processing;

        /// <summary>
        /// Occurs when the background thread has finished processing all enqueued items.
        /// </summary>
        public event EventHandler QueueEmptied;
        Thread _processThread;
        readonly AutoResetEvent _waitSignal = new AutoResetEvent(false);
        readonly object _processThreadLock = new object();
        /// <summary>
        /// Gets the background processing thread, creating and configuring it if necessary.
        /// </summary>
        public Thread ProcessThread
        {
            get
            {
                return _processThreadLock.DoubleCheckLock(ref _processThread, () =>
                {
                    _processThread = new Thread(() =>
                    {
                        while (Continue)
                        {
                            try
                            {
                                Waiting?.Invoke(this, new BackgroundThreadQueueEventArgs());
                                _waitSignal.WaitOne();
                                Processing?.Invoke(this, new BackgroundThreadQueueEventArgs());
                                while (_processQueue.Count > 0)
                                {
                                    if (_processQueue.TryDequeue(out T val))
                                    {
                                        Process(val);
                                    }
                                }
                                QueueEmptied?.Invoke(this, new BackgroundThreadQueueEventArgs());
                            }
                            catch (Exception ex)
                            {
                                Exception?.Invoke(this, new BackgroundThreadQueueEventArgs { Exception = ex });
                            }
                        }
                    })
                    {
                        IsBackground = true
                    };
                    return _processThread;
                });
            }
        }

        /// <summary>
        /// Gets or sets the action invoked for each item dequeued from the processing queue.
        /// </summary>
        public Action<T> Process { get; set; }
    }
}
