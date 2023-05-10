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
        _threadPool.Execute(state, d.Invoke);
    }
}
