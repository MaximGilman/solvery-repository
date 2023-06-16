﻿using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCP.Listener.Task_2._File_sender.Utils;
using TCP.Listener.Utils;
using Utils.Constants;
using Utils.Guards;

namespace TCP.Listener.Task_2._File_sender;

internal class FileSenderTcpServer
{
    private ILogger _logger { get; }
    private TcpListener _server { get; }

    private const int BYTE_BUFFER_SIZE = 1024;
    private readonly ArrayPool<byte> _arrayPool;

    internal FileSenderTcpServer(ILoggerFactory loggerFactory, int? port = null)
    {
        this._logger = loggerFactory.CreateLogger<FileSenderTcpServer>();

        if (port.HasValue)
        {
            // Guard.IsValidClientPort(port.Value);
        }
        else
        {
            _logger.LogWarning("Port is not provided. It will be set automatically on HandleReceiveFile()");
        }
        _arrayPool = ArrayPool<byte>.Create();
        _server = TcpListener.Create(port ?? TcpConstants.USE_ANY_FREE_PORT_KEY);
    }

    internal async Task HandleReceiveFile(string fileNameTarget, CancellationToken cancellationToken)
    {
        try
        {
            this._server.Start();
            var ipEndpoint = (IPEndPoint)this._server.LocalEndpoint;
            this._logger.LogInformation("Listener start listening on {address}:{port}", ipEndpoint.Address, ipEndpoint.Port);


            while (!cancellationToken.IsCancellationRequested)
            {
                this._logger.LogInformation("Listener waiting for a connection...");

                using var client = await _server.AcceptTcpClientAsync(cancellationToken);
                this._logger.LogInformation("Tcp client connected");

                var stream = client.GetStream();

                var bytes = _arrayPool.Rent(BYTE_BUFFER_SIZE);
                var memoryBuffer = bytes.AsMemory();
                await using var fileStream = File.OpenWrite(fileNameTarget);

                Guard.IsNotDefault(fileStream);
                var tasks = new List<Task>();
                while (await stream.ReadAsync(memoryBuffer, cancellationToken) != 0)
                {
                    this._logger.LogInformation("Received segment");

                    tasks.Add(Task.Run( async() =>
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        await fileStream.WriteAsync(memoryBuffer, cancellationToken);
                    }, cancellationToken));
                }
                await Task.WhenAll(tasks);
                this._logger.LogInformation("Received all segments");
                _arrayPool.Return(bytes, true);
                break;
            }
        }
        catch (SocketException ex)
        {
            _logger.LogError("Exception: {ex}. {help}", ex.Message, ExceptionHelpConstants.SocketExceptionHelpMessage);
        }
        catch (IOException ex)
        {
            _logger.LogError("Exception: {ex}. {help}", ex.Message, ExceptionHelpConstants.IOExceptionHelpMessage);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError("Exception: {ex}. {help}", ex.Message, ExceptionHelpConstants.OperationCanceledExceptionHelpMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unhandled exception. {ex}", ex.Message);
        }
        finally
        {
            _server.Stop();
            this._logger.LogInformation("Listener server stopped");
        }
    }
}
