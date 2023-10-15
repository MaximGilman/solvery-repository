using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCP.Utils;
using TCPViaUDP.Helpers.BlockSelectors;
using TCPViaUDP.Helpers.IOHandlers;

namespace TCPViaUDP.Helpers;

public class TCPViaUDPReceiver : IDisposable
{
    private const int ACKNOWLEDGMENT_SECONDS_DELAY = 2;
    private const int FILE_HANDLE_SECONDS_DELAY = 3;

    private readonly TimeSpan _acknowledgmentDelay = TimeSpan.FromSeconds(ACKNOWLEDGMENT_SECONDS_DELAY);
    private readonly TimeSpan _fileHandlerDelay = TimeSpan.FromSeconds(FILE_HANDLE_SECONDS_DELAY);

    private readonly ILogger<TCPViaUDPReceiver> _logger;
    private readonly UdpClient _udpClient;
    private readonly List<Task> _tasks = new();
    private readonly IFileReadHandler _fileHandler;
    private IPEndPoint _targetSenderEndPoint;

    private readonly ConcurrentDictionary<int, Memory<byte>> _receivedBlocks = new();
    private bool _isReceivedAll;

    public TCPViaUDPReceiver(int portReceive, ILogger<TCPViaUDPReceiver> logger)
    {
        _udpClient = new UdpClient(portReceive);
        _logger = logger;

        _fileHandler = default;
    }

    public async Task HandleAllReceiveAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start handling requests on {port}...", ((IPEndPoint)_udpClient.Client.LocalEndPoint)!.Port);

        _tasks.Add(Task.Run(async () => await this.StartSendingAcknowledgementAsync(cancellationToken), cancellationToken));
        _tasks.Add(Task.Run(async () => await StartHandleReceivedBlocks(cancellationToken), cancellationToken));
        while (!_isReceivedAll)
        {
            var result = await _udpClient.ReceiveAsync(cancellationToken);
            this._targetSenderEndPoint ??= result.RemoteEndPoint;
            _tasks.Add(Task.Run(() => HandleBlockAsync(result.Buffer), cancellationToken));
        }

        await Task.WhenAll(_tasks);
    }

    private void HandleBlockAsync(Memory<byte> data)
    {
        var blockId = BitConverter.ToInt32(data[..sizeof(int)].Span);
        if (blockId == -1)
        {
            _logger.LogInformation("All blocks were received");
            this._isReceivedAll = true;
        }
        else
        {
            var blockData = data[sizeof(int)..];
            // Обработать wasAdded
            var wasAdded = this._receivedBlocks.TryAdd(blockId, blockData);
            _logger.LogInformation("Received block with id:{blockId}", blockId);
        }
    }

    private async Task StartSendingAcknowledgementAsync(CancellationToken cancellationToken)
    {
        while (!_isReceivedAll)
        {
            try
            {
                await Task.Delay(_acknowledgmentDelay, cancellationToken);

                if (NeedSendAcknowledgement())
                {
                    //// Обсудить №2.  Можно придумать условие, по которому отправлять не все ключи, а только последние.
                    //// Как вариант в блоке данных хранить еще минимальный сохраненный номер блока. В таком случае, все блоки с ключем меньше этого можно не подтверждать.
                    var receivedKeysAsByteArray = this.GetReceivedKeysData();
                    await _udpClient.SendAsync(receivedKeysAsByteArray, this._targetSenderEndPoint, cancellationToken);
                    _logger.LogInformation("Acknowledge for blocks was sent to {address}:{port}", this._targetSenderEndPoint.Address,
                        this._targetSenderEndPoint.Port);
                }
            }
            catch (SocketException socketException) when (socketException.NativeErrorCode == SocketConstants.NON_EXISTED_IP_ADDRESS_NATIVE_ERROR_CODE)
            {
                // Ignore
            }
            catch (Exception exception)
            {
                _logger.LogError("Acknowledge sending finished with error. {error}", exception.Message);
            }
        }
    }

    private async Task StartHandleReceivedBlocks(CancellationToken cancellationToken)
    {
        while (!_isReceivedAll && _receivedBlocks.Any())
        {
            await Task.Delay(_fileHandlerDelay, cancellationToken);
            await _fileHandler.HandleAsync();
        }
    }

    private bool NeedSendAcknowledgement() => this._targetSenderEndPoint != null && this._receivedBlocks.Any() && IsNewDataReceived();

    private bool IsNewDataReceived() => true;

    private Memory<byte> GetReceivedKeysData()
    {
        var keys = _receivedBlocks.Keys.ToArray();
        return keys.SelectMany(BitConverter.GetBytes).ToArray().AsMemory();
    }

    private IEnumerable<Memory<byte>> PrepareBlocksByKeysToSave(IEnumerable<int> keys)
    {
        var blocksToSave = _receivedBlocks.Where(x => keys.Contains(x.Key)).OrderBy(x => x.Key).Select(x => x.Value);
        foreach (var blockToSaveKey in keys)
        {
            _receivedBlocks.TryRemove(blockToSaveKey, out _);
        }
        return blocksToSave;
    }

    private IEnumerable<int> GetKeys()
    {
        return _receivedBlocks.Keys;
    }

    public void Dispose()
    {
        _udpClient?.Close();
        _udpClient?.Dispose();
    }
}
