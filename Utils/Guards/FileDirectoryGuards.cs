namespace Utils.Guards;

/// <summary>
/// Валидатор.
/// </summary>
public static partial class Guard
{
    /// <summary>
    /// Проверить, что по пути существует папка.
    /// </summary>
    /// <param name="path">Путь до папки.</param>
    /// <exception cref="ArgumentException">Если папка не существует.</exception>
    public static void DirectoryExists(string path)
    {
        if (!IsCorrectPath(path) || !Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"По пути {path} должна существовать папка.");
        }
    }

    /// <summary>
    /// Проверить, что по указанному пути существует файл.
    /// </summary>
    /// <param name="filePath">Путь до файла.</param>
    /// <exception cref="ArgumentException">Если файл не существует.</exception>
    public static void FileExists(string filePath)
    {
        if (!IsCorrectPath(filePath) || !File.Exists(filePath))
        {
            throw new FileNotFoundException($"По пути {filePath} не найдено указанного файла.", nameof(filePath));
        }
    }

    /// <summary>
    /// Является ли путь корректным.
    /// </summary>
    /// <param name="path">Путь до файла.</param>
    public static bool IsCorrectPath(string path)
    {
        try
        {
            Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
