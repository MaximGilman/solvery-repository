using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Utils.Guards;

namespace InstanceThreadPool;

public sealed class InstanceThreadPool : IInstanceThreadPool
{
    private readonly ThreadPriority _threadPriority;
    private readonly string _threadPoolName;
    private readonly ImmutableArray<Thread> _threads;
    private readonly ConcurrentQueue<(object parameter, Action<object> action)> _actionsQueue = new();
    private readonly AutoResetEvent _actionExecuteEvent = new(false);

    public InstanceThreadPool(int maxThreadsCount) : this(maxThreadsCount, ThreadPriority.Normal, null)
    {
    }

    private InstanceThreadPool(int maxThreadsCount, ThreadPriority threadPriority, string threadPoolName)
    {
        Guard.IsGreater(maxThreadsCount, 0);
        _threadPriority = threadPriority;
        _threadPoolName = threadPoolName;
        _threads = GetImmutableThreadArray(maxThreadsCount);
    }

    private ImmutableArray<Thread> GetImmutableThreadArray(int capacity)
    {
        var immutableArrayBuilder = ImmutableArray.CreateBuilder<Thread>(capacity);

        for (var i = 0; i < _threads.Length; i++)
        {
            var threadName = this.GetThreadName(i);
            var thread = new Thread(WorkingThread)
            {
                Name = threadName,
                IsBackground = true,
                Priority = _threadPriority,
            };
            thread.Start();
            immutableArrayBuilder.Add(thread);
        }
        Guard.IsEqual(immutableArrayBuilder.Capacity, immutableArrayBuilder.Count);
        return immutableArrayBuilder.MoveToImmutable();
    }

    public void Execute(Action action) => Execute(null, _ => action());

    public void Execute(object parameter, Action<object> action)
    {
        _actionsQueue.Enqueue((parameter, action));
        _actionExecuteEvent.Set();
    }

    private void WorkingThread()
    {
        SynchronizationContext.SetSynchronizationContext(new InstanceThreadPoolSynchronizationContext(this));

        var threadName = Thread.CurrentThread.Name;
        while (true)
        {
            while (_actionsQueue.IsEmpty)
            {
                _actionExecuteEvent.WaitOne();
            }

            if (_actionsQueue.TryDequeue(out var actionWithParameter))
            {
                if (!_actionsQueue.IsEmpty)
                {
                    _actionExecuteEvent.Set();
                }

                var (parameter, action) = actionWithParameter;
                try
                {
                    action(parameter);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Ошибка выполнения задания в потоке {0}:{1}", threadName, ex);
                }
            }
        }
    }

    private string GetThreadName(int threadIndex) =>
        $"{nameof(InstanceThreadPool)}[{_threadPoolName ?? GetHashCode().ToString("x")}]-Thread[{threadIndex}]";
}
