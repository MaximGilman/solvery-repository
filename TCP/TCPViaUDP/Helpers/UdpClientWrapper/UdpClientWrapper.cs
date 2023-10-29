using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCP.Utils;
using Utils.Guards;

namespace TCPViaUDP.Helpers.UdpClientWrapper;

public class UdpClientWrapper
{
    public UdpClient UdpClient { get; init; }
    private readonly ILogger<UdpClientWrapper> _logger;
    private readonly NetworkExceptionHandler _exceptionHandler;

    public UdpClientWrapper(IPAddress remoteIp, int port, ILogger<UdpClientWrapper> logger)
    {
        _logger = logger;
        _exceptionHandler = new NetworkExceptionHandler(_logger);

        Guard.IsValidClientPort(port);
        Guard.IsNotNull(remoteIp);
        UdpClient = new UdpClient(port);
        try
        {
            UdpClient.Connect(new IPEndPoint(remoteIp, port));
        }
        catch (Exception ex)
        {
            _exceptionHandler.HandleException(ex);
        }
    }
}
