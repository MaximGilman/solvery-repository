namespace Utils;

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
            throw new ArgumentException($" {expectedValue} не должно быть пустым или равняться null", nameof(expectedValue));
        }
    }

}
