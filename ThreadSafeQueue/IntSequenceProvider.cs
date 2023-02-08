namespace ThreadSafeQueue;

/// <summary>
/// Генератор последовательных уникальных чисел.
/// </summary>
public static class IntSequenceProvider
{
    #region Поля и свойства

    /// <summary>
    /// Текущее значение.
    /// </summary>
    private static int _currentValue { get; set; } = 1;

    /// <summary>
    /// Объект синхронизации.
    /// </summary>
    private static readonly object _lock = new();

    #endregion

    #region Методы

    /// <summary>
    /// Получить следующее значение
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">При переполнении типа.</exception>
    internal static int GetNext()
    {
        lock (_lock)
        {
            if (_currentValue < int.MaxValue)
            {
                return _currentValue++;
            }
            else
            {
                throw new ArgumentOutOfRangeException( nameof(_currentValue),"Значение вышло за границы типа.");
            }

        }
    }

    #endregion
}
