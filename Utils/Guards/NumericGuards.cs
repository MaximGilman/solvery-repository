namespace Utils.Guards;

public partial class Guard
{
    /// <summary>
    /// Проверить, что значение строго больше 0.
    /// </summary>
    public static void IsGreaterZero(int first)
    {
        IsGreater(first, default);
    }

    /// <summary>
    /// Проверить, что значение строго больше 0.
    /// </summary>
    public static void IsGreaterZero(double first)
    {
        IsGreater(first, default);
    }

    /// <summary>
    /// Проверить, что значение строго больше 0.
    /// </summary>
    public static void IsGreaterZero(float first)
    {
        IsGreater(first, default);
    }

    /// <summary>
    /// Проверить, что значение строго больше 0.
    /// </summary>
    public static void IsGreaterZero(decimal first)
    {
        IsGreater(first, default);
    }
}
