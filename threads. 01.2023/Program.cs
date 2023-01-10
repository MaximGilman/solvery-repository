#region simple-thread

using System.Collections.Concurrent;

namespace threads._01._2023;

static class Program
{
    public static void Main()
    {
        using (BlockingCollection<int> collection = new BlockingCollection<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })
        {
            Action read = () =>
            {
                while (collection.TryTake(out var item))
                    Console.WriteLine($"Thread ID: {Environment.CurrentManagedThreadId}. Value: {item}");
            };
            
            Console.WriteLine($"Parent thread ID: {Environment.CurrentManagedThreadId} \n");

            var mainTask = new Task(read);
            var childTask = Task.Run(read);
            mainTask.RunSynchronously();

            Task.WaitAll(childTask, mainTask);
        }
    }
    
}

# endregion