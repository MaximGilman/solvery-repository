using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Utils.Extensions;

namespace UDPCopiesCount.Nodes;

public class WatcherNode : Node, IWatcherNode
{
    /// <summary>
    /// Время ожидания между обновлениями списка активных узлов.
    /// </summary>
    private const int SIBLINGS_UPDATE_INTERVAL = 1000;


    /// <summary>
    /// Словарь: ИД узла -> время последнего сообщения.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, DateTime> _siblingsWithAliveTime = new();

    /// <summary>
    /// Количество узлов (с учетом себя).
    /// </summary>
    private int _nodesCount => _siblingsWithAliveTime.IsEmpty ? 1 : _siblingsWithAliveTime.Count;

    /// <summary>
    /// Интервал, в котором считаем, что узел еще живой, даже если не получали сообщения.
    /// </summary>
    private const int IS_ALIVE_INTERVAL_SEC = 5;

    public WatcherNode(Guid id, string ipAddress, int port, ILogger<WatcherNode> logger) : base(id, ipAddress, port,
        logger)
    {
        _logger.LogInformation("Instance {id} set as watcher", this._id);
    }

    /// <summary>
    /// Слушать сообщения от соседей.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task StartReceiveStatusAsync(CancellationToken cancellationToken)
    {
        using var receiver = new UdpClient(this._port);
        receiver.JoinMulticastGroup(this._broadcastIpAddress);
        //receiver.MulticastLoopback = false; // отключаем получение своих же сообщений
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Watcher {id} waiting message on {ip}:{port}", this._id, this._broadcastIpAddress, this._port);
            var result = await receiver.ReceiveAsync(cancellationToken);
            _logger.LogInformation("Watcher {id} received message on {ip}:{port}", this._id, this._broadcastIpAddress, this._port);

            try
            {
                string message = Encoding.UTF8.GetString(result.Buffer);
                var siblingGuid = message.SubstringGuid();
                _siblingsWithAliveTime.AddOrUpdate(siblingGuid, DateTime.Now, (key, oldValue) => DateTime.Now);
                _logger.LogInformation("Watcher {id} apply that {siblingGuid} is alive", this._id, siblingGuid);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Received message was not in the correct format. {message}", ex.Message);
            }

        }
    }

    public override async Task DoSomeWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Instance {id} start do some work as watcher", this._id);

        var tasks = new List<Task>
        {
            Task.Run(async () => await StartReceiveStatusAsync(cancellationToken), cancellationToken),
            Task.Run(async () => await base.DoSomeWork(cancellationToken), cancellationToken),
            Task.Run(() =>  StartUpdateIsAlive(CancellationToken.None), cancellationToken)
        };
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Обновить список еще живых узлов.
    /// </summary>
    private void StartUpdateIsAlive(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Watcher {id} starts to updateAlive nodes", this._id);

            var notAliveNodeKeys = _siblingsWithAliveTime.Where(x => !IsStillAlive(x.Value))
                .Select(x => x.Key);

            foreach (var notAliveNodeKey in notAliveNodeKeys)
            {
                // Проверяем узел, может уже ожил.
                if (_siblingsWithAliveTime.TryGetValue(notAliveNodeKey, out var currentAliveDateTime) &&
                    !IsStillAlive(currentAliveDateTime))
                {
                    _siblingsWithAliveTime.TryRemove(notAliveNodeKey, out _);
                }
            }

            _logger.LogInformation("Watcher {id} finished nodes update. Current nodes count: {count}", this._id,
                this._nodesCount);

            Thread.Sleep(SIBLINGS_UPDATE_INTERVAL);
        }
    }

    private static bool IsStillAlive(DateTime lastAliveDateTime) =>
        lastAliveDateTime >= DateTime.Now.AddSeconds(-IS_ALIVE_INTERVAL_SEC);
}
