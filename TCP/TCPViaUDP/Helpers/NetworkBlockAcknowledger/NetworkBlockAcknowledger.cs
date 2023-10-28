using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Models.NetworkBlockReceiverResults;

namespace TCPViaUDP.Helpers.NetworkBlockAcknowledger;

public class NetworkBlockAcknowledger : INetworkBlockAcknowledger
{
    private readonly UdpClient _udpClient;
    private readonly TimeSpan _acknowledgmentDelay;

    private readonly Func<DataNetworkBlockResult, Task> _onReceived;
    private readonly Func<EmptyNetworkBlockResult, Task> _onUnreceived;
    private readonly ILogger<INetworkBlockAcknowledger> _logger;
    private int _retryCount;
    private const int MAX_ERROR_RETRY_AMOUNT = 2;

    public NetworkBlockAcknowledger(UdpClient udpClient, TimeSpan acknowledgmentDelay, Func<DataNetworkBlockResult, Task> onReceived,
        Func<EmptyNetworkBlockResult, Task> onUnreceived, ILogger<INetworkBlockAcknowledger> logger)
    {
        _udpClient = udpClient;
        _acknowledgmentDelay = acknowledgmentDelay;
        this._onReceived = onReceived;
        this._onUnreceived = onUnreceived;
        _logger = logger;
    }

    public async Task WaitAcknowledgeAndFire(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync(cancellationToken).AsTask().WaitAsync(this._acknowledgmentDelay, cancellationToken);
                // Отправить сообщение, что получили результат.
                await _onReceived(new DataNetworkBlockResult(result.Buffer));
            }
            catch (TimeoutException)
            {
                // Отправить сообщение, что было получено ничего.
                // Залогировать что переповтор
                await _onUnreceived(new EmptyNetworkBlockResult());
                _logger.LogInformation("За указанное время не было получено подтверждения. Переповторяю отправку.");
            }
            catch (Exception exception)
            {
                _retryCount++;

                if (_retryCount > MAX_ERROR_RETRY_AMOUNT)
                {
                    _logger.LogError("Превышено количество переповторов получения подтвержения с ошибкой {error}", exception.Message);
                    throw;
                }
                else
                {
                    _logger.LogInformation("При получении подтверждения произошла ошибка {error}. Переповторяю отправку {current} из {count}.",
                        exception.Message, _retryCount, MAX_ERROR_RETRY_AMOUNT);
                }
            }
        }
    }
}
