using Utils.Guards;

namespace TCP.Task2.Listener;

public class BaseTcpListenerAsynchronizer : ITcpListenerAsynchronizer
{
    private List<Task> _tasks;

    public async Task Init(params object[] parameters)
    {
        _tasks = new List<Task>();
    }

    public async Task QueueWork(params object[] parameters)
    {
        Guard.IsEqual(2, parameters.Length);
        var func = parameters[0] as Func<Task>;
        Guard.IsNotDefault(func);
        var cancellationToken = (CancellationToken)parameters[1];


        _tasks.Add(Task.Run(func!, cancellationToken));
    }

    public async Task GetResult()
    {
        await Task.WhenAll(_tasks);

    }
}
