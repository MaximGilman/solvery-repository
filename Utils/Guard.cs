namespace Utils;

public static class Guard
{
    /// <summary>
    /// Проверить, что два значения не равны.
    /// </summary>
    /// <typeparam name="T">Тип значения.</typeparam>
    /// <param name="expectedValue">Текущее значение.</param>
    /// <param name="valueToCompare">Значение для сравнения.</param>
    public static void GuardIsNotEqual<T>(T? expectedValue, T? valueToCompare)
    {
        if (expectedValue?.Equals(valueToCompare) == true)
        {
            throw new ArgumentException($" {expectedValue} Не должно быть равно {valueToCompare}");
        }
    }
    
    /// <summary>
    /// Проверить, что значение не равно значению по-умолчанию.
    /// </summary>
    /// <typeparam name="T">Тип значения.</typeparam>
    /// <param name="expectedValue">Текущее значение.</param>
    public static void GuardIsNotDefault<T>(T? expectedValue)
    {
        if (expectedValue?.Equals(default(T)) == true)
        {
            throw new ArgumentException($" {expectedValue} Не должно быть равно значению по умолчанию");
        }
    }
}