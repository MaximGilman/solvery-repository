﻿namespace Threads;

public static class Task6SleepSort
{
    public const int ProportionalityFactor = 50;
    public static void SleepSort(List<string> items)
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