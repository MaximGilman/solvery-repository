using System.Diagnostics;
using System.Runtime.InteropServices;

namespace threads._01._2023;

public class Task8_Pi_parallel_calc
{
    private const int MAX_THREAD_COUNT = 10;
    private static long MAX_ITER_COUNT = 200_000_000;
    private static long currentIterId = 0;
    private static double pi = 1;
    private static object piLock = new object();
    static readonly Stopwatch stopWatch = new Stopwatch();


    public static async Task Execute()
    {
        // В 1 поток. ~ = за 8 секунд.

        // stopWatch.Start();
        // DoStep();
        // stopWatch.Stop();
        // Console.WriteLine(pi * 4);
        // Console.WriteLine(stopWatch.Elapsed);

        // В N потоков - из-за локов время даже дольше - 9 сек. Без партиций 13.

        stopWatch.Restart();
        var tasks = Enumerable.Range(1, MAX_THREAD_COUNT + 1).Select(x => Task.Run(() => DoStep(x)));

        await Task.WhenAll(tasks).ContinueWith(_ =>
        {
            stopWatch.Stop();
            Console.WriteLine(pi * 4);
            Console.WriteLine(stopWatch.Elapsed);
        });
    }

    /// <summary>
    /// Выполнение шага расчета Пи.
    /// </summary>
    /// <param name="threadIndex">Индекс потока среди остальных обработчиков. По нему партицируем инкременты</param>
    private static void DoStep(int threadIndex = 1)
    {
        while (Interlocked.Increment(ref currentIterId) is var currentThreadIterId &&
               currentThreadIterId < MAX_ITER_COUNT / threadIndex)
        {
            // Убрал вывод, т.к. заметно замедляет
            // Console.WriteLine($"Current thread ID: {Environment.CurrentManagedThreadId}. Current iteration: {currentThreadIterId}/{MAX_ITER_COUNT}");

            var addedValue = 1.0 / (currentThreadIterId * 4.0 + 1.0);
            var minusValue = 1.0 / (currentThreadIterId * 4.0 - 1.0);
            lock (piLock)
            {
                pi += addedValue;
                pi -= minusValue;
            }
        }
    }
}