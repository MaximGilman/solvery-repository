namespace threads._01._2023;

static class Program
{
    public static async Task Main()
    {
        // 8e задание
       Console.WriteLine(Task8PiParallelCalc.CalculatePi());

        var randomStrings = Enumerable.Range(1, 100).Select(x => string.Join("",Enumerable.Repeat($"{x}", 100 / x))).ToList();
        Task6SleepSort.SleepSort(randomStrings);
    }
}

