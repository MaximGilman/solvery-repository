using Microsoft.Extensions.Logging;
using TCPViaUDP.Helpers.ConcurrentWindow;
using TCPViaUDP.Helpers.IOHandlers;
using TCPViaUDP.Models.DataBlocks;
using Utils.Constants;

namespace TCPViaUDP.Sender;

public class DataBlockSender
{
    private readonly LongKeyMemoryByteAcknowledgedConcurrentBlockWindow _blockWindow;
    private readonly FileReadBlockHandler _fileReadHandler;
    private readonly ILogger<DataBlockSender> _logger;

    // вынести куда-то
    private const int WINDOW_SIZE = 2;

    public DataBlockSender(LoggerFactory loggerFactory)
    {
        _blockWindow = new LongKeyMemoryByteAcknowledgedConcurrentBlockWindow(WINDOW_SIZE,
            loggerFactory.CreateLogger<LongKeyMemoryByteAcknowledgedConcurrentBlockWindow>());
        _fileReadHandler = new FileReadBlockHandler(HandleBlockAction, _blockWindow.OnCanAdd, loggerFactory.CreateLogger<FileReadBlockHandler>());
        _logger = loggerFactory.CreateLogger<DataBlockSender>();
    }

    public async Task Start(string path)
    {
        await _fileReadHandler.HandleAsync(path, NetworkConstants.MTU_DATA_BLOCK_MAX_BYTE_SIZE);
    }

    private async Task HandleBlockAction(long id, Memory<byte> data)
    {
        var block = new LongKeyMemoryByteDataBlock(id, data);

        if (!_blockWindow.TryAddBlock(block))
        {
            // Гипотетическая ситуация. Мы прошли сюда, значит
            // 1. за собой закрыли состояние в не сигнальное - то есть других потоков не будет,
            // 2. в окне есть место (раз зашли).

            // Причин не добавиться нет. Явно бросим исключение, чтобы заметить, при эксплуатации.
            throw new ApplicationException(
                $"Произошла ситуация, которая считалась гипотетической. После входа в крит. секцию по AutoResetEvent не получилось добавить блок с ID {id}");
        }
    }
}
