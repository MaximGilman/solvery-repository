
namespace threads._01._2023;

internal static class Task8PiParallelCalc
{
    private const int ThreadCount = 20;
    private const int IterationsCount = 200_000_000;

    internal static double CalculatePi()
    {
        double pi = 1;
        var workItems = Enumerable.Range(1, ThreadCount);
        var workingGroups = workItems.
                Select(x => new { minValue = CalcGroupNumber(x - 1), maxValue = CalcGroupNumber(x) });
        
        var threads = workingGroups.
                Select(x => new Thread(() => DoStep(x.minValue, x.maxValue, ref pi)))
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
        };

        pi += piValue;
    }

    private static int CalcGroupNumber(int itemIndex) =>
        itemIndex == ThreadCount ? IterationsCount : IterationsCount / ThreadCount * (itemIndex);

    private static bool IsInGroup(int itemIndex, int groupNumber) => itemIndex <= groupNumber;
}