using System.Buffers;
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

    private const int BYTE_BUFFER_SIZE = 4096 * 8;
    private readonly ArrayPool<byte> _arrayPool;
    private readonly ITcpListenerAsynchronizer _asynchronizer;
    private int _totalTransferredBytes = 0;
    public BaseTcpListener(ILoggerFactory loggerFactory) : this(loggerFactory, null)
    {
    }

    public BaseTcpListener(ILoggerFactory loggerFactory, int? port)
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

        _asynchronizer = new BaseTcpListenerAsynchronizer();

    }

    public async Task HandleReceiveFile(string fileNameTarget, CancellationToken cancellationToken)
    {
        try
        {
            this._server.Start();
            // var ipEndpoint = (IPEndPoint)this._server.LocalEndpoint;
            //this.logger.LogInformation("Listener start listening on {address}:{port}", ipEndpoint.Address,
               // ipEndpoint.Port);


            while (!cancellationToken.IsCancellationRequested)
            {
                //this.logger.LogInformation("Listener waiting for a connection...");

                using var client = await _server.AcceptTcpClientAsync(cancellationToken);
                //this.logger.LogInformation("Tcp client connected");

                var stream = client.GetStream();


                if (!FileHandler.TryOpenWriteFile(fileNameTarget, out var fileStream))
                {
                    //this.logger.LogError("Can't open file to write");
                    return;
                }

                await using (fileStream)
                {
                    await _asynchronizer.Init();
                    //this.logger.LogInformation("Start receiving segments");

                    while (true)
                    {
                        var bytes = _arrayPool.Rent(BYTE_BUFFER_SIZE);
                        try
                        {
                            var readBytesAmount = await stream.ReadAsync(bytes, cancellationToken);
                            if (readBytesAmount == 0)
                            {
                                break;
                            }

                            // Keep for fix AccessToDisposedClosure
                            var localStream = fileStream;
                            await _asynchronizer.QueueWork(async () =>
                            {
                                await localStream.WriteAsync(bytes, cancellationToken);
                                _totalTransferredBytes = Interlocked.Add(ref _totalTransferredBytes, readBytesAmount);
                            }, cancellationToken);
                        }
                        finally

                        {
                            _arrayPool.Return(bytes, true);
                        }

                        await _asynchronizer.GetResult();
                        //this.logger.LogInformation("Received segments");
                    }

                    break;
                }
            }
            this._logger.LogInformation($"Received all segments. Total bytes: {_totalTransferredBytes}");

        }
        catch (Exception ex)
        {
            _exceptionHandler.HandleException(ex);
        }
        finally
        {
            _server.Stop();
            //this.logger.LogInformation("Listener server stopped");
        }
    }
}
