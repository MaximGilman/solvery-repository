using System;
using System.Collections.Generic;
using System.Threading;

namespace Threads
{
    public static class Task6SleepSort
    {
        public const int ProportionalityFactor = 50;
        public static void SleepSort(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                var sleepThread = new Thread(() => HandleItem(item));
                sleepThread.Start();
            }
        
        }

        private static void HandleItem(string item)
        {
            Thread.Sleep(item.Length * ProportionalityFactor);
            Console.WriteLine(item);
        }
    }
}