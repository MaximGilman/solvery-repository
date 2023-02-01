using Philosophers;
namespace Philosophers;

internal class Program
{
    
    internal const int PhilosophersCount = 5;

    public static void Main(string[] args)
    {


        var forks = Enumerable.Repeat(new object(), PhilosophersCount).ToList();
        var philosophers = forks.Select((_, i) => new Philosopher(forks, i));
        var threads = philosophers.Select(x => new Thread(x.Meal));

        Console.WriteLine("Start meals");
        foreach (var thread in threads)
        {
            thread.Start();
        }
    }
}


