using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Helpers.ConcurrentWindow;
using TCPViaUDP.Helpers.IOHandlers;
using TCPViaUDP.Helpers.NetworkBlockAcknowledger;
using TCPViaUDP.Helpers.NetworkBlockSender;
using TCPViaUDP.Helpers.UdpClientWrapper;
using TCPViaUDP.Models.DataBlocks;
using TCPViaUDP.Models.NetworkBlockReceiverResults;
using Utils.Constants;

namespace TCPViaUDP.Sender;

public class DataBlockSender
{
    private readonly LongKeyMemoryByteAcknowledgedConcurrentBlockWindow _blockWindow;
    private readonly FileReadBlockHandler _fileReadHandler;
    private readonly ILogger<DataBlockSender> _logger;
    private readonly NetworkBlockSender _networkBlockSender;
    private readonly UdpClientWrapper _udpClientWrapper;
    private readonly INetworkBlockAcknowledger _blockAcknowledger;

    // вынести куда-то
    private const int WINDOW_SIZE = 2;

    public DataBlockSender(IPAddress remoteIp, int port, ILoggerFactory loggerFactory)
    {
        _blockWindow = new LongKeyMemoryByteAcknowledgedConcurrentBlockWindow(WINDOW_SIZE,
            loggerFactory.CreateLogger<LongKeyMemoryByteAcknowledgedConcurrentBlockWindow>());
        _fileReadHandler = new FileReadBlockHandler(HandleBlockAction, _blockWindow.OnCanAdd, loggerFactory.CreateLogger<FileReadBlockHandler>());
        _logger = loggerFactory.CreateLogger<DataBlockSender>();

        _udpClientWrapper = new UdpClientWrapper(remoteIp, port, loggerFactory.CreateLogger<UdpClientWrapper>());
        _networkBlockSender = new NetworkBlockSender(_udpClientWrapper, loggerFactory.CreateLogger<NetworkBlockSender>());

        // тут другой поди клиент?
        // ожидание динамически
        _blockAcknowledger = new NetworkBlockAcknowledger(_udpClientWrapper.UdpClient, TimeSpan.FromSeconds(5), 5, this.OnAcknowledgeReceived, this.OnAcknowledgeUnreceived,
            loggerFactory.CreateLogger<NetworkBlockAcknowledger>());
    }

    public async Task StartHandleAsync(string path, CancellationToken cancellationToken)
    {
        await _fileReadHandler.HandleAsync(path, NetworkConstants.MTU_DATA_BLOCK_MAX_BYTE_SIZE, cancellationToken);
        await Task.Run(async () => await _blockAcknowledger.WaitAcknowledgeAndFire(cancellationToken), cancellationToken);
    }

    private async Task HandleBlockAction(long id, Memory<byte> data, CancellationToken cancellationToken)
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

        // Отправить в сеть
        await _networkBlockSender.SendAsync(block, cancellationToken);
    }

    private async Task OnAcknowledgeUnreceived(EmptyNetworkBlockResult arg, CancellationToken cancellationToken)
    {
        var (firstOnFlyBlockKey, firstOnFlyBlockDataData) = _blockWindow.GetFirstValueOrDefault();
        if (firstOnFlyBlockKey != default)
        {
            // Отдать минимальный ключ на переотправку.
            var block = new LongKeyMemoryByteDataBlock(firstOnFlyBlockKey, firstOnFlyBlockDataData);
            _logger.LogInformation("Блок с ключем {id} не был подтвержден принимающей стороной за указанное время. Переповтор", firstOnFlyBlockKey);

            await _networkBlockSender.SendAsync(block, cancellationToken);
        }

        throw new NotImplementedException();
    }

    private Task OnAcknowledgeReceived(AcknowledgeNetworkBlockResult arg, CancellationToken cancellationToken)
    {
        var lastAcknowledgedKey = arg.GetValue();
        if (lastAcknowledgedKey > _blockWindow.GetLastAcknowledgedKey())
        {
            _logger.LogInformation("Блок с ключем {id} был подтвержден принимающей стороной", lastAcknowledgedKey);
            _blockWindow.TryRemoveUntil(lastAcknowledgedKey);
        }
        return Task.CompletedTask;
    }
}
