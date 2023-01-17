namespace threads._01._2023;

internal static class Task6SleepSort
{
    private const int ProportionalityFactor = 50;
    internal static void SleepSort(List<string> items)
    {
        items.ForEach(x =>
        {
            var sleepThread = new Thread(() => HandleItem(x));
            sleepThread.Start();
        });
    }

    private static void HandleItem(string item)
    {
        Thread.Sleep(item.Length * ProportionalityFactor);
        Console.WriteLine(item);
    }
}