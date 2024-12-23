using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Job
{
    public class JobQueue
    {
        Queue<IJob> _queue = new Queue<IJob>();
        List<IJob> _pendingList = new List<IJob>();
        object _lock = new object();
        bool _flush = false;
        public void Push(Action action) { Push(new Job(action)); }
        public void Push<T1>(Action<T1> action, T1 t1) { Push(new Job<T1>(action, t1)); }
        public void Push<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Push(new Job<T1, T2>(action, t1, t2)); }
        public void Push<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Push(new Job<T1, T2, T3>(action, t1, t2, t3)); }
        public void Push<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { Push(new Job<T1, T2, T3, T4>(action, t1, t2, t3, t4)); }

        public void Push(IJob job)
        {
            bool flush = false;
            lock (_lock)
            {
                _queue.Enqueue(job);
                if (_flush == false)
                {
                    flush = true;
                    _flush = true;
                }
            }
            if (flush)
                Flush();
        }
        public void Flush()
        {
            while (true)
            {
                lock (_lock)
                {
                    while (_queue.Count > 0)
                    {
                        _pendingList.Add(_queue.Dequeue());
                    }
                }
                foreach (IJob job in _pendingList)
                {
                    job.Execute();
                }
                _pendingList.Clear();
                lock (_lock)
                {
                    if (_queue.Count == 0)
                    {
                        _flush = false;
                        return;
                    }
                }
            }

        }
    }
}
