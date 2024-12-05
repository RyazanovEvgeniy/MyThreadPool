namespace MyThreadPool;

class WorkerPool : IDisposable
{
    private readonly Thread[] _threads;
    private readonly Queue<Action> _actions = new();
    private readonly object _syncRoot = new();

    public WorkerPool(int maxThreads = 4)
    {
        _threads = new Thread[maxThreads];
        for (var i = 0; i < maxThreads; i++)
        {
            _threads[i] = new Thread(ThreadProc)
            {
                IsBackground = true,
                Name = $"MythreadPool {i}"
            };
            _threads[i].Start();
        }
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if (!IsDisposed)
        {
            bool isDisposing = false;
            Monitor.Enter(_syncRoot);

            try
            {
                if (!IsDisposed)
                {
                    IsDisposed = true;
                    Monitor.PulseAll(_syncRoot);
                    isDisposing = true;
                }
            }
            finally
            {
                Monitor.Exit(_syncRoot);
            }

            if (isDisposing)
            {
                for (var i = 0; i < _threads.Length; i++)
                {
                    _threads[i].Join();
                }
            }
        }
    }

    public void Queue(Action action)
    {
        Monitor.Enter(_syncRoot);
        try
        {
            _actions.Enqueue(action);
            if (_actions.Count == 1)
            {
                Monitor.Pulse(_syncRoot);
            }
        }
        finally
        {
            Monitor.Exit(_syncRoot);
        }
    }

    private void ThreadProc()
    {
        while (true)
        {
            if (IsDisposed)
                return;

            Action action;

            Monitor.Enter(_syncRoot);
            try
            {
                if (_actions.Count > 0)
                {
                    action = _actions.Dequeue();
                }
                else
                {
                    Monitor.Wait(_syncRoot);
                    continue;
                }
            }
            finally
            {
                Monitor.Exit(_syncRoot);
            }

            action();
        }
    }
}