using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Lifetime;
using CaptchaDestroy.ClientApi;
using Microsoft.VisualBasic;

namespace SimpleConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }


        static ConcurrentDictionary<int, CaptchaDestroyClient> clientPerThreadId = new();
        static async Task MainAsync()
        {
            // Create a scheduler that uses two threads.
            var lcts = new LimitedConcurrencyLevelTaskScheduler(16);
            var tasks = new List<Task>();

            // Create a TaskFactory and pass it our custom scheduler.
            var factory = new TaskFactory(lcts);
            var cts = new CancellationTokenSource();

            var builder = new ContainerBuilder();



            // builder.Register<ILifetimeScope>(s =>
            // {
            //     if (!scopePerThreadId.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var scope))
            //     {
            //         scope = (s  LifetimeScope).BeginLifetimeScope();
            //     }
            //     return scope;
            // });

            builder.Register<CaptchaDestroyClient>(s =>
            {
                if (!clientPerThreadId.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var client))
                {
                    var scope = s.Resolve<ILifetimeScope>().BeginLifetimeScope();
                    client = new CaptchaDestroyClient("http://localhost:57679", new HttpClient());
                    clientPerThreadId.TryAdd(Thread.CurrentThread.ManagedThreadId, client);
                }
                return client;
            });


            var container = builder.Build();

            for (var i = 0; i < 1000; i++)
            {
                tasks.Add(factory.StartNew(() =>
                {
                    var client = container.Resolve<CaptchaDestroyClient>();

                    var res = client.AddCaptchaAsync(
                            "F1877OFH3USBGF0P", false,
                            new CreateCaptchaDTO() 
                            { CaptchaUri = new("https://vk.com/captcha.php?sid=625944628698&s=1") }).GetAwaiter().GetResult();
                }));
            }
            var sw = new Stopwatch();
            sw.Start();
            await Task.WhenAll(tasks);
            sw.Stop();
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds}");
            Console.WriteLine();
        }
        public static async Task Work(IContainer container)
        {
            var client = container.Resolve<CaptchaDestroyClient>();

            var res = await client.AddCaptchaAsync(
                    "F1877OFH3USBGF0P", false,
                    new CreateCaptchaDTO() { CaptchaUri = new("https://vk.com/captcha.php?sid=625944628698&s=1") });
        }

    }

    // Provides a task scheduler that ensures a maximum concurrency level while
    // running on top of the thread pool.
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        // Indicates whether the current thread is processing work items.
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;

        // The list of tasks to be executed
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

        // The maximum concurrency level allowed by this scheduler.
        private readonly int _maxDegreeOfParallelism;

        // Indicates whether the scheduler is currently processing work items.
        private int _delegatesQueuedOrRunning = 0;

        // Creates a new instance with the specified degree of parallelism.
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        // Queues a task to the scheduler.
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        // Inform the ThreadPool that there's work to be executed for this scheduler.
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // Get the next item from the queue
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        base.TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread
                finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }

        // Attempts to execute the specified task on the current thread.
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
                // Try to run the task.
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        // Attempt to remove a previously scheduled task from the scheduler.
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        // Gets the maximum concurrency level supported by this scheduler.
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        // Gets an enumerable of the tasks currently scheduled on this scheduler.
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }
}
