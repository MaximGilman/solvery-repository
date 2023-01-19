using System.Linq;
using System.Threading;
using Utils;

namespace Threads
{
    public static class Task8PiParallelCalc
    {
        private const int ThreadCount = 20;
        private const int IterationsCount = 1_000_000;
        private const int DefaultPiValue = 1;
        public static double CalculatePi(int threadCount = ThreadCount, int iterationCount = IterationsCount)
        {
            Guard.IsNotDefault(threadCount);
            Guard.IsNotDefault(iterationCount);

            var resultArray = new double[threadCount];
            // Подготовка данных.
            // Даем каждому потоку группу примерно одного количества итераций, без пересечений.
            var workItems = Enumerable.Range(1, threadCount);
            var workingGroups = workItems.Select(x => new
            {
                minValue = CalcGroupNumber(x - 1, threadCount, iterationCount),
                maxValue = CalcGroupNumber(x, threadCount, iterationCount)
            });

            var threads = workingGroups
                .Select((x, i) => new Thread(() => DoStep(x.minValue, x.maxValue, out resultArray[i])))
                .ToList();

            threads.ForEach(x =>
            {
                x.Start();
                x.Join();
            });

            return (DefaultPiValue + resultArray.Sum()) * 4;
        }

        private static void DoStep(int minValue, int maxValue, out double pi)
        {
            double piValue = 0;
            while (minValue++ < maxValue)
            {
                piValue += 1.0 / (minValue * 4.0 + 1.0);
                piValue -= 1.0 / (minValue * 4.0 - 1.0);
            }

            pi = piValue;
        }

        private static int CalcGroupNumber(int itemIndex, int threadCount, int iterationCount)
        {
            return itemIndex == threadCount ? iterationCount : iterationCount / threadCount * itemIndex;
        }
    }
}