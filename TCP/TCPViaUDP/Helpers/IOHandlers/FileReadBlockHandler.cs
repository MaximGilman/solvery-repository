using System.Buffers;
using Microsoft.Extensions.Logging;
using Utils.Guards;

namespace TCPViaUDP.Helpers.IOHandlers;

public class FileReadBlockHandler : IFileReadHandler
{
    private readonly string _filePath;
    private readonly int _bufferSize;
    private readonly FileShare _fileShareOption = FileShare.None;
    private readonly Func<Memory<byte>, int, Task> _blockActionAsync;
    private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Create();
    private ILogger<FileReadBlockHandler> _logger;

    public FileReadBlockHandler(string filePath, int bufferSize, Func<Memory<byte>, int, Task> blockActionAsync, ILogger<FileReadBlockHandler> logger,
        FileShare fileShareOption = FileShare.None)
    {
        Guard.IsNotNullOrWhiteSpace(filePath);
        Guard.IsTrue(() => File.Exists(filePath), "Файл не существует");
        _filePath = filePath;

        Guard.IsNotDefault(bufferSize);
        Guard.IsGreaterOrEqual(bufferSize, 0);
        Guard.IsTrue(() => bufferSize % 1024 == 0, "Размер буффера должен быть кратен 1 КБ");
        _bufferSize = bufferSize;

        Guard.IsNotDefault(blockActionAsync);
        Guard.IsNotNull(blockActionAsync);
        _blockActionAsync = blockActionAsync;

        _logger = logger;

        _fileShareOption = fileShareOption;
    }

    public async Task HandleAsync()
    {
        try
        {
            await using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, _fileShareOption);
            while (true)
            {
                var buffer = _arrayPool.Rent(_bufferSize);
                try
                {
                    var readBytes = await fs.ReadAsync(buffer);

                    if (readBytes == 0)
                    {
                        _logger.LogInformation("Все блоки прочитаны. Завершаю чтение с диска");
                        break;
                    }
                    else
                    {
                        await _blockActionAsync(buffer, readBytes);
                    }
                }
                finally
                {
                    _arrayPool.Return(buffer);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("При чтении файла произошло исключение {error}", exception.Message);

        }
    }
}
