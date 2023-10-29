using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Helpers.DataBlockTransformer;
using TCPViaUDP.Helpers.UdpClientWrapper;

namespace TCPViaUDP.Receiver;

public class DataBlockReceiver
{
    // просто подняться и логгировать что получил с подключения
    private readonly UdpClientWrapper _udpClientWrapper;
    private readonly ILogger<DataBlockReceiver> _logger;
    private readonly UdpClient _udpClient;

    public DataBlockReceiver(int port, ILoggerFactory loggerFactory)
    {
        //_udpClientWrapper = new UdpClientWrapper(IPAddress.Any, port, loggerFactory.CreateLogger<UdpClientWrapper>());
        _udpClient = new UdpClient(port);

        _logger = loggerFactory.CreateLogger<DataBlockReceiver>();
    }

    public async Task StartHandleAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начало ожидания пакетов от отправителя");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync(cancellationToken);
                var block = LongKeyMemoryDataBlockTransformer.ToBlock(result.Buffer);
                _logger.LogInformation("Блок с id {id} был получен", block.Id);
            }
            catch (Exception exception)
            {
                _logger.LogError("Произошла ошибка", exception.Message);
            }
        }
    }
}
