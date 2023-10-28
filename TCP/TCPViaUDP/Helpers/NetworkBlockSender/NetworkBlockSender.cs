using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Models;
using TCPViaUDP.Models.DataBlocks;

namespace TCPViaUDP.Helpers.NetworkBlockSender;

public class NetworkBlockSender : INetworkBlockSender<long, Memory<byte>>
{
    private readonly UdpClient _udpClient;
    private readonly ILogger<NetworkBlockSender> _logger;

    public NetworkBlockSender(UdpClient udpClient, ILogger<NetworkBlockSender> logger)
    {
        _udpClient = udpClient;
        _logger = logger;
    }

    public async Task SendAsync(DataBlockWithId<long, Memory<byte>> dataBlockWithId, CancellationToken cancellationToken)
    {
        try
        {
            await _udpClient.SendAsync(dataBlockWithId.Block.Data, cancellationToken);
            _logger.LogInformation("Блок с ключем: {blockId} отправлен в сеть", dataBlockWithId.Id);
        }
        catch (SocketException)
        {
            _logger.LogError("{Message}", $"Произошлка ошибка при отправке блока {dataBlockWithId.Id}. Переотправка...");
        }
        catch (Exception exception)
        {
            _logger.LogError("{Message}", exception.Message);
        }
    }
}
