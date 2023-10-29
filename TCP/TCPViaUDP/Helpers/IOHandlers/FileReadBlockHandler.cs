using System.Buffers;
using Microsoft.Extensions.Logging;
using Utils.Guards;
using System;

namespace TCPViaUDP.Helpers.IOHandlers;

public class FileReadBlockHandler : IFileReadHandler
{
    private readonly FileShare _fileShareOption;
    private readonly Func<long, Memory<byte>, CancellationToken, Task> _blockActionAsync;
    private readonly AutoResetEvent _onCanAct;
    private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Create();
    private readonly ILogger<FileReadBlockHandler> _logger;

    public FileReadBlockHandler(Func<long, Memory<byte>, CancellationToken, Task> blockActionAsync, AutoResetEvent onCanAct,
        ILogger<FileReadBlockHandler> logger,
        FileShare fileShareOption = FileShare.None)
    {
        Guard.IsNotDefault(blockActionAsync);
        Guard.IsNotNull(blockActionAsync);
        _blockActionAsync = blockActionAsync;
        _onCanAct = onCanAct;
        _logger = logger;
        _fileShareOption = fileShareOption;
    }

    public async Task HandleAsync(string filePath, int bufferSize, CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(filePath);
        Guard.IsTrue(() => File.Exists(filePath), "Файл не существует");

        Guard.IsNotDefault(bufferSize);
        Guard.IsGreater(bufferSize, 0);

        long blockCounter = 1;
        try
        {
            await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, _fileShareOption);
            while (true)
            {
                _onCanAct.WaitOne();
                var buffer = _arrayPool.Rent(bufferSize);
                try
                {
                    var bufferAsMemory = buffer.AsMemory(0, bufferSize);
                    var readBytes = await fs.ReadAsync(bufferAsMemory, cancellationToken);

                    if (readBytes == 0)
                    {
                        _logger.LogInformation("Все блоки прочитаны. Завершаю чтение с диска");
                        break;
                    }
                    else
                    {
                        var actualData = bufferAsMemory[..readBytes];
                        await _blockActionAsync(blockCounter, actualData, cancellationToken);
                        blockCounter++;
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
