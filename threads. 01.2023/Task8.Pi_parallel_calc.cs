using System.Diagnostics;
using System.Runtime.InteropServices;

namespace threads._01._2023;

public class Task8_Pi_parallel_calc
{
    private const int MAX_THREAD_COUNT = 10;
    private static long MAX_ITER_COUNT = 200_000;
    private static long currentIterId = 1;
    private static double pi = 0;
    private static object piLock = new object();
    static readonly Stopwatch stopWatch = new Stopwatch();


    public static async Task Execute()
    {
        // В 1 поток
        // stopWatch.Start();
        // DoStep();
        // stopWatch.Stop();
        // Console.WriteLine(pi);
        // Console.WriteLine(stopWatch.Elapsed);

        // В N потоков - из-за локов время то же.
        stopWatch.Start();
        var tasks = Enumerable.Range(0, MAX_THREAD_COUNT).Select(_ => Task.Run(DoStep));

        await Task.WhenAll(tasks).ContinueWith(_ =>
        {
            stopWatch.Stop();
            Console.WriteLine(pi);
            Console.WriteLine(stopWatch.Elapsed);
        });
    }

    private static void DoStep()
    {
        while (Interlocked.Increment(ref currentIterId) is var currentThreadIterId &&
               currentThreadIterId < MAX_ITER_COUNT)
        {
            Console.WriteLine(
                $"Current thread ID: {Environment.CurrentManagedThreadId}. Current iteration: {currentThreadIterId}");

            var addedValue = 1.0 / (currentThreadIterId * 4.0 + 1.0);
            var minusValue = 1.0 / (currentThreadIterId * 4.0 + 3.0);

            lock (piLock)
            {
                pi += addedValue;
                pi -= minusValue;
            }
        }
    }
}