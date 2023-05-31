using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Utils.Guards;

namespace InstanceThreadPool;

public sealed class InstanceThreadPool : IInstanceThreadPool, IDisposable
{
    private readonly ThreadPriority _threadPriority;
    private readonly string _threadPoolName;
    private readonly ImmutableArray<Thread> _threads;
    private readonly ConcurrentQueue<ThreadPoolWorkGrain> _actionsQueue = new();
    private readonly AutoResetEvent _actionExecuteEvent = new(false);
    private readonly ManualResetEvent _stopWaitHandle = new(true);

    public InstanceThreadPool(int maxThreadsCount) : this(maxThreadsCount,
        ThreadPriority.Normal, null)
    {
    }

    private InstanceThreadPool(int maxThreadsCount, ThreadPriority threadPriority, string threadPoolName)
    {
        Guard.IsGreater(maxThreadsCount, default);
        _threadPriority = threadPriority;
        _threadPoolName = threadPoolName;
        _threads = GetImmutableThreadArray(maxThreadsCount);
    }

    private ImmutableArray<Thread> GetImmutableThreadArray(int capacity)
    {
        var immutableArrayBuilder = ImmutableArray.CreateBuilder<Thread>(capacity);

        for (var i = 0; i < capacity; i++)
        {
            var threadName = this.GetThreadName(i);
            var thread = new Thread(ThreadExecution)
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

    public void QueueExecute(Action action)
    {
        Guard.IsNotDefault(action);
        QueueExecute(null, (_) => action());
    }

    public void QueueExecute(object parameter, Action<object> action)
    {
        Guard.IsNotDefault(action);
        var workItemGrain = new ThreadPoolWorkGrain(parameter, WrappedAction, CancellationToken.None);
        _actionsQueue.Enqueue(workItemGrain);
        _actionExecuteEvent.Set();

        void WrappedAction(object actionParameter, CancellationToken _)
        {
            action(actionParameter);
        }
    }

    public void QueueExecute(Action<CancellationToken> action, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        Guard.IsNotDefault(action);
        QueueExecute(null, (_, _) => action(cancellationToken), cancellationToken);
    }

    public void QueueExecute(object parameter, Action<object, CancellationToken> action,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        Guard.IsNotDefault(action);

        var workItemGrain = new ThreadPoolWorkGrain(parameter, action, cancellationToken);
        _actionsQueue.Enqueue(workItemGrain);
        _actionExecuteEvent.Set();

    }

    private void ThreadExecution()
    {
        SynchronizationContext.SetSynchronizationContext(new InstanceThreadPoolSynchronizationContext(this));

        var threadName = Thread.CurrentThread.Name;
        while (true)
        {
            while (_actionsQueue.IsEmpty)
            {
                _actionExecuteEvent.WaitOne();
                _stopWaitHandle.WaitOne();
            }

            if (_actionsQueue.TryDequeue(out var actionWithParameter))
            {
                if (!_actionsQueue.IsEmpty)
                {
                    _actionExecuteEvent.Set();
                }

                try
                {
                    var (parameter, funcDelegate, cancellationToken) = actionWithParameter;
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    funcDelegate.Invoke(parameter, cancellationToken);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Ошибка выполнения задания в потоке {0}:{1}", threadName, ex);
                    throw;
                }
            }
        }
    }

    private string GetThreadName(int threadIndex) =>
        $"{nameof(InstanceThreadPool)}[{_threadPoolName ?? GetHashCode().ToString("x")}]-Thread[{threadIndex}]";

    public void Dispose()
    {
        _actionExecuteEvent?.Dispose();
        _stopWaitHandle?.Dispose();
    }
}
