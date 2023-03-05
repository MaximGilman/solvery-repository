using System.Collections.Concurrent;
using System.Diagnostics;
using Utils.Guards;

namespace InstanceThreadPool;

public sealed class InstanceThreadPool
{
    private readonly ThreadPriority _threadPriority;
    private readonly string _name;
    private readonly Thread[] _threads;
    private readonly AutoResetEvent _actionExecuteEvent = new(false);
    private readonly AutoResetEvent _actionQueueAccessEvent = new(true);

    // ToDo: переделать свою очередь на generic  и заиспользовать здесь.
    private readonly ConcurrentQueue<(object parameter, Action<object> action)> _actionsQueue = new();


    public InstanceThreadPool(int maxThreadsCount, ThreadPriority threadPriority = ThreadPriority.Normal,
        string name = null)
    {
        Guard.IsGreater(maxThreadsCount, 0);
        _threadPriority = threadPriority;
        _name = name;

        _threads = new Thread[maxThreadsCount];
        Init();
    }

    private void Init()
    {
        for (int i = 0; i < _threads.Length; i++)
        {
            var name = $"{nameof(InstanceThreadPool)}[{_name ?? GetHashCode().ToString("x")}]-Thread[{i}]";
            var thread = new Thread(WorkingThread)
            {
                Name = name,
                IsBackground = true,
                Priority = _threadPriority
            };
            _threads[i] = thread;
            thread.Start();
        }
    }

    public void Execute(Action action) => Execute(null, _ => action());

    public void Execute(object parameter, Action<object> action)
    {
        _actionQueueAccessEvent.WaitOne(); // запрос доступа к очереди
        _actionsQueue.Enqueue((parameter, action));
        _actionQueueAccessEvent.Set(); // отпускаем доступ к очереди

        _actionExecuteEvent.Set(); // Пингуем работника
    }

    private void WorkingThread()
    {
        var threadName = Thread.CurrentThread.Name;
        while (true)
        {
            _actionExecuteEvent.WaitOne();
            _actionQueueAccessEvent.WaitOne();

            while (_actionsQueue.IsEmpty)
            {
                _actionQueueAccessEvent.Set();
                _actionExecuteEvent.WaitOne();
                _actionQueueAccessEvent.WaitOne();
            }

            if (!_actionsQueue.TryDequeue(out var actionWithParameter))
            {
                _actionQueueAccessEvent.Set();
                throw new ApplicationException("Произошла ошибка при чтении задачи из очереди");
            }

            if (!_actionsQueue.IsEmpty)
            {
                _actionExecuteEvent.Set();
            }
            _actionQueueAccessEvent.Set();
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
