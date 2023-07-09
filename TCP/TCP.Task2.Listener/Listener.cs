using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCP.Utils;
using Utils.Constants;
using Utils.Guards;

namespace TCP.Task2.Listener;

public class Listener
{
    private ILogger _logger { get; }
    private readonly TcpExceptionHandler _exceptionHandler;
    private TcpListener _server { get; }

    private const int BYTE_BUFFER_SIZE = 1024;
    private readonly ArrayPool<byte> _arrayPool;

    public Listener(ILoggerFactory loggerFactory) : this(loggerFactory, null)
    {
    }

    public Listener(ILoggerFactory loggerFactory, int? port)
    {
        this._logger = loggerFactory.CreateLogger<Listener>();
        _exceptionHandler = new TcpExceptionHandler(_logger);

        if (port.HasValue)
        {
            Guard.IsValidClientPort(port.Value);
        }
        else
        {
            _logger.LogWarning("Port is not provided. It will be set automatically on HandleReceiveFile()");
        }

        _arrayPool = ArrayPool<byte>.Create();
        _server = TcpListener.Create(port ?? TcpConstants.USE_ANY_FREE_PORT_KEY);
        _exceptionHandler = new TcpExceptionHandler(_logger);
    }

    public async Task HandleReceiveFile(string fileNameTarget, CancellationToken cancellationToken)
    {
        try
        {
            this._server.Start();
            var ipEndpoint = (IPEndPoint)this._server.LocalEndpoint;
            this._logger.LogInformation("Listener start listening on {address}:{port}", ipEndpoint.Address,
                ipEndpoint.Port);


            while (!cancellationToken.IsCancellationRequested)
            {
                this._logger.LogInformation("Listener waiting for a connection...");

                using var client = await _server.AcceptTcpClientAsync(cancellationToken);
                this._logger.LogInformation("Tcp client connected");

                var stream = client.GetStream();


                if (!FileHandler.TryOpenWriteFile(fileNameTarget, out var fileStream))
                {
                    this._logger.LogError("Can't open file to write");
                    return;
                }

                await using (fileStream)
                {
                    var tasks = new List<Task>();
                    this._logger.LogInformation("Start receiving segments");

                    while (true)
                    {
                        var bytes = _arrayPool.Rent(BYTE_BUFFER_SIZE);
                        try
                        {
                            var memoryBuffer = bytes.AsMemory();
                            var readBytesAmount = await stream.ReadAsync(memoryBuffer, cancellationToken);
                            if (readBytesAmount == 0)
                            {
                                break;
                            }

                            // Keep for fix AccessToDisposedClosure
                            var localStream = fileStream;
                            tasks.Add(Task.Run(async () =>
                            {
                                await localStream.WriteAsync(memoryBuffer, cancellationToken);
                            }, cancellationToken));
                        }
                        finally

                        {
                            _arrayPool.Return(bytes);
                        }

                        await Task.WhenAll(tasks);
                        this._logger.LogInformation("Received all segments");
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _exceptionHandler.HandleException(ex);
        }
        finally
        {
            _server.Stop();
            this._logger.LogInformation("Listener server stopped");
        }
    }
}
