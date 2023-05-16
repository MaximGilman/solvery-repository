var threadPool = new InstanceThreadPool.InstanceThreadPool(3);

var tks = new CancellationTokenSource();
var tkn = tks.Token;
tkn.Register(() => { Console.WriteLine("was canceled"); });


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

threadPool.QueueExecute(async (cancellationToken) => await MyCancelableAction(cancellationToken), tkn);


Console.ReadLine();
tks.Cancel();
Console.ReadLine();
