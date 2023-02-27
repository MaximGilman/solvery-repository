using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace UDPCopiesCount.Nodes;

public class Node : INode
{
    public Node(Guid id, string ipAddress, int port, ILogger<Node> logger)
    {
        _broadcastIpAddress = IPAddress.Parse(ipAddress);
        _port = port;
        _logger = logger;
        _id = id;

        _logger.LogInformation("Instance {id} is initialized", this._id);
    }

    protected readonly ILogger<Node> _logger;
    protected Guid _id { get; }
    protected IPAddress _broadcastIpAddress { get; }
    protected int _port { get; }

    private const int HEARTBEAT_INTERVAL = 3000;

    /// <summary>
    /// Начать цикл отправки статуса.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    private async Task StartSendingStatusAsync(CancellationToken cancellationToken)
    {
        var aliveMessage = $"Instance {_id} is alive";
        byte[] data = Encoding.UTF8.GetBytes(aliveMessage);

        using var sender = new UdpClient();
        while (!cancellationToken.IsCancellationRequested)
        {
            await sender.SendAsync(data, new IPEndPoint(this._broadcastIpAddress, this._port), cancellationToken);
            _logger.LogInformation("Instance {id} send status to {ip}: {port}", this._id, this._broadcastIpAddress, this._port);
            Thread.Sleep(HEARTBEAT_INTERVAL);
        }
    }

    /// <summary>
    /// Имитация работы узла.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public virtual async Task DoSomeWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Instance {id} start do some work", this._id);
        await StartSendingStatusAsync(cancellationToken);
        _logger.LogInformation("Instance's {id} work canceled", this._id);
    }
}
