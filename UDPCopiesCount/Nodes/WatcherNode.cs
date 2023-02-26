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
    private const int SIBLINGS_UPDATE_SLEEP_TIMEOUT = 1000;


    /// <summary>
    /// Словарь: ИД узла -> время последнего сообщения.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, DateTime> _siblingsWithAliveTime = new();

    /// <summary>
    /// Количество узлов (с учетом себя).
    /// </summary>
    public int NodesCount => _siblingsWithAliveTime.Count + 1;

    /// <summary>
    /// Интервал, в котором считаем, что узел еще живой, даже если не получали сообщения.
    /// </summary>
    private const int IS_ALIVE_INTERVAL_SEC = 5;

    public WatcherNode(Guid id, string ipAddress, int port, ILogger<WatcherNode> logger) : base(id, ipAddress, port,
        logger)
    {
        _logger.LogInformation("Instance {id} set as watcher", this._id);
        Task.Run(() => StartUpdateIsAlive(CancellationToken.None));
    }

    /// <summary>
    /// Слушать сообщения от соседей.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task StartReceiveStatusAsync(CancellationToken cancellationToken)
    {
        using var receiver = new UdpClient(this._port);
        receiver.JoinMulticastGroup(this._broadcastIpAddress); // Возникнет ошибка. МБ из-за локальной тачки
        receiver.MulticastLoopback = false; // отключаем получение своих же сообщений

        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Watcher {id} receiving message from {ip}:{port}", this._id,
                this._broadcastIpAddress, this._port);
            var result = await receiver.ReceiveAsync(cancellationToken);
            _logger.LogInformation("Watcher {id} received message from {ip}:{port}", this._id, this._broadcastIpAddress,
                this._port);

            string message = Encoding.UTF8.GetString(result.Buffer);
            var siblingGuid = message.SubstringGuid();

            _siblingsWithAliveTime.AddOrUpdate(siblingGuid, DateTime.Now, (key, oldValue) => DateTime.Now);
            _logger.LogInformation("Watcher {id} apply that {siblingGuid} is alive", this._id, siblingGuid);
        }
    }

    public override async Task DoSomeWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Instance {id} start do some work as watcher", this._id);

        await StartReceiveStatusAsync(cancellationToken);
        await base.DoSomeWork(cancellationToken);
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
                this.NodesCount);

            Thread.Sleep(SIBLINGS_UPDATE_SLEEP_TIMEOUT);
        }
    }

    private static bool IsStillAlive(DateTime lastAliveDateTime) =>
        lastAliveDateTime >= DateTime.Now.AddSeconds(-IS_ALIVE_INTERVAL_SEC);
}
