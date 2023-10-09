namespace Utils.Guards;

public static partial class Guard
{
    /// <summary>
    /// Проверить, что память не пустая.
    /// </summary>
    /// <typeparam name="T">Тип элемента коллекции.</typeparam>
    public static void IsNotEmpty<T>(Memory<T> memory)
    {
        if (memory.IsEmpty)
        {
            throw new ArgumentException($"Память не должна быть пустой", nameof(memory));
        }
    }
}
