using Utils;


const int messagesCount = 10;
const int handlersCount = 4;
var queue = new ThreadSafeQueue();
var handlers = Enumerable.Range(0, handlersCount).Select(x => new QueueHandler { Id = x, Queue = queue}).ToArray();

var readers = handlers[..2];
var writers = handlers[2..];
var dropper = new Thread(() =>
{
    Thread.Sleep(700);
    queue.Drop();
});
dropper.Start();

foreach (var reader in readers)
{
    var thread = new Thread(() => reader.HandleRead());
    thread.Start();
}

foreach (var writer in writers)
{
    var writerInnerMessages = Enumerable.Range(0, messagesCount).Select(x => x.ToString());
    var thread = new Thread(() => writer.HandleWrite(writerInnerMessages));
    thread.Start();
}


internal class ThreadSafeQueue
{
    private const int MaxLength = 10;
    private const int MessageMaxLength = 80;

    private readonly string[] Items = new string[MaxLength];
    private readonly object LockTarget = new();

    private bool _isDropped = false;
    private int _head, _tail, _count = 0;

    internal int Enqueue(string item)
    {
        if (_isDropped)
        {
            return 0;
        }

        lock (LockTarget)
        {
            var croppedString = item.CropUpToLength(MessageMaxLength);

            while (_count == MaxLength)
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

    internal int Dequeue(out string item)
    {
        if (_isDropped)
        {
            item = string.Empty;
            return 0;
        }

        lock (LockTarget)
        {
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

    internal void Drop()
    {
        _isDropped = true;
        lock (LockTarget)
        {
            Monitor.PulseAll(LockTarget);
        }

        ConsoleWriter.WriteEvent("Queue Dropped!");
    }
}

internal class QueueHandler
{
    internal int Id { private get; init; }
    internal ThreadSafeQueue Queue { get; init; }
    private const int MessageCropLength = 5;

    internal void HandleWrite(IEnumerable<string> messages)
    {
        foreach (var message in messages)
        {
            var threadSpecificMessage = $"{Id}_{message}";
            Thread.Sleep(100);
            ConsoleWriter.WriteEvent(
                $"Writer {Id} try to push new value: {threadSpecificMessage.CropUpToLength(MessageCropLength)}...");
            var messageLength = Queue.Enqueue(threadSpecificMessage);
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
            var messageLength = Queue.Dequeue(out var message);
            if (message.Length == 0)
            {
                ConsoleWriter.WriteEvent($"Reader {Id} exited due to drop");
                return;
            }

            ConsoleWriter.WriteEvent(
                $"Reader {Id} read value {message.CropUpToLength(MessageCropLength)}... with length of {messageLength}");
        }
    }
}