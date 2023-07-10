using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCP.Utils;
using Utils.Guards;

namespace TCP.Task2.Listener;

public class PipelineTcpListener : ITcpListener
{
    private ILogger _logger { get; }
    private readonly TcpExceptionHandler _exceptionHandler;
    private TcpListener _server { get; }

    private int _totalTransferredBytes = 0;

    public PipelineTcpListener(ILoggerFactory loggerFactory) : this(loggerFactory, null)
    {
    }

    public PipelineTcpListener(ILoggerFactory loggerFactory, int? port)
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
                    this._logger.LogInformation("Start receiving segments");


                    var pipe = new Pipe();
                    var writing = FillPipeAsync(stream, pipe.Writer, cancellationToken);
                    var reading = ReadPipeAsync(fileStream, pipe.Reader, cancellationToken);
                    await Task.WhenAll(reading, writing);
                }
                this._logger.LogInformation($"Received all segments. Total bytes: {_totalTransferredBytes}");

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

    async Task FillPipeAsync(NetworkStream stream, PipeWriter writer, CancellationToken cancellationToken)
    {
        while (true)
        {
            var memory = writer.GetMemory(ITcpListener.BYTE_BUFFER_SIZE);
            try
            {
                var readBytesAmount = await stream.ReadAsync(memory, cancellationToken);

                if (readBytesAmount == 0)
                {
                    break;
                }

                writer.Advance(readBytesAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                break;
            }

            var result = await writer.FlushAsync(cancellationToken);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await writer.CompleteAsync();
    }

    async Task ReadPipeAsync(FileStream fileStream, PipeReader reader, CancellationToken cancellationToken)
    {
        while (true)
        {
            var result = await reader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;

            foreach (var part in buffer)
            {
                await fileStream.WriteAsync(part, cancellationToken);
                buffer = buffer.Slice(buffer.GetPosition(part.Length));
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await reader.CompleteAsync();
    }
}
