using System.Diagnostics;

namespace threads._01._2023;

public static class Task8_PiParallelCalc
{
    private const int MAX_THREAD_COUNT = 10;
    private static long MAX_ITER_COUNT = 200_000_000;
    private static long currentIterId = 0;
    private static double pi = 1;
    private static object piLock = new object();
    static readonly Stopwatch stopWatch = new Stopwatch();


    public static async Task Execute()
    {
        Console.WriteLine("Вывод в консоль отключен. Примерное время выполнения с пустой консолью - 10 секунд.");
        // В 1 поток. ~ = за 8 секунд.

        // stopWatch.Start();
        // DoStep();
        // stopWatch.Stop();
        // Console.WriteLine($"Ответ: {pi * 4}");
        // Console.WriteLine($"Потраченное время: {stopWatch.Elapsed}");

        // В N потоков - из-за локов время даже дольше - 9 сек. Без партиций 13.

        stopWatch.Restart();
        var tasks = Enumerable.Range(1, MAX_THREAD_COUNT + 1).Select(x => Task.Run(() => DoStep(x)));

        await Task.WhenAll(tasks).ContinueWith(_ =>
        {
            stopWatch.Stop();
            Console.WriteLine($"Ответ: {pi * 4}");
            Console.WriteLine($"Потраченное время: {stopWatch.Elapsed}");
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