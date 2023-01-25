namespace Threads;

internal static class MonitorQueue
{
    private static readonly Queue<string> Queue= new Queue<string>();

    private static readonly object LockTarget = new();
    internal static void Execute()
    {
        var provider = new Provider() {LockTarget=LockTarget, Queue=Queue};
        var handler = new Handler() {LockTarget=LockTarget, Queue=Queue};
        handler.Receive();       
        provider.Send("Hello");
        handler.Receive();
        provider.Send("World");

    }
}

internal class Provider : QueueWorker<string>
{
    internal int Send(string message)
    {
        var croppedMessage = message;
        lock (LockTarget)
        {
            if (Queue.Count > 10) Monitor.Wait(LockTarget);
            Monitor.PulseAll(LockTarget);
            Queue.Enqueue(croppedMessage);
        }

        return croppedMessage.Length;
    }
}

internal class Handler : QueueWorker<string>
{
    internal int Receive()
    {
        lock (LockTarget)
        {
            Monitor.Wait(LockTarget);
            var message = Queue.Dequeue();
            Console.WriteLine(message);
            return message.Length;
        }
        
    }
}

internal abstract class QueueWorker<T>
{
    internal object LockTarget { get; init; }
    internal Queue<T> Queue { get; init; }
    ~QueueWorker () {
        Monitor.Exit(LockTarget);
    }
}