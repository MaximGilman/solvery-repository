namespace Utils.Guards;

/// <summary>
/// Валидатор.
/// </summary>
public static partial class Guard
{
    /// <summary>
    /// Проверить, что по пути существует папка.
    /// </summary>
    /// <param name="path"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void DirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new ArgumentException($"По пути {path} должна существовать папка.",
                nameof(path));
        }
    }
}
