using Microsoft.Extensions.Logging;
using Utils;

namespace Threads
{
    internal static class Philosophers
    {
        internal const int PhilosophersCount = 5;
        internal static void Execute()
        {
            var forks = Enumerable.Repeat(new object(), PhilosophersCount).ToList();
            var philosophers = forks.Select((_, i) => new PhilosopherData(forks, i));
            var threads = philosophers.Select(x => new Thread(x.Meal));

            Console.WriteLine("Start meals");
            foreach (var thread in threads)
            {
                thread.Start();
            }
        }
    }

    internal class PhilosopherData
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

        /// <summary>
        /// Флаг, указывающий, выводить ли информацию в консоль.
        /// </summary>
        private static readonly bool IsDebug = ConfigurationProvider.GetValue<bool>("isDebug");

        private readonly Random _rand = new();

        private readonly Mutex _preventDeadlock = new Mutex();
        private static long _forksPreDeadlockCount = 0;

        internal PhilosopherData(IReadOnlyList<object> forks, int id)
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
            WriteEvent($"Philosopher {Id} is about to release forks");
            if (Interlocked.Read(ref _forksPreDeadlockCount) == Philosophers.PhilosophersCount)
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
            WriteEvent($"Philosopher {Id} took forks");
            Interlocked.Increment(ref _forksPreDeadlockCount);
            if (Interlocked.Read(ref _forksPreDeadlockCount) == Philosophers.PhilosophersCount)
            {
                _preventDeadlock.WaitOne();
                WriteEvent($"Philosopher {Id} waits to avoid deadlock");
            }
        }

        private void Wait(string waitingClause)
        {
            var sleepPeriod = _rand.Next(1000);
            WriteEvent($"Philosopher {Id} {waitingClause} for {sleepPeriod}");
            Thread.Sleep(sleepPeriod);
        }


        private static void WriteEvent(string message)
        {
            if (IsDebug)
            {
                Console.WriteLine(message);
            }
        }
    }
}