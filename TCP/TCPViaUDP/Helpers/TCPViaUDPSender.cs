using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace TCPViaUDP.Helpers;

public class TCPViaUDPSender: IDisposable
{
    private const int MAX_ON_FLY_WINDOW_SIZE = 2;
    private const int ACKNOWLEDGMENT_SECONDS_DELAY = 5;
    private readonly TimeSpan _acknowledgmentDelay = TimeSpan.FromSeconds(ACKNOWLEDGMENT_SECONDS_DELAY);
    private readonly ILogger<TCPViaUDPSender> _logger;
    private readonly UdpClient _udpClientSend;
    private readonly UdpClient _udpClientReceive;

    private readonly ConcurrentQueue<int> _blockIdsOnFly = new();
    private readonly ConcurrentDictionary<int, Memory<byte>> _blockBodiesOnFly = new();
    private readonly ManualResetEvent _windowOnFlyIsFull = new(true);
    private readonly List<Task> _tasks = new();
    private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Create();
    private int _currentBlockId;
    private object _lockObject = new();

    public TCPViaUDPSender(IPAddress remoteIp, int remotePort, ILogger<TCPViaUDPSender> logger)
        : this(remoteIp, remotePort, remotePort, logger)
    {
    }

    public TCPViaUDPSender(IPAddress remoteIp, int remotePortSend, int remotePortReceive, ILogger<TCPViaUDPSender> logger)
    {
        // TODO: Добавить вариант захвата какого-то свободного порта, как было сделано в TCP. Вынести как отдельный класс эту логику.
        _logger = logger;
        _udpClientSend = new UdpClient(remotePortSend);
        _udpClientSend.Connect(new IPEndPoint(remoteIp, remotePortSend));

        _udpClientReceive = new UdpClient(remotePortReceive);
    }

    public async Task SendAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start waiting acknowledgement messages on {port}", ((IPEndPoint)_udpClientReceive.Client.LocalEndPoint)!.Port);
        _tasks.Add(Task.Run(async () => await AcknowledgeBlockAsync(cancellationToken), cancellationToken));

        await using (fileStream)
        {
            while (true)
            {
                //// Обсудить №1. Просачивается больше потоков, чем указано в окне. Нужно изменить структуру блокировок.
                _windowOnFlyIsFull.WaitOne();
                var bytes = _arrayPool.Rent(ReliableUdpConstants.BYTE_BUFFER_SIZE);
                try
                {
                    // Оставляем в начале блока данных место под номер блока.
                    var readBytes = await fileStream.ReadAsync(bytes, sizeof(int), ReliableUdpConstants.BYTE_BUFFER_DATA_SIZE, cancellationToken);

                    if (readBytes == 0)
                    {
                        _logger.LogInformation("All blocks were read from IO. Waiting for sending & acknowledgment");

                        break;
                    }

                    _tasks.Add(Task.Run(async () => await SendBlockAsync(_currentBlockId, bytes, cancellationToken), cancellationToken));
                }
                catch
                {
                    _arrayPool.Return(bytes);
                }
            }
        }

