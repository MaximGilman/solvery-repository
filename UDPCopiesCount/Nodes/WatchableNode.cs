using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NLog;
using Utils.Extensions;
using Utils.Guards;

namespace UDPCopiesCount.Nodes;

internal sealed class WatchableNode
{
    public WatchableNode(Guid id, int port, LogFactory logFactory)
    {
        _logger = logFactory.GetCurrentClassLogger();
        _id = id;
        _port = port;
        _aliveMessage = $"Instance {_id} is alive";

        _siblingsIdsWithAliveTime.AddOrUpdate(_id, DateTime.Now, (_, _) => DateTime.Now);
    }

    private readonly Logger _logger;
    private readonly Guid _id;
    private readonly int _port;
    private readonly string _aliveMessage;

    private static readonly TimeSpan _nodeKeepAliveInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan _siblingListUpdateInterval = TimeSpan.FromSeconds(3);
    private readonly ConcurrentDictionary<Guid, DateTime> _siblingsIdsWithAliveTime = new();

    /// <summary>
    /// Начать цикл отправки статуса.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task StartSendingStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            Memory<byte> data = Encoding.UTF8.GetBytes(this._aliveMessage).AsMemory();
            UdpGuard.IsNoMaxDataSizeExceeded(data.Length);

            var ipEndPoint = new IPEndPoint(IPAddress.Broadcast, this._port);
            using var sender = new UdpClient();
            while (!cancellationToken.IsCancellationRequested)
            {
                await sender.SendAsync(data, ipEndPoint, cancellationToken);
                _logger.Debug("Instance {id} broadcast status to {ip}: {port}", this._id, ipEndPoint.Address,
                    ipEndPoint.Port);
                await Task.Delay(_heartbeatInterval, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);
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
                _logger.Debug("Watcher {id} waiting message on port:{port}", this._id, this._port);
                var result = await receiver.ReceiveAsync(cancellationToken);
                _logger.Debug("Watcher {id} received message on port:{port}", this._id, this._port);

                try
                {
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    var siblingGuid = message.SubstringGuidFromWords();
                    _siblingsIdsWithAliveTime.AddOrUpdate(siblingGuid, DateTime.Now, (_, _) => DateTime.Now);
                    _logger.Debug("Watcher {id} apply that {siblingGuid} is alive", this._id, siblingGuid);
                }
                catch (ArgumentException ex)
                {
                    _logger.Error("Received message was not in the correct format. {message}", ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);
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
                _logger.Debug("Watcher {id} starts to updateAlive nodes", this._id);

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

                _logger.Info("Watcher {id} finished nodes update. Current nodes count: {count}", this._id,
                    this._siblingsIdsWithAliveTime.Count);

                await Task.Delay(_siblingListUpdateInterval, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);
        }
    }

    private static bool IsStillAlive(DateTime lastAliveDateTime) =>
        lastAliveDateTime.Add(_nodeKeepAliveInterval) >= DateTime.Now;
}
