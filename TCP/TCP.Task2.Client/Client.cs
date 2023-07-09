using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCP.Utils;

namespace TCP.Task2.Client;

public class Client
{
    private ILogger _logger { get; }
    private readonly TcpExceptionHandler _exceptionHandler;
    private IPEndPoint _endPoint { get; set; }

    private const int BYTE_BUFFER_SIZE = 1024;
    private readonly ArrayPool<byte> _arrayPool;
    private int _totalTransferredBytes = 0;

    public Client(IPAddress targetAddress, int targetPort, ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger<Client>();
        _endPoint = new IPEndPoint(targetAddress, targetPort);
        _arrayPool = ArrayPool<byte>.Create();
        _exceptionHandler = new TcpExceptionHandler(_logger);
    }

    public async Task HandleSendingFile(string fileName, CancellationToken cancellationToken)
    {
        var client = new TcpClient();

        try
        {
            await client.ConnectAsync(_endPoint, cancellationToken);
            _logger.LogInformation("Connection succeeded");
            await using var networkStream = client.GetStream();

            if (!FileHandler.TryOpenReadFile(fileName, out var fileStream))
            {
                this._logger.LogError("Can't open file to read");
                return;
            }

            await using (fileStream)
            {
                while (true)
                {
                    var bytes = _arrayPool.Rent(BYTE_BUFFER_SIZE);

                    try
                    {
                        var readBytes = await fileStream.ReadAsync(bytes, cancellationToken);
                        if (readBytes == 0)
                        {
                            break;
                        }

                        // offset count!!!!
                        await networkStream.WriteAsync(bytes, 0, readBytes, cancellationToken);
                        _totalTransferredBytes = Interlocked.Add(ref _totalTransferredBytes, readBytes);

                    }
                    finally
                    {
                        _arrayPool.Return(bytes);
                    }
                }

                this._logger.LogInformation($"Sent all segments. Total bytes: {_totalTransferredBytes}");
            }
        }
        catch (Exception ex)
        {
            _exceptionHandler.HandleException(ex);
        }
    }
}