        await Task.WhenAll(_tasks);
    }

    private async Task SendBlockAsync(int blockId, Memory<byte> blockData, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _currentBlockId);
        this._blockIdsOnFly.Enqueue(blockId);
        var blockDataWithBlockId = AddSequenceNumberToData(blockId, blockData);
        var wasAdded = this._blockBodiesOnFly.TryAdd(blockId, blockDataWithBlockId);

        if (wasAdded)
        {
            _logger.LogInformation("Added block {blockId} to onFly status", blockId);
        }


        await SendNetworkBlockAsync(blockId, blockDataWithBlockId, cancellationToken);
    }

    private async Task ReSendBlockAsync(int blockId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Block with id: {blockId} wasn't acknowledged", blockId);

        this._blockIdsOnFly.Enqueue(blockId);
        this._blockBodiesOnFly.TryGetValue(blockId, out var blockBody);
        var blockDataWithBlockId = AddSequenceNumberToData(blockId, blockBody);
        await SendNetworkBlockAsync(blockId, blockDataWithBlockId, cancellationToken);
    }

    private async Task SendNetworkBlockAsync(int blockId, Memory<byte> blockData, CancellationToken cancellationToken)
    {
        try
        {
            await _udpClientSend.SendAsync(blockData, cancellationToken);
            _logger.LogInformation("Block: {blockId} was sent to network", blockId);
        }
        catch (SocketException)
        {
            _logger.LogError("{Message}", $"Error sending packet {blockId}. Retransmitting...");
        }
        catch (Exception exception)
        {
            _logger.LogError("{Message}", exception.Message);
        }
    }

    private async Task AcknowledgeBlockAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                var result = await _udpClientReceive.ReceiveAsync(cancellationToken).AsTask().WaitAsync(this._acknowledgmentDelay, cancellationToken);
                var receivedInts = ExtractInts(result.Buffer);

                _logger.LogInformation("Received some blockIds that were sent");

                if (IsAllSend(receivedInts))
                {
                    _logger.LogInformation("All blocks were sent");
                    return;
                }

                TryDequeueBlockAndHandleRetransmitting(receivedInts, cancellationToken);
            }
            catch (TimeoutException)
            {
                if (this._blockIdsOnFly.TryDequeue(out var blockId))
                {
                    _tasks.Add(this.ReSendBlockAsync(blockId, cancellationToken));
                }
            }
        }
    }

    private void TryDequeueBlockAndHandleRetransmitting(HashSet<int> receivedInts, CancellationToken cancellationToken)
    {
        if (this._blockIdsOnFly.TryDequeue(out var blockId))
        {
            this._blockBodiesOnFly.TryGetValue(blockId, out var blockBody);

            if (receivedInts.Contains(blockId))
            {
                this._blockBodiesOnFly.Remove(blockId, out _);
                if (this._blockIdsOnFly.Count < MAX_ON_FLY_WINDOW_SIZE)
                {
                    this._windowOnFlyIsFull.Set();
                }

                this._arrayPool.Return(blockBody.ToArray());
            }
            else
            {
                _logger.LogInformation("Block with id: {blockId} wasn't acknowledged", blockId);

                this._blockIdsOnFly.Enqueue(blockId);
                _tasks.Add(this.SendBlockAsync(blockId, blockBody, cancellationToken));
            }

            receivedInts.Remove(blockId);
        }
    }

    /// <summary>
    /// Проверить, получено ли сообщение от получателя.
    /// </summary>
    private static bool IsAllSend(HashSet<int> receivedInts)
    {
        return receivedInts.Count == 1 && receivedInts.First() == -1;
    }

    /// <summary>
    /// Преобразует буффер в хэшсет int'ов - ключей полученных блоков.
    /// </summary>
    private static HashSet<int> ExtractInts(Memory<byte> receivedAcknowledgeData)
    {
        var innerSize = receivedAcknowledgeData.Length / sizeof(int);

        var innerInts = new int[innerSize];
        for (var index = 0; index < innerSize; index++)
        {
            var startOfCurrentInt = index * sizeof(int);
            var endOfCurrentInt = startOfCurrentInt + sizeof(int);

            innerInts[index] = BitConverter.ToInt32(receivedAcknowledgeData[startOfCurrentInt..endOfCurrentInt].Span);
        }

        return innerInts.ToHashSet();
    }

    /// <summary>
    /// Добавляет индекс текущего блока в начало его данных
    /// </summary>
    /// <remarks>Добавляет в начало блока данных int - номер блока. Ожидается, что блок заранее меньше на sizeof(int)</remarks>
    private static Memory<byte> AddSequenceNumberToData(int blockId, Memory<byte> data)
    {
        var resultMemory = new byte[sizeof(int) + data.Length];
        var blockIdBytes = BitConverter.GetBytes(blockId);
        var dataArray = data.ToArray();
        Buffer.BlockCopy(blockIdBytes, 0, resultMemory, 0, blockIdBytes.Length);
        Buffer.BlockCopy(dataArray, 0, resultMemory, blockIdBytes.Length, dataArray.Length);
        return resultMemory;
    }

    public void Dispose()
    {
        _udpClientSend?.Close();
        _udpClientSend?.Dispose();

        _udpClientReceive?.Close();
        _udpClientReceive?.Dispose();

        _windowOnFlyIsFull?.Dispose();
    }
}
