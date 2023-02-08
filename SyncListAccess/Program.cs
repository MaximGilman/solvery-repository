using SyncListAccess;
using Utils;

const string EXIT_COMMAND = "exit";

Console.WriteLine(@$"
    Многопоточный список.
    - Введите строку, чтобы добавить ее в список
    - Введите {EXIT_COMMAND} чтобы выйти
    - Введите пустую строку, чтобы вывести элементы списка
");
var input = Console.ReadLine();
var list = new SyncLinkedList<string>();

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
    }

    input = Console.ReadLine();
}
