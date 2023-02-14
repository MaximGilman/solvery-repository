using SyncListAccess;


var list = new SyncLinkedList<string>();

for (var j = 0; j < 3; j++)
{
    for (var i = 0; i < 20; i++)
    {
        var i1 = i.ToString();
        var addThread = new Thread(() => list.Add(i1));
        addThread.Start();

    }
}

Console.WriteLine(list);
