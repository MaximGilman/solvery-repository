using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCP.Utils;
using Utils.Guards;

namespace TCP.Task2.Listener;

public class BaseTcpListener : ITcpListener
{
    private ILogger _logger { get; }
    private readonly TcpExceptionHandler _exceptionHandler;
    private TcpListener _server { get; }

    private readonly ArrayPool<byte> _arrayPool;
    private int _totalTransferredBytes = 0;
    private readonly Stopwatch _watcher;

    public BaseTcpListener(ILoggerFactory loggerFactory) : this(loggerFactory, null)
    {
    }

    public BaseTcpListener(ILoggerFactory loggerFactory, int? port)
    {
        this._logger = loggerFactory.CreateLogger<BaseTcpListener>();
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
        _watcher = new Stopwatch();
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
                    this._logger.LogInformation("Start receiving segments");

                    _watcher.Restart();
                    while (true)
                    {
                        var bytes = _arrayPool.Rent(ITcpListener.BYTE_BUFFER_SIZE);
                        try
                        {
                            var readBytesAmount = await stream.ReadAsync(bytes, cancellationToken);
                            if (readBytesAmount == 0)
                            {
                                break;
                            }

                            await fileStream.WriteAsync(bytes, 0, readBytesAmount, cancellationToken);
                            _totalTransferredBytes = Interlocked.Add(ref _totalTransferredBytes, readBytesAmount);
                        }
                        finally

                        {
                            _arrayPool.Return(bytes);
                        }
                    }

                    break;
                }
            }

            _watcher.Stop();
            this._logger.LogInformation($"Received all segments. Total bytes: {_totalTransferredBytes}." +
                                        $" Total time: {_watcher.Elapsed}");
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
