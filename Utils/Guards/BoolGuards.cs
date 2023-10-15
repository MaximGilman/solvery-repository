namespace Utils.Guards;

public partial class Guard
{
    /// <summary>
    /// Проверить, что предикат выполняется.
    /// </summary>
    public static void IsTrue(Func<bool> predicate, string errorMessage)
    {
        if (!predicate())
        {
            throw new ArgumentException(errorMessage);
        }
    }
}
