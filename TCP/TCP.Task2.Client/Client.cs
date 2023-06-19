using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCP.Utils;
using Utils.Constants;

namespace TCP.Task2.Client;

public class Client
{
    private ILogger _logger { get; }

    private IPEndPoint _endPoint { get; set; }

    private const int BYTE_BUFFER_SIZE = 1024;
    private readonly ArrayPool<byte> _arrayPool;

    public Client(IPAddress targetAddress, int targetPort, ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger<Client>();
        _endPoint = new IPEndPoint(targetAddress, targetPort);
        _arrayPool = ArrayPool<byte>.Create();
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
                    var fileBuffer = bytes.AsMemory();
                    var readBytes = await fileStream.ReadAsync(fileBuffer, cancellationToken);
                    if (readBytes == 0)
                    {
                        break;
                    }
                    await networkStream.WriteAsync(fileBuffer, cancellationToken);
                    _arrayPool.Return(bytes, true);

                }

                this._logger.LogInformation("Sent all segments");
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
    }
}
