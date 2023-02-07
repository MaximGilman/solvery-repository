using System.Diagnostics.Contracts;
using SyncListAccess;
using Utils;

const string exitCommand = "exit";

Console.WriteLine(@$"
    Многопоточный список.
    - Введите строку, чтобы добавить ее в список
    - Введите {exitCommand} чтобы выйти
    - Введите пустую строку, чтобы вывести элементы списка
");
var input = Console.ReadLine();
var isExited = false;
var list = new SyncLinkedList<string>();
var sortHandler = new Thread(() =>
{
    while (!isExited)
    {
        Thread.Sleep(5000);
        list.Sort();
    }
});

sortHandler.Start();


while (!string.Equals(input, exitCommand, StringComparison.InvariantCultureIgnoreCase))
{
    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Текущее состояние списка: " +
                          $"{list}");
    }
    else
    {
        list.Add(input);
        Console.WriteLine($"Элемент {input.CropUpToLength(5)}.. добавлен");
    }

    input = Console.ReadLine();
}
isExited = false;