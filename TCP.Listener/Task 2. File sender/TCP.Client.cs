using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Utils.Constants;

namespace TCP.Listener.Task_2._File_sender;

internal class FileSenderTcpClient
{
    private ILogger _logger { get; }

    private IPEndPoint _endPoint { get; set; }

    public FileSenderTcpClient(IPAddress targetAddress, int targetPort, ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger<FileSenderTcpClient>();
        _endPoint = new IPEndPoint(targetAddress, targetPort);
    }

    internal async Task HandleSendingFile(string fileName, CancellationToken cancellationToken)
    {
        var client = new TcpClient();

        try
        {
            await client.ConnectAsync(_endPoint, cancellationToken);
            _logger.LogInformation("Connection succeeded");
            await using var networkStream = client.GetStream();
            await using var fileStream = File.OpenRead(fileName);
            // Наверное, стоит отловить переполнение при инициализации буффера?
            var fileBuffer = new byte[fileStream.Length].AsMemory();
            var readedBytes = await fileStream.ReadAsync(fileBuffer, cancellationToken);
            await networkStream.WriteAsync(fileBuffer, cancellationToken);
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
    }
}
