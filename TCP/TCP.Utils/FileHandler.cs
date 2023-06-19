using Utils.Guards;

namespace TCP.Utils;

public static class FileHandler
{
    public static bool TryOpenWriteFile(string filePath, out FileStream fileStream)
    {
        Guard.IsCorrectPath(filePath);
        return TryOpenFile(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, out fileStream);
    }

    public static bool TryOpenReadFile(string filePath, out FileStream fileStream)
    {
        Guard.FileExists(filePath);
        return TryOpenFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, out fileStream);
    }

    private static bool TryOpenFile(string filePath, FileMode mode, FileAccess fileAccess, FileShare fileShare,
        out FileStream fileStream)
    {
        try
        {
            fileStream = File.Open(filePath, mode, fileAccess, fileShare);
            Guard.IsNotDefault(fileStream);
            return true;
        }
        catch
        {
            fileStream = default;
            return false;
        }
    }
}
