using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedGaussianMethodLES
{
    public class ThreadPool<T>
    {
        public bool Ready { get => _ready; }
        public bool WorkingNow { get => _WorkingNow(); }

        private bool _ready = true;
        private int threadNextIndex = 0;

        private Mutex mutex;
        private Task[] threads;
        private bool[] working;

        private ConcurrentQueue<Action<T>> tasks;
        private ConcurrentQueue<T> parameters;

        public ThreadPool(int threadCount)
        {
            tasks = new ConcurrentQueue<Action<T>>();
            parameters = new ConcurrentQueue<T>();
            threads = new Task[threadCount];
            working = new bool[threadCount];
            mutex = new Mutex(false);
            for(int i = 0; i < threadCount; i++)
            {
                threads[i] = Task.Run(StartThread);
            }
        }

        public void AddTask(Action<T> task, T obj)
        {
            mutex.WaitOne();
            tasks.Enqueue(task);
            parameters.Enqueue(obj);
            mutex.ReleaseMutex();
        }

        public void StartPool()
        {
            _ready = true;
        }

        public void EndPool()
        {
            _ready = false;
            while (_WorkingNow()) ;
        }

        private void StartThread()
        {
            mutex.WaitOne();
            int idx = threadNextIndex;
            threadNextIndex++;
            mutex.ReleaseMutex();

            while(_ready) 
            { 
                if (!parameters.IsEmpty)
                {
                    if (tasks.TryDequeue(out Action<T> task))
                    {
                        if (parameters.TryDequeue(out T obj))
                        { 
                            working[idx] = true;
                            task.Invoke(obj);
                            working[idx] = false;
                        }
                    }
                }
            }
        }

        private bool _WorkingNow()
        {
            return working.Any(w => w) || !tasks.IsEmpty;
        }
    }
}
