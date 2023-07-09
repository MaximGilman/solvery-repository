namespace TCP.Task2.Listener;

public interface ITcpListenerAsynchronizer
{
    public Task Init(params object[] parameters);
    public Task QueueWork(params object[] parameters);
    public Task GetResult();
}
