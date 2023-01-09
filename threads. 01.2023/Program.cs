#region simple-thread

class Program
{
    private static readonly List<int> Collection = Enumerable.Range(0, 10).ToList();
    private static List<int>.Enumerator _enumerator = Collection.GetEnumerator();
    private static readonly object BalanceLock = new();

    public static void Main()
    {
        Console.WriteLine($"Parent thread ID: {Environment.CurrentManagedThreadId} \n");

        var mainTask = new Task(MyLockedAction);
        var childTask = Task.Run(MyLockedAction);
        mainTask.RunSynchronously();

        Task.WaitAll(childTask, mainTask);
    }

    private static void MyLockedAction()
    {
            lock (BalanceLock)
            {
                if (_enumerator.MoveNext())
                    Console.WriteLine($"Thread ID: {Environment.CurrentManagedThreadId}. Value: {_enumerator.Current}");
            }
    }
}

# endregion