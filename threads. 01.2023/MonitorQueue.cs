using Utils;

namespace Threads;

internal static class MonitorQueue
{
    private const int MessagesCount = 10;

    private const int HandlersCount = 4;

    internal static void Execute()
    {
        var handlers = Enumerable.Range(0, HandlersCount).Select(x => new QueueHandler { Id = x }).ToArray();

        var readers = handlers[..2];
        var writers = handlers[2..];
        var dropper = new Thread(() =>
        {
            Thread.Sleep(700);
            ThreadSafeQueue.Drop();
        });
        dropper.Start();

        foreach (var reader in readers)
        {
            var thread = new Thread(() => reader.HandleRead());
            thread.Start();
        }

        foreach (var writer in writers)
        {
            var writerInnerMessages = Enumerable.Range(0, MessagesCount).Select(x => x.ToString());
            var thread = new Thread(() => writer.HandleWrite(writerInnerMessages));
            thread.Start();
        }


    }
}

internal static class ThreadSafeQueue
{
    private const int MaxLength = 10;
    private const int MessageMaxLength = 80;

    private static readonly string[] Items = new string[MaxLength];
    private static readonly object LockTarget = new();

    private static bool _isDropped = false;
    private static int _head, _tail, _count = 0;

    internal static int Enqueue(string item)
    {

        lock (LockTarget)
        {
            if (_isDropped)
            {
                Monitor.PulseAll(LockTarget);
                return 0;
            }

            var croppedString = item.CropUpToLength(MessageMaxLength);

            if (_count == MaxLength)
            {
                Monitor.Wait(LockTarget);
            }

            if (_count == 1)
            {
                Monitor.PulseAll(LockTarget);
            }

            _head = (_head + 1) % MaxLength;
            Items[_head] = croppedString;
            _count++;
            return croppedString.Length;
        }
    }

    internal static int Dequeue(out string item)
    {

        lock (LockTarget)
        {
            if (_isDropped)
            {
                item = string.Empty;
                Monitor.PulseAll(LockTarget);
                return 0;
            }

            if (_count == MaxLength)
            {
                Monitor.PulseAll(LockTarget);
            }

            if (_count == 0)
            {
                Monitor.Wait(LockTarget);
            }

            _tail = (_tail + 1) % MaxLength;

            item = Items[_tail];
            _count--;
            return item.Length;
        }

    }

    internal static void Drop()
    {
        _isDropped = true;
        ConsoleWriter.WriteEvent("Queue Dropped!");
    }
}

internal class QueueHandler
{
    internal int Id { private get; init; }

    private const int MessageCropLength = 5;
    internal void HandleWrite(IEnumerable<string> messages)
    {
        foreach (var message in messages)
        {
            Thread.Sleep(100);
            ConsoleWriter.WriteEvent($"Writer {Id} pushing new value: {message.CropUpToLength(MessageCropLength)}...");
            var messageLength = ThreadSafeQueue.Enqueue(message);
            if (messageLength == 0)
            {
                ConsoleWriter.WriteEvent($"Writer {Id} exited due to drop");
                return;
            }

            ConsoleWriter.WriteEvent($"Writer {Id} pushed value with length {messageLength}.");

        }
    }

    internal void HandleRead()
    {
        while (true)
        {
            ConsoleWriter.WriteEvent($"Reader {Id} waiting value.");
            var messageLength = ThreadSafeQueue.Dequeue(out var message);
            ConsoleWriter.WriteEvent(
                $"Reader {Id} read value {message.CropUpToLength(MessageCropLength)}... with length of {messageLength}");

            if (message.Length == 0)
            {
                ConsoleWriter.WriteEvent($"Reader {Id} exited due to drop");
                return;
            }
        }
    }
}