namespace Threads;

public class MutexEducativeExamples
{
    // Program exits normally because mutex is reentrant
    internal static void Q1()
    {
        Mutex mutex = new Mutex();
        mutex.WaitOne();
        mutex.WaitOne();
        Console.WriteLine("Program Exiting");
        mutex.ReleaseMutex();
        mutex.ReleaseMutex();
    }

    // Program exits normally 
    internal static void Q2()
    {
        Mutex mutex = new Mutex();
        mutex.WaitOne();
        Console.WriteLine("Program Exiting");
    }

    // AbandonedMutexException is thrown when t2 attempts to acquire the mutex (но на Unix)
    internal static void Q3()
    {
        Mutex mutex = new Mutex();

        Thread t1 = new Thread(() =>
        {

            mutex.WaitOne();
            // Exit without releasing mutex
            Console.WriteLine("t1 exiting");
        });
        t1.Start();
        t1.Join();

        Thread t2 = new Thread(() =>
        {
            // Acquire an unreleased mutex
            mutex.WaitOne();
            Console.WriteLine("t2 exiting");
        });
        t2.Start();
        t2.Join();
    }

    // Если в DIW ошибка - то мьютекс не отпустится
    internal static void Q4()
    {
        using var mutex = new Mutex();
        mutex.WaitOne();
        DoImportantWork();
        mutex.ReleaseMutex();
        Console.WriteLine("All Good");
    }

    // AbandonedMutexException is thrown because the mutex isn’t released as many times as it is acquired.
    internal static void Q5()
    {
        Mutex mutex = new Mutex();

        Thread t1 = new Thread(() =>
        {
            // Child thread locks the mutex
            // twice but releases it only once
            mutex.WaitOne();
            mutex.WaitOne();
            mutex.ReleaseMutex();
        });

        t1.Start();
        t1.Join();

        // Main thread attemps to acquire the mutex
        mutex.WaitOne();
        Console.WriteLine("All Good");
        mutex.ReleaseMutex();
    }


    private static void DoImportantWork() => throw new Exception();
}