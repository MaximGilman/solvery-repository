using SyncListAccess;


var list = new SyncLinkedList<string>();

for (int i = 0; i < 100; i++)
{
   list.Add(i.ToString());
}

for (var j = 0; j < 100; j++)
{
    for (var i = 0; i < 20; i++)
    {
        var addThread = new Thread(() =>
        {
            list.Sort();
        });
        addThread.Start();
    }
}

