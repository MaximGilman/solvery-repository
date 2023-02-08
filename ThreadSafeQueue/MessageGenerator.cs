namespace ThreadSafeQueue;

/// <summary>
/// Генератор строк.
/// </summary>
internal static class MessageGenerator
{
    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private static readonly Random _random = new();

    /// <summary>
    /// Сгенерировать случайную строку.
    /// </summary>
    /// <param name="length">Длина строки</param>
    internal static string GenerateString(int length)
    {
        return new string(Enumerable.Repeat(CHARS, length).Select(s => s[_random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Сгенерировать коллекцию случайных строк.
    /// </summary>
    /// <param name="count">Количество строк в коллекции.</param>
    /// <param name="stringLength">Длина строки - элемента коллекции.</param>
    /// <returns></returns>
    internal static IEnumerable<string> GenerateStrings(int count, int stringLength)
    {
        return Enumerable.Range(0, count).Select(_=> GenerateString(stringLength));

    }
}
