using MyThreadPool;

using (var pool = new WorkerPool())
{
    pool.Queue(() => WriteLine("From thread pool"));
}

ReadLine();