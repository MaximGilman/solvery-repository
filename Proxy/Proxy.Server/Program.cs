var threadPool = new InstanceThreadPool.InstanceThreadPool(3);

var cancellationTokenSource = new CancellationTokenSource();
var token = cancellationTokenSource.Token;
token.Register(() => { Console.WriteLine("was canceled"); });


try
{
    threadPool.QueueExecute(null);
}
catch
{
    Console.WriteLine("catched");
}

Task MyCancelableAction(CancellationToken cancellationToken)
{
    while (true)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("stopped");

            return Task.CompletedTask;
        }
        Console.WriteLine("Work");
        Thread.Sleep(1000);
    }
}

threadPool.QueueExecute(async (cancellationToken) => await MyCancelableAction(cancellationToken), token);


Console.ReadLine();
cancellationTokenSource.Cancel();
Console.ReadLine();
