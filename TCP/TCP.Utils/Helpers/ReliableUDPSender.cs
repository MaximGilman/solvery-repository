using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace TCP.Utils.Helpers;

public class ReliableUdpSender : IDisposable
{
    private const int WINDOW_SIZE_TO_BLOCK = 2;
    private const int SECONDS_TIMEOUT = 1;

    private readonly UdpClient _udpClient;
    private readonly ArrayPool<byte> _arrayPool;
    private readonly ILogger<ReliableUdpSender> _logger;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(SECONDS_TIMEOUT);

    private readonly AutoResetEvent _canSend = new(true);

    // private readonly List<Task> _tasks = new List<Task>();
    private int _currentSequenceNumber;
    private int _baseSequenceNumber;


    private readonly Dictionary<int, Memory<byte>> _packetsOnFly = new();

    public ReliableUdpSender(IPAddress remoteIp, int remotePort, ILogger<ReliableUdpSender> logger)
    {
        // Ловить null порты и занятые порты. Вынести в хелпер
        _logger = logger;
        _udpClient = new UdpClient();
        var remoteEndPoint = new IPEndPoint(remoteIp, remotePort);
        _udpClient.Connect(remoteEndPoint);
        _udpClient.Client.ReceiveTimeout = (int)_timeout.TotalMilliseconds;
        _udpClient.Client.SendTimeout = (int)_timeout.TotalMilliseconds;
        _arrayPool = ArrayPool<byte>.Create();
    }

    public async Task SendAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        await using (fileStream)
        {
            while (true)
            {
                var bytes = _arrayPool.Rent(ReliableUdpConstants.BYTE_BUFFER_SIZE);

                try
                {
                    _canSend.WaitOne();
                    var readBytes = await fileStream.ReadAsync(bytes, 0, ReliableUdpConstants.BYTE_BUFFER_DATA_SIZE, cancellationToken);
                    if (readBytes == 0)
                    {
                        // await Task.WhenAll(_tasks);
                        await SendFinishAsync(cancellationToken);
                        return;
                    }

                    // TODO: оптимизировать отпускание массива внутри метода
                    ThreadPool.QueueUserWorkItem(async _ => await SendAsync(bytes, readBytes, cancellationToken));
                }
                finally
                {
                    _arrayPool.Return(bytes);
                }
            }
        }
    }

    private async Task SendFinishAsync(CancellationToken cancellationToken)
    {
        // МОЖЕТ БЫТЬ ПРОБЛЕМА С _baseSequenceNumber < _nextSequenceNumber
        await SendAsync(ReliableUdpConstants.FinishSequenceMemory, ReliableUdpConstants.FinishSequenceMemory.Length, cancellationToken);
        await WaitForAcknowledgmentAsync(ReliableUdpConstants.FINISH_SEQUENCE_NUMBER, cancellationToken);
    }

    private async Task SendAsync(Memory<byte> data, int readBytes, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Message}", $"Start sending part {_currentSequenceNumber}");
        SetDataOnFly(data, readBytes);
        await SendPacketAsync(_currentSequenceNumber, cancellationToken);

        if (IsCurrentWindowCanSend())
        {
            _canSend.Set();
        }
        else
        {
            _canSend.Reset();
        }
    }

    private void SetDataOnFly(Memory<byte> data, int readBytes)
    {
        var packetData = AddSequenceNumberToData(data, _currentSequenceNumber);
        var dataLength = readBytes + sizeof(int);
        _packetsOnFly[_currentSequenceNumber] = packetData[..dataLength];
    }

    private async Task SendPacketAsync(int sequenceNumber, CancellationToken cancellationToken, bool isRetry = false)
    {
        try
        {
            var data = _packetsOnFly[sequenceNumber];
            await _udpClient.SendAsync(data, cancellationToken);
        }
        catch (SocketException)
        {
            _logger.LogError("{Message}", $"Error sending packet {sequenceNumber}. Retransmitting...");
        }
        catch (Exception exception)
        {
            _logger.LogError("{Message}", exception.Message);
        }
        finally
        {
            ThreadPool.QueueUserWorkItem(async _ => await WaitForAcknowledgmentAsync(sequenceNumber, cancellationToken));

            if (!isRetry)
            {
                _currentSequenceNumber++;
            }
        }
    }

    private static Memory<byte> AddSequenceNumberToData(Memory<byte> data, int sequenceNumber)
    {
        var resultMemory = new Memory<byte>(new byte[sizeof(int) + data.Length]);
        if (BitConverter.TryWriteBytes(resultMemory.Span, sequenceNumber) && data.TryCopyTo(resultMemory))
        {
            return resultMemory;
        }

        throw new SystemException("Error concatenating packet data and its sequence number");
    }

    private async Task WaitForAcknowledgmentAsync(int sequenceNumber, CancellationToken cancellationToken)
    {
        var startTimestamp = DateTime.Now;
        var ackCancellationTokenSource = new CancellationTokenSource();
        var joinedCancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ackCancellationTokenSource.Token);
        while (DateTime.Now - startTimestamp < _timeout)
        {
            UdpReceiveResult result;
            try
            {
                ackCancellationTokenSource.CancelAfter(_timeout);
                result = await _udpClient.ReceiveAsync(joinedCancellationTokenSource.Token);
            }
            catch (SocketException)
            {
                continue;
            }
            catch (OperationCanceledException)
            {
                continue;
            }

            var ackSequenceNumber = BitConverter.ToInt32(result.Buffer, 0);

            if (ackSequenceNumber == sequenceNumber)
            {
                HandleAcknowledgementSuccess(sequenceNumber);
                return;
            }
        }

        HandleAcknowledgementFailed(sequenceNumber, cancellationToken);
    }

    private void HandleAcknowledgementFailed(int sequenceNumber,CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Message}", $"Packet {sequenceNumber} not acknowledged. Retransmitting...");
        ThreadPool.QueueUserWorkItem(async _ => await SendPacketAsync(sequenceNumber, cancellationToken, isRetry: true));
    }

    private void HandleAcknowledgementSuccess(int sequenceNumber)
    {
        _logger.LogInformation("{Message}", $"Packet {sequenceNumber} acknowledged.");
        _packetsOnFly.Remove(sequenceNumber);

        // Нужно так же придумать логику, как двигать seqNumber
        _baseSequenceNumber++;

        if (IsCurrentWindowCanSend())
        {
            _canSend.Set();
        }
    }

    public void Close()
    {
        _udpClient.Close();
    }

    public void Dispose()
    {
        Close();
        _udpClient?.Dispose();
    }

    private bool IsCurrentWindowCanSend() => _currentSequenceNumber - _baseSequenceNumber <= WINDOW_SIZE_TO_BLOCK || _currentSequenceNumber <= WINDOW_SIZE_TO_BLOCK;
}


// Заходят больше чем указано в WINDOW_SIZE_TO_BLOCK
//
