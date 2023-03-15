var threadPool = new InstanceThreadPool.InstanceThreadPool(3);

var workToDone = Enumerable.Range(1, 100).Select(x=>$"message{x}");

foreach (var workItem in workToDone)
{
    threadPool.Execute(workItem, param =>
    {
        var message = (string)param;
        Console.WriteLine($"Running {message} started");
        Thread.Sleep(100);
        Console.WriteLine($"{message} finished");
    });
}

threadPool.Execute(async () => await MyMethodAsync());

async Task MyMethodAsync()
{
    Console.WriteLine($"Async work");
    await Task.Delay(1000);
    Console.WriteLine($"Async work done.");
}

Console.ReadLine();
