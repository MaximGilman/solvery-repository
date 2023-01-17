using Utils;

namespace Threads;

public static class Task8PiParallelCalc
{
    private const int ThreadCount = 20;
    private const int IterationsCount = 1_000_000;

    public static double CalculatePi(int threadCount = ThreadCount, int iterationCount = IterationsCount)
    {
        Guard.IsNotDefault(threadCount);
        Guard.IsNotDefault(iterationCount);
        
        double pi = 1;

        // Подготовка данных.
        // Даем каждому потоку группу примерно одного количества итераций, без пересечений.
        var workItems = Enumerable.Range(1, threadCount);
        var workingGroups = workItems.Select(x => new
        {
            minValue = CalcGroupNumber(x - 1, threadCount, iterationCount),
            maxValue = CalcGroupNumber(x, threadCount, iterationCount)
        });

        var threads = workingGroups
            .Select(x => new Thread(() => DoStep(x.minValue, x.maxValue, ref pi)))
            .ToList();

        threads.ForEach(x =>
        {
            x.Start();
            x.Join();
        });

        return pi * 4;
    }

    private static void DoStep(int minValue, int maxValue, ref double pi)
    {
        double piValue = 0;
        while (minValue++ < maxValue)
        {
            piValue += 1.0 / (minValue * 4.0 + 1.0);
            piValue -= 1.0 / (minValue * 4.0 - 1.0);
        }

        pi += piValue;
    }

    internal static int CalcGroupNumber(int itemIndex, int threadCount, int iterationCount)
    {
        return itemIndex == threadCount ? iterationCount : iterationCount / threadCount * itemIndex;
    }
}