using System.Text.RegularExpressions;

namespace Utils.Guards;

public static partial class Guard
{
    /// <summary>
    /// Проверить, что строка не пустая.
    /// </summary>
    /// <param name="expectedValue">Текущее значение.</param>
    public static void IsNotNullOrWhiteSpace(string expectedValue)
    {
        if (string.IsNullOrWhiteSpace(expectedValue))
        {
            throw new ArgumentException($" {expectedValue} не должно быть пустым или равняться null",
                nameof(expectedValue));
        }
    }

    /// <summary>
    /// Проверить, что строка удовлетворяет маске.
    /// </summary>
    /// <param name="expectedValue">Ожидаемое значение.</param>
    /// <param name="regex">Regex маска.</param>
    public static void IsMatch(string expectedValue, string regex)
    {
        if (!Regex.IsMatch(expectedValue, regex))
        {
            throw new ArgumentException($" {expectedValue} должно соответствовать регулярному выражению {regex}",
                nameof(expectedValue));
        }
    }
}
