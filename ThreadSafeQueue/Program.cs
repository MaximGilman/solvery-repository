using ThreadSafeQueue;

const int COUNT_OF_READERS = 2, COUNT_OF_WRITERS = 2, MESSAGES_PER_WORKER_COUNT = 15, MESSAGES_LENGTH = 10;
var queue = new ThreadSafeQueue.ThreadSafeQueue();

var readers = CreateWorkers(COUNT_OF_READERS, queue);
var writers = CreateWorkers(COUNT_OF_WRITERS, queue);

StartDropThread(700, queue);
StartReadThreads(readers);
StartWriteThreads(writers);


#region Методы

void StartReadThreads(IEnumerable<QueueWorker> queueWorkers)
{
    foreach (var reader in queueWorkers)
    {
        var thread = new Thread(() => reader.HandleRead());
        thread.Start();
    }
}

void StartWriteThreads(IEnumerable<QueueWorker> queueWorkers)
{
    foreach (var writer in queueWorkers)
    {
        var thread = new Thread(() =>
            writer.HandleWrite(MessageGenerator.GenerateStrings(MESSAGES_PER_WORKER_COUNT, MESSAGES_LENGTH)));
        thread.Start();
    }
}


void StartDropThread(int timeOut, ThreadSafeQueue.ThreadSafeQueue targetQueue)
{
    var dropper = new Thread(() =>
    {
        Thread.Sleep(timeOut);
        targetQueue.Drop();
    });
    dropper.Start();
}

IEnumerable<QueueWorker> CreateWorkers(int numberOfWorkers, ThreadSafeQueue.ThreadSafeQueue targetQueue)
{
    return Enumerable.Range(0, numberOfWorkers).Select(x => new QueueWorker(targetQueue)).ToList();
}

#endregion
