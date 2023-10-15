using Utils.Guards;
using System;

namespace TCPViaUDP.Helpers.IOHandlers;

public class FileBlockReaderBuilder
{
    // logger с исключениями
    private string _filePath;
    private int _bufferSize;
    private FileShare _fileShareOption = FileShare.None;
    private Func<byte[], int, Task> _blockActionAsync;

    public FileBlockReaderBuilder WithPath(string filePath)
    {
        Guard.IsNotNullOrWhiteSpace(filePath);
        Guard.IsTrue(() => File.Exists(filePath), "Файл не существует");
        _filePath = filePath;
        return this;
    }

    public FileBlockReaderBuilder WithFileShareOption(FileShare fileShareOption)
    {
        _fileShareOption = fileShareOption;
        return this;
    }


    public FileBlockReaderBuilder WithBlockBufferSize(int bufferSize)
    {
        Guard.IsNotDefault(bufferSize);
        Guard.IsGreaterOrEqual(bufferSize, 0);
        Guard.IsTrue(() => bufferSize % 1024 == 0, "Размер буффера должен быть кратен 1 КБ");
        _bufferSize = bufferSize;
        return this;
    }

    public FileBlockReaderBuilder WithBlockAction(Func<byte[], int, Task> blockActionAsync)
    {
        Guard.IsNotDefault(blockActionAsync);
        Guard.IsNotNull(blockActionAsync);
        _blockActionAsync = blockActionAsync;
        return this;
    }

    public async Task Сreate()
    {
        // заполнены все настройки guard
        //  добавить arrayPool
        await using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, _fileShareOption);
        byte[] buffer = new byte[_bufferSize];
        int bytesRead;
        while ((bytesRead = await fs.ReadAsync(buffer)) > 0)
        {
            await _blockActionAsync(buffer, bytesRead);
        }
    }
}
