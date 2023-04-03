using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace TCP.Listener.Task_2._File_sender;

internal class TCP_Client
{
    private ILogger _logger { get; }

    private IPEndPoint _endPoint { get; set; }

    public TCP_Client(IPAddress targetAddress, int targetPort, ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger<TCP_Client>();
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
            await fileStream.ReadAsync(fileBuffer, cancellationToken);
            await networkStream.WriteAsync(fileBuffer, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}
