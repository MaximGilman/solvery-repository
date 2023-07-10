using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using TCP.Utils;
using Utils.Guards;

namespace TCP.Task2.Listener;

public class DataFlowTcpListener : ITcpListener
{
    private ILogger _logger { get; }
    private readonly TcpExceptionHandler _exceptionHandler;
    private TcpListener _server { get; }

    private readonly ArrayPool<byte> _arrayPool;
    private int _totalTransferredBytes = 0;

    public DataFlowTcpListener(ILoggerFactory loggerFactory) : this(loggerFactory, null)
    {
    }

    public DataFlowTcpListener(ILoggerFactory loggerFactory, int? port)
    {
        this._logger = loggerFactory.CreateLogger<DataFlowTcpListener>();
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
                    // Закомментировал строки, в попытках разобраться.
                    // Вынес явно код первого блока в метод, поскольку при наличии первого блока пуллинг из сети не прекращается.

                    // var networkTransferBlock = CreateNetworkTransferBlock(cancellationToken);
                    var fileActionBlock = CreateFileIOActionBlock(fileStream, cancellationToken);
                    // networkTransferBlock.LinkTo(fileActionBlock, new DataflowLinkOptions { PropagateCompletion = true });

                    this._logger.LogInformation("Start receiving segments");

                    while (true)
                    {
                        // Первый вариант, описанный выше в комментариях
                        // await networkTransferBlock.SendAsync(bytes, cancellationToken);

                        // Второй вариант, без второго блока ниже:
                        // текущее состояние: грузит, работу завершает, но объем файла больше чем нужно.

                        var bytes = _arrayPool.Rent(ITcpListener.BYTE_BUFFER_SIZE);
                        try
                        {
                            var readBytesAmount = await stream.ReadAsync(bytes, cancellationToken);
                            Console.WriteLine($"Received from network: {bytes.Length}");

                            if (readBytesAmount == 0)
                            {
                                fileActionBlock.Complete();
                                this._logger.LogInformation($"Received all segments. Total bytes: {_totalTransferredBytes}");

                                return;
                            }
                            await fileActionBlock.SendAsync(bytes, cancellationToken);
                        }
                        catch
                        {
                            _arrayPool.Return(bytes);
                            throw;
                        }
                    }
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

    private TransformBlock<NetworkStream, byte[]> CreateNetworkTransferBlock(
        CancellationToken cancellationToken)
        =>
            new(async stream =>
            {
                var bytes = _arrayPool.Rent(ITcpListener.BYTE_BUFFER_SIZE);
                try
                {
                    var readBytesAmount = await stream.ReadAsync(bytes, cancellationToken);

                    if (readBytesAmount == 0)
                        throw new OperationCanceledException();
                    return bytes;
                }
                catch
                {
                    _arrayPool.Return(bytes);
                    throw;
                }
            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken
            });

    private ActionBlock<byte[]> CreateFileIOActionBlock(Stream fileStream, CancellationToken cancellationToken) =>
        new(async bytes =>
        {
            if (bytes != null)
            {
                try
                {
                    await fileStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                    _totalTransferredBytes = Interlocked.Add(ref _totalTransferredBytes, bytes.Length);
                }
                finally
                {
                    _arrayPool.Return(bytes);
                }
            }
            else throw new OperationCanceledException();
        }, new ExecutionDataflowBlockOptions
        {
            CancellationToken = cancellationToken
        });
}
