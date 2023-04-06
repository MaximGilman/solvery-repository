using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCP.Listener.Utils;
using Utils.Constants;

internal class TCP_Server
{
    private ILogger _logger { get; }
    private TcpListener _server { get; }

    private const int BYTE_BUFFER_SIZE = 1024;

    internal TCP_Server(ILoggerFactory loggerFactory, int? port = null)
    {
        this._logger = loggerFactory.CreateLogger<TCP_Server>();

        try
        {
            if (port == null)
            {
                throw new ArgumentException("Port is not provided");
            }

            var listener = TcpListener.Create(port.Value);
            if (!TcpPortUtils.IsTcpPortAvailable(port.Value))
            {
                throw new ApplicationException("Port is busy");
            }
            _server = listener;
        }
        catch (ArgumentException)
        {
            _logger.LogWarning("Port is not provided. It will be set automatically on TcpListener.Start()");
            this._server = new TcpListener(new IPEndPoint(IPAddress.Any, port: 0));
        }
        catch
        {
            _logger.LogWarning("Error accured while accessing port: {port}. It will be set automatically on TcpListener.Start()",
                port);
            this._server = new TcpListener(new IPEndPoint(IPAddress.Any, port: 0));
        }
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
                var bytes = new byte[BYTE_BUFFER_SIZE].AsMemory();
                await using var fileStream = File.OpenWrite(fileNameTarget);
                while (await stream.ReadAsync(bytes, cancellationToken) != 0)
                {
                    this._logger.LogInformation("Received segment");

                    await fileStream.WriteAsync(bytes, cancellationToken);
                }
                this._logger.LogInformation("Received all segments");
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
