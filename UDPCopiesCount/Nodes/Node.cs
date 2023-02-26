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
    protected Guid _id { get;  }
    protected IPAddress _broadcastIpAddress { get; }
    protected int _port { get; }

    private const int WORK_SLEEP_TIMEOUT = 3000;
    protected async Task SendStatusAsync(string message, CancellationToken cancellationToken)
    {
        using var sender = new UdpClient();
        byte[] data = Encoding.UTF8.GetBytes(message);

         await sender.SendAsync(data, new IPEndPoint(this._broadcastIpAddress, this._port), cancellationToken);
        _logger.LogInformation("Instance {id} send status to {ip}: {port}", this._id, this._broadcastIpAddress, this._port);
    }

    public virtual async Task DoSomeWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Instance {id} start do some work", this._id);

        var aliveMessage = $"Instance {_id} is alive";
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Instance {id} doing work", this._id);
            await SendStatusAsync(aliveMessage, cancellationToken);
            Thread.Sleep(WORK_SLEEP_TIMEOUT);
        }

        _logger.LogInformation("Instance's {id} work canceled", this._id);

    }
}
