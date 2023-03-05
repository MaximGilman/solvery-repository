var threadPool = new InstanceThreadPool.InstanceThreadPool(10);

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

Console.ReadLine();
