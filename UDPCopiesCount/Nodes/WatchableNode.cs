using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Utils.Extensions;

namespace UDPCopiesCount.Nodes;

internal sealed class WatchableNode
{
    public WatchableNode(Guid id, int port, ILogger<WatchableNode> logger)
    {
        _logger = logger;
        _id = id;
        _port = port;
    }

    private readonly ILogger<WatchableNode> _logger;
    private Guid _id { get; }
    private int _port { get; }
    private int NodesInBroadcastCount => _siblingsIdsWithAliveTime.IsEmpty ? 1 : _siblingsIdsWithAliveTime.Count;

    private const int IS_NODE_STILL_ALIVE_INTERVAL = 5000;
    private const int HEARTBEAT_INTERVAL = 1000;
    private const int SIBLING_LIST_UPDATE_INTERVAL = 3000;
    private readonly ConcurrentDictionary<Guid, DateTime> _siblingsIdsWithAliveTime = new();

    /// <summary>
    /// Начать цикл отправки статуса.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task StartSendingStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            var aliveMessage = $"Instance {_id} is alive";
            byte[] data = Encoding.UTF8.GetBytes(aliveMessage);
            var ipEndPoint = new IPEndPoint(IPAddress.Broadcast, this._port);
            using var sender = new UdpClient();
            while (!cancellationToken.IsCancellationRequested)
            {
                await sender.SendAsync(data, ipEndPoint, cancellationToken);
                _logger.LogDebug("Instance {id} broadcast status to {ip}: {port}", this._id, ipEndPoint.Address,
                    ipEndPoint.Port);
                await Task.Delay(HEARTBEAT_INTERVAL, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    /// <summary>
    /// Начать цикл прослушки статуса.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task StartReceiveStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var receiver = new UdpClient();
            receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            receiver.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Watcher {id} waiting message on port:{port}", this._id, this._port);
                var result = await receiver.ReceiveAsync(cancellationToken);
                _logger.LogDebug("Watcher {id} received message on port:{port}", this._id, this._port);

                try
                {
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    var siblingGuid = message.SubstringGuid();
                    _siblingsIdsWithAliveTime.AddOrUpdate(siblingGuid, DateTime.Now, (_, _) => DateTime.Now);
                    _logger.LogDebug("Watcher {id} apply that {siblingGuid} is alive", this._id, siblingGuid);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError("Received message was not in the correct format. {message}", ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    /// <summary>
    /// Обновить список еще живых узлов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task StartUpdateIsAlive(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Watcher {id} starts to updateAlive nodes", this._id);

                var notAliveNodeKeys = _siblingsIdsWithAliveTime.Where(x => !IsStillAlive(x.Value))
                    .Select(x => x.Key);

                foreach (var notAliveNodeKey in notAliveNodeKeys)
                {
                    // Проверяем узел, может уже ожил.
                    if (_siblingsIdsWithAliveTime.TryGetValue(notAliveNodeKey, out var currentAliveDateTime) &&
                        !IsStillAlive(currentAliveDateTime))
                    {
                        _siblingsIdsWithAliveTime.TryRemove(notAliveNodeKey, out _);
                    }
                }

                _logger.LogInformation("Watcher {id} finished nodes update. Current nodes count: {count}", this._id,
                    this.NodesInBroadcastCount);

                await Task.Delay(SIBLING_LIST_UPDATE_INTERVAL, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    private static bool IsStillAlive(DateTime lastAliveDateTime) =>
        lastAliveDateTime >= DateTime.Now.AddMilliseconds(-IS_NODE_STILL_ALIVE_INTERVAL);
}
