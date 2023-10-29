using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Helpers.DataBlockTransformer;
using TCPViaUDP.Models;
using TCPViaUDP.Models.DataBlocks;

namespace TCPViaUDP.Helpers.NetworkBlockSender;

public class NetworkBlockSender : INetworkBlockSender<long, Memory<byte>>
{
    private readonly UdpClientWrapper.UdpClientWrapper _udpClientWrapper;
    private readonly ILogger<NetworkBlockSender> _logger;

    public NetworkBlockSender(UdpClientWrapper.UdpClientWrapper udpClientWrapper, ILogger<NetworkBlockSender> logger)
    {
        _udpClientWrapper = udpClientWrapper;
        _logger = logger;
    }

    public async Task SendAsync(DataBlockWithId<long, Memory<byte>> dataBlockWithId, CancellationToken cancellationToken)
    {
        try
        {
            var blockWithId = LongKeyMemoryDataBlockTransformer.ToMemory(dataBlockWithId);
            await _udpClientWrapper.UdpClient.SendAsync(blockWithId, cancellationToken);
            _logger.LogInformation("Блок с ключем: {blockId} отправлен в сеть", dataBlockWithId.Id);
        }
        catch (SocketException)
        {
            _logger.LogError("{Message}", $"Произошлка ошибка при отправке блока {dataBlockWithId.Id}");
        }
        catch (Exception exception)
        {
            _logger.LogError("{Message}", exception.Message);
        }
    }
}
