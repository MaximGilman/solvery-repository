using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace TCP.Utils.Helpers;

public class ReliableUdpReceiver : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly ILogger<ReliableUdpReceiver> _logger;

    public ReliableUdpReceiver(int localPort, ILogger<ReliableUdpReceiver> logger)
    {
        _logger = logger;
        _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, localPort));
    }

    public async Task StartReceivingAsync()
    {
        _logger.LogInformation("Receiver started.");

        while (true)
        {
            var result = await _udpClient.ReceiveAsync();
            var sequenceNumber = BitConverter.ToInt32(result.Buffer, 0);

            _logger.LogInformation("{Message}", $"Received packet with sequence number: {sequenceNumber}");
            await SendAcknowledgmentAsync(sequenceNumber, result.RemoteEndPoint);
        }
    }

    private async Task SendAcknowledgmentAsync(int sequenceNumber, IPEndPoint remoteEndPoint)
    {
        byte[] acknowledgmentData = BitConverter.GetBytes(sequenceNumber);
        await _udpClient.SendAsync(acknowledgmentData, acknowledgmentData.Length, remoteEndPoint);
    }

    public void Close()
    {
        _udpClient.Close();
    }

    public void Dispose()
    {
        _udpClient.Close();
        _udpClient?.Dispose();
    }
}
