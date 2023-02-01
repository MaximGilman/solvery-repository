namespace Philosophers;

using static Program;
using Utils;

internal class Philosopher
{
    private int Id { get; }

    /// <summary>
    /// Правая вилка.
    /// </summary>
    private object Left { get; }

    /// <summary>
    /// Левая вилка.
    /// </summary>
    private object Right { get; }

    private readonly Random _rand = new();

    private readonly Mutex _preventDeadlock = new Mutex();
    private static long _forksPreDeadlockCount = 0;

    internal Philosopher(IReadOnlyList<object> forks, int id)
    {
        Left = forks[id];
        Right = forks[(id + 1) % forks.Count];
        Id = id;
    }


    internal void Meal()
    {
        while (true)
        {
            Wait("thinking");
            TryTakeOrWait();
            lock (Left)
            lock (Right)
            {
                Wait("eating");
            }

            ReleaseForks();
        }
    }


    private void ReleaseForks()
    {
        ConsoleWriter.WriteEvent($"Philosopher {Id} is about to release forks");
        if (Interlocked.Read(ref _forksPreDeadlockCount) == PhilosophersCount)
        {
            Interlocked.Decrement(ref _forksPreDeadlockCount);
            try
            {
                _preventDeadlock.ReleaseMutex();
            }
            catch
            {
                // ignored
            }
        }
        else
        {
            Interlocked.Decrement(ref _forksPreDeadlockCount);
        }
    }


    private void TryTakeOrWait()
    {
        ConsoleWriter.WriteEvent($"Philosopher {Id} took forks");
        Interlocked.Increment(ref _forksPreDeadlockCount);

        // Если все, кроме текущего потока взяли вилки - то ждем, иначе будет дэдлок
        if (Interlocked.Read(ref _forksPreDeadlockCount) == PhilosophersCount)
        {
            _preventDeadlock.WaitOne();
            ConsoleWriter.WriteEvent($"Philosopher {Id} waits to avoid deadlock");
        }
    }

    private void Wait(string waitingClause)
    {
        var sleepPeriod = _rand.Next(1000);
        ConsoleWriter.WriteEvent($"Philosopher {Id} {waitingClause} for {sleepPeriod}");
        Thread.Sleep(sleepPeriod);
    }
}

