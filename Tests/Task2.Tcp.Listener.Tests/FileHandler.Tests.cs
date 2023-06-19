using System.IO;
using Moq;
using TCP.Utils;
using Xunit;

namespace Task2.Tcp.Listener.Tests;

public class FileHandlerTests
{
    private const string TEST_FILE_PATH = "test.txt";
    private const string NON_EXISTENT_FILE_PATH = "nonexistent.txt";
    private const string LOCKED_FILE_PATH = "locked.txt";

    [Fact]
    public void TryOpenWriteFile_WhenValidPath()
    {

        var result = FileHandler.TryOpenWriteFile(TEST_FILE_PATH, out var fileStream);

        Assert.True(result);
        Assert.NotNull(fileStream);
        fileStream.Dispose();
    }

    [Fact]
    public void TryOpenReadFile_WhenValidPath()
    {
        var result = FileHandler.TryOpenReadFile(TEST_FILE_PATH, out var fileStream);

        Assert.True(result);
        Assert.NotNull(fileStream);
        fileStream.Dispose();
    }

    [Fact]
    public void TryOpenReadFile_WhenFileDoesNotExist()
    {

        Assert.Throws<FileNotFoundException>(() => FileHandler.TryOpenReadFile(NON_EXISTENT_FILE_PATH, out _));
    }

    [Fact(Skip = "Нужно подключить библиотеку для мока статики.")]
    public void TryOpenFile_FileIsLocked_ReturnsFalseAndNullFileStream()
    {
       // Замокать статический File.Open()...

        // Act
        var result = FileHandler.TryOpenWriteFile(LOCKED_FILE_PATH, out var fileStream);

        // Assert
        Assert.False(result);
        Assert.Null(fileStream);
    }
}
