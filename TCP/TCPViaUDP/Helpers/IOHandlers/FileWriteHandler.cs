namespace TCPViaUDP.Helpers.IOHandlers;

public class FileWriteHandler : IFileWriteHandler
{
    private string _filePath;
    public FileWriteHandler(string filePath)
    {
        // Проверить путь, если доступен

        _filePath = filePath;
    }


    public async Task HandleAsync(Memory<byte> data, CancellationToken cancellationToken)
    {
        // Проверить доступ

        await using var stream = new FileStream(_filePath, FileMode.Append);
        await stream.WriteAsync(data, cancellationToken);
    }
}
