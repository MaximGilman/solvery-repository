namespace InstanceThreadPool;

public class InstanceThreadPoolSynchronizationContext : SynchronizationContext
{
    private readonly IInstanceThreadPool _threadPool;

    public InstanceThreadPoolSynchronizationContext(IInstanceThreadPool threadPool)
    {
        _threadPool = threadPool;
    }

    public override void Post(SendOrPostCallback callback, object state)
    {
        void Action(object parameter, CancellationToken cancellationToken)
        {
            callback.Invoke(state);
        }

        _threadPool.QueueExecute(state, Action, CancellationToken.None);
    }
}
