using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using Microsoft.VisualBasic.CompilerServices;
using Utils;

namespace Threads
{
    internal static class Philosophers
    {
        internal static void Execute()
        {
            Mutex fork1 = new();
            Mutex fork2 = new();
            Mutex fork3 = new();
            Mutex fork4 = new();
            Mutex fork5 = new();
            
            // В самом верху сидит философ 1. Слева от него лежит вилка 1. Справа вилка 5.
            // По часовой, соответсвтвенно, ph1 -> f1 -> ph2 -> f2 и т.д.
            var ph1 = new PhilosopherData() {Id = 1, Left = fork1, Right = fork5};
            var ph2 = new PhilosopherData() {Id = 2, Left = fork2, Right = fork1};
            var ph3 = new PhilosopherData() {Id = 3, Left = fork2, Right = fork2};
            var ph4 = new PhilosopherData() {Id = 4, Left = fork4, Right = fork3};
            var ph5 = new PhilosopherData() {Id = 5, Left = fork5, Right = fork4};

            var thread1 = new Thread(() => ph1.Meal());
            var thread2 = new Thread(() => ph2.Meal());
            var thread3 = new Thread(() => ph3.Meal());
            var thread4 = new Thread(() => ph4.Meal());
            var thread5 = new Thread(() => ph5.Meal());
            
            
            // Считаем, что едят бесконечно.
            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread4.Start();
            thread5.Start();

        }
    }

    internal class PhilosopherData
    {
        internal int Id
        {
            private get => Id;
            init => Guard.IsLessOrEqual(value, PhilosophersCount);
        }

        /// <summary>
        /// Правая вилка.
        /// </summary>
        internal Mutex Left { get; init; }
        
        /// <summary>
        /// Левая вилка.
        /// </summary>
        internal Mutex Right { get; init; }

        /// <summary>
        /// Флаг, что нужно пропустить другие потоки - не брать следующие вилки, а подождать.
        /// </summary>
        private bool _shouldISkip = false;
        
        /// <summary>
        /// Флаг, указывающий, выводить ли информацию в консоль.
        /// </summary>
        private static readonly bool IsDebug = 
            bool.TryParse(ConfigurationManager.AppSettings["isDebug"], out bool isDebugSettings) 
            && isDebugSettings;
        
        private readonly Random _rand = new ();

        /// <summary>
        /// Количество ожидающих потоков. Если равно количеству - значит дэдлок.
        /// </summary>
        private static long _waitingCount = 0;
        
        /// <summary>
        /// Приоритеты, для будущих "торгов" при дедлоках.
        /// </summary>
        private static readonly int [] Priorities = { 1, 2, 3, 4, 5};

        private const int PhilosophersCount = 5;


        internal void Meal()
        {
            while (true)
            {
                Wait();
                TryTakeLeftFork();
                if (_shouldISkip)
                {
                    _shouldISkip = false;
                    Meal();
                }

                TryTakeRightFork();
                if (_shouldISkip)
                {
                    _shouldISkip = false;
                    Meal();
                }

                Wait("eating");
                ReleaseForks();
                Priorities[Id - 1] = Id;
            }
            return;
        }

        private void TryTakeLeftFork()
        {
            // Если поток попытается взять вилку слева, а она будет занята - пусть ожидает.
            Left.WaitOne();
            
            if (IsDebug)
                Console.WriteLine($"Philosopher {Id} took left fork");
        }
        
        private void TryTakeRightFork()
        {
            // Поток уже держит левую вилку, помечаем его, как ожидающий.
            Interlocked.Increment(ref _waitingCount);

            while (true)
            {
                if (Right.WaitOne(_rand.Next(1000)))
                {
                    // Правая вилка взята.
                    if (IsDebug)
                        Console.WriteLine($"Philosopher {Id} took right fork");
                    break;

                }
                else
                { 
                    var waitingCount = Interlocked.Read(ref _waitingCount);
                    
                    // Если правая вилка занята и все потоки ждут - значит дэдлок.
                    if (waitingCount == PhilosophersCount && Priorities.Min() == Priorities[Id -1])
                    {
                        // Поток перестает считаться ожидающим.
                        Interlocked.Decrement(ref _waitingCount);
                        // В этом случае самый "неприоритетный" поток отпускает первую вилку.
                        _shouldISkip = true;
                        Left.ReleaseMutex();
                        // За то, что "пропустил" вперед - его приоритет поднимается, в след. раз он пойдет раньше.
                        Priorities[Id -1] += PhilosophersCount;
                        return;
                    }
                    
                    // Если дэдлока нет - продолжаем ждать.
                }
            }
           
            Interlocked.Decrement(ref _waitingCount);
        }
        
        private void ReleaseForks()
        {
            Left.ReleaseMutex();
            Right.ReleaseMutex();
        }

        private void Wait(string waitingClause = "thinking")
        {
            var sleepPeriod = _rand.Next(5000);
            if (IsDebug)
                Console.WriteLine($"Philosopher {Id} {waitingClause} for {sleepPeriod}");
            Thread.Sleep(sleepPeriod);
        }
    }
}