using ThreadSafeQueue;

const int COUNT_OF_READERS = 2,
    COUNT_OF_WRITERS = 2,
    MESSAGES_PER_WORKER_COUNT = 10,
    MESSAGES_LENGTH = 10,
    DROP_TIMEOUT = 1000;

var queue = new ThreadSafeQueue.ThreadSafeQueue();

var readers = CreateWorkers(COUNT_OF_READERS, queue);
var writers = CreateWorkers(COUNT_OF_WRITERS, queue);

var dropThread = StartDropThread(DROP_TIMEOUT, queue);
StartReadThreads(readers);
StartWriteThreads(writers);

dropThread.Join();

#region Методы

void StartReadThreads(IEnumerable<QueueWorker> queueWorkers)
{
    foreach (var reader in queueWorkers)
    {
        var thread = new Thread(() => reader.HandleRead())
        {
            IsBackground = true
        };
        thread.Start();
    }
}

void StartWriteThreads(IEnumerable<QueueWorker> queueWorkers)
{
    foreach (var writer in queueWorkers)
    {
        var thread = new Thread(() =>
            writer.HandleWrite(MessageGenerator.GenerateStrings(MESSAGES_PER_WORKER_COUNT, MESSAGES_LENGTH)))
        {
            IsBackground = true
        };
        thread.Start();
    }
}


Thread StartDropThread(int timeOut, ThreadSafeQueue.ThreadSafeQueue targetQueue)
{
    var dropQueueThread = new Thread(() =>
    {
        Thread.Sleep(timeOut);
        targetQueue.Drop();
    });
    dropQueueThread.Start();

    return dropQueueThread;
}

IEnumerable<QueueWorker> CreateWorkers(int numberOfWorkers, ThreadSafeQueue.ThreadSafeQueue targetQueue)
{
    return Enumerable.Range(0, numberOfWorkers).Select(x => new QueueWorker(targetQueue)).ToList();
}

#endregion
