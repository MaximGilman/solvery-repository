namespace Philosophers;
using Utils;

internal class Philosopher
{
    private int Id { get; }

    /// <summary>
    /// Правая вилка.
    /// </summary>
    private object FirstFork { get; }

    /// <summary>
    /// Левая вилка.
    /// </summary>
    private object SecondFork { get; }

    private readonly Random _rand = new();
    
    internal Philosopher(IReadOnlyList<object> forks, int id)
    {
        FirstFork = forks[id];
        SecondFork = forks[(id + 1) % forks.Count];

        // У последнего философа меняем вилки.
        if (id == forks.Count - 1)
        {
            SecondFork = forks[id];
            FirstFork = forks[(id + 1) % forks.Count];
        }
        

        Id = id;
    }


    internal void Meal()
    {
        while (true)
        {
            Wait("thinking");
            
            lock(FirstFork)
            lock (SecondFork)
            {
                Wait("eating");
            }
        }
    }
    
    private void Wait(string waitingClause)
    {
        var sleepPeriod = _rand.Next(1000);
        ConsoleWriter.WriteEvent($"Philosopher {Id} {waitingClause} for {sleepPeriod}");
        Thread.Sleep(sleepPeriod);
    }
}

