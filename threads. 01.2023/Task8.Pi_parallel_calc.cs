using System.Diagnostics;

namespace threads._01._2023;

public class Task8_PiParallelCalc
{
    private const int MAX_THREAD_COUNT = 50;
    private const long MAX_ITER_COUNT = 200_000_000;
    private static object locked = new();

    public static async Task Execute()
    {
        double pi = 1;

        // В 1 поток. ~ = за 8 секунд.

        var workItems = Enumerable.Range(1, MAX_THREAD_COUNT);
        var workingGroups =
            workItems.Select((x, i) => new { minValue = CalcGroupNumber(x - 1), maxValue = CalcGroupNumber(x) });

        // Thread'ы создаются в foreground - их можно не ждать.
        // x - maxValue
        var threads = workingGroups.Select(x => new Thread(() => DoStep(x.minValue, x.maxValue, ref pi)))
            .ToList();
        threads.ForEach(x =>
        {
            x.Start();
            x.Join();
        });

        // Thread.Sleep(10000);
        Console.WriteLine(pi * 4);
    }

    private static void DoStep(int minValue, int maxValue, ref double pi)
    {
        double piValue = 0;
        while (minValue++ < maxValue)
        {
            piValue += 1.0 / (minValue * 4.0 + 1.0);
            piValue -= 1.0 / (minValue * 4.0 - 1.0);
        }

        ;


        pi += piValue;
    }

    private static int CalcGroupNumber(int itemIndex) => (int)(MAX_ITER_COUNT / MAX_THREAD_COUNT) * (itemIndex);
    private static bool IsInGroup(int itemIndex, int groupNumber) => itemIndex <= groupNumber;
}