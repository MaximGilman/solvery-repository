namespace InstanceThreadPool;

public class ThreadPoolSynchronizationContext : SynchronizationContext
{
    private readonly InstanceThreadPool _threadPool;

    public ThreadPoolSynchronizationContext(InstanceThreadPool threadPool)
    {
        _threadPool = threadPool;
    }

    public override void Post(SendOrPostCallback d, object state)
    {
        _threadPool.Execute(state, d.Invoke);
    }
}
