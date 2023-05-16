namespace InstanceThreadPool;

public class InstanceThreadPoolSynchronizationContext : SynchronizationContext
{
    private readonly IInstanceThreadPool _threadPool;

    public InstanceThreadPoolSynchronizationContext(IInstanceThreadPool threadPool)
    {
        _threadPool = threadPool;
    }

    public override void Post(SendOrPostCallback d, object state)
    {
        void Action(object o, CancellationToken cancellationToken)
        {
            d.Invoke(state);
        }

        _threadPool.QueueExecute(state, Action, CancellationToken.None);
    }
}
