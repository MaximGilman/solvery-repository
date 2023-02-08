namespace Philosophers;

internal static class Program
{
    private const int PHILOSOPHERS_COUNT = 5;

    public static void Main()
    {
        var forks = Enumerable.Repeat(new object(), PHILOSOPHERS_COUNT).ToList();
        var philosophers = forks.Select((_, i) => new Philosopher(forks, i));
        var threads = philosophers.Select(x => new Thread(() => x.Meal(CancellationToken.None)));

        Console.WriteLine("Start meals");
        foreach (var thread in threads)
        {
            thread.Start();
        }
    }
}