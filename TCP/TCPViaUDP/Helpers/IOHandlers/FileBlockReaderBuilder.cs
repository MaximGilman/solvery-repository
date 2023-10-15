using Utils.Guards;
using System;
using System.Buffers;
using Microsoft.Extensions.Logging;

namespace TCPViaUDP.Helpers.IOHandlers;

public class FileBlockReaderBuilder
{
    private string _filePath;
    private int _bufferSize;
    private FileShare _fileShareOption = FileShare.None;
    private Func<Memory<byte>, int, Task> _blockActionAsync;
    private ArrayPool<byte> _arrayPool = ArrayPool<byte>.Create();
    private ILogger<FileBlockReaderBuilder> _logger;

    public FileBlockReaderBuilder(ILogger<FileBlockReaderBuilder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Установить путь до файла.
    /// </summary>
    public FileBlockReaderBuilder WithPath(string filePath)
    {
        GuardPath(filePath);
        _filePath = filePath;
        return this;
    }

    /// <summary>
    /// Установить опцию доступа до читаемого файла.
    /// </summary>
    public FileBlockReaderBuilder WithFileShareOption(FileShare fileShareOption)
    {
        _fileShareOption = fileShareOption;
        return this;
    }

    /// <summary>
    /// Установить размер буффера для чтения.
    /// </summary>
    public FileBlockReaderBuilder WithBlockBufferSize(int bufferSize)
    {
        GuardBufferSize(bufferSize);
        _bufferSize = bufferSize;
        return this;
    }


    /// <summary>
    /// Установить действие над блоком, после получения из потока.
    /// </summary>
    public FileBlockReaderBuilder WithBlockAction(Func<Memory<byte>, int, Task> blockActionAsync)
    {
        GuardAction(blockActionAsync);
        _blockActionAsync = blockActionAsync;
        return this;
    }

    public async Task Execute()
    {
        GuardPath(_filePath);
        GuardBufferSize(_bufferSize);
        GuardAction(_blockActionAsync);

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
            catch (Exception exception)
            {
                _logger.LogError("При чтении файла произошло исключение {error}", exception.Message);
            }
            finally
            {
                _arrayPool.Return(buffer);
            }
        }
    }

    private static void GuardPath(string filePath)
    {
        Guard.IsNotNullOrWhiteSpace(filePath);
        Guard.IsTrue(() => File.Exists(filePath), "Файл не существует");
    }

    private static void GuardBufferSize(int bufferSize)
    {
        Guard.IsNotDefault(bufferSize);
        Guard.IsGreaterOrEqual(bufferSize, 0);
        Guard.IsTrue(() => bufferSize % 1024 == 0, "Размер буффера должен быть кратен 1 КБ");
    }

    private static void GuardAction(Func<Memory<byte>, int, Task> blockActionAsync)
    {
        Guard.IsNotDefault(blockActionAsync);
        Guard.IsNotNull(blockActionAsync);
    }
}
