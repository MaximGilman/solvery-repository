using SyncListAccess.Lists;
using Utils;

#region Константы

const string EXIT_COMMAND = "exit";
const int SORT_INTERVAL = 5000;
const string START_MESSAGE = @$"
    Многопоточный список.
    - Введите строку, чтобы добавить ее в список
    - Введите {EXIT_COMMAND} чтобы выйти
    - Введите пустую строку, чтобы вывести элементы списка
";

#endregion

#region Методы

void Start(Action action, CancellationToken token)
{
    while (true)
    {
        if (token.IsCancellationRequested)
        {
            Console.WriteLine("Сортировка остановлена. Прервано извне");
            return;
        }

        Thread.Sleep(SORT_INTERVAL);
        action.Invoke();
    }
}

void Run(string input, SyncLinkedList<string> list, CancellationTokenSource cts)
{

    try
    {
        while (!string.Equals(input, EXIT_COMMAND, StringComparison.InvariantCultureIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"Текущее состояние списка: {list}");
            }
            else
            {
                list.Add(input);
                Console.WriteLine($"Элемент {input.CropUpToLength(5)}.. добавлен");
                Console.WriteLine(list.Count);
            }

            input = Console.ReadLine();
        }
    }
    finally
    {
        cts.Cancel();
    }

}

#endregion

#region Поля и свойства

CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
var cancellationToken = cancelTokenSource.Token;

var list = new SyncLinkedList<string>();

var sortThread = new Thread(() => Start(list.Sort, cancellationToken));
sortThread.Start();

#endregion

Console.WriteLine(START_MESSAGE);
Run(Console.ReadLine(), list, cancelTokenSource);