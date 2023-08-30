﻿using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace TCP.Utils.Helpers;

public class NewVersionOfUdpReceiver
{
    // Дано
    // конкурент справочник с полученными блоками
    // два потока
    // 1.
    //      while true
    //      лочимся на получение сообщений
    //      получаем сообщение
    //      распарсиваем ключ и тело
    //      складываем тело в справочник по ключу
    //      Если пришел последний - начинаем слушать подтверждение о получении от нас (-1)
    // 2.
    //      while true
    //      ждем таймаут
    //      отправляем сообщение со всеми ключами !!!! Тут если не влезет - будет плохо
    //      если пришли все ключи - закрываем соединение

    private readonly ConcurrentDictionary<int, Memory<byte>> _receivedBlocks = new();
    private const int ACKNOWLEDGMENT_SECONDS_DELAY = 2;
    private readonly TimeSpan _acknowledgmentDelay = TimeSpan.FromSeconds(ACKNOWLEDGMENT_SECONDS_DELAY);
    private readonly UdpClient _udpClient;
    private readonly ILogger<NewVersionOfUdpReceiver> _logger;
    private bool _isReceivedAll;
    private readonly List<Task> _tasks = new();

    public NewVersionOfUdpReceiver(int port, ILogger<NewVersionOfUdpReceiver> logger)
    {
        _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, port));
        _logger = logger;
    }

    public async Task HandleAllReceiveAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start handling requests...");

        _tasks.Add(Task.Run(async () => await this.SendAcknowledgementAsync(cancellationToken), cancellationToken));

        while (!_isReceivedAll)
        {
            var result = await _udpClient.ReceiveAsync(cancellationToken);
            _tasks.Add(Task.Run(async () => await HandleBlockAsync(result.Buffer, cancellationToken), cancellationToken));
        }
    }

    private async Task HandleBlockAsync(Memory<byte> data, CancellationToken cancellationToken)
    {
        var blockId = BitConverter.ToInt32(data[..sizeof(int)].Span);
        if (blockId == -1)
        {
            _logger.LogInformation("All blocks were received");
            this._isReceivedAll = true;
        }
        else
        {
            var blockData = data[sizeof(int)..];
            // Обработать wasAdded
            var wasAdded = this._receivedBlocks.TryAdd(blockId, blockData);
            _logger.LogInformation("Received block with id:{blockId}", blockId);

        }
    }

    private async Task SendAcknowledgementAsync(CancellationToken cancellationToken)
    {
        while (!_isReceivedAll)
        {
            await Task.Delay(_acknowledgmentDelay);
            // Можно придумать условие, по которому отправлять не все ключи, а только некоторые
            var receivedKeysAsByteArray = this.GetReceivedKeysData();
            await _udpClient.SendAsync(receivedKeysAsByteArray, cancellationToken);
            _logger.LogInformation("Acknowledge for blocks was sent");
        }
    }

    private Memory<byte> GetReceivedKeysData()
    {
        var keys = _receivedBlocks.Keys.ToArray();
        return keys.SelectMany(BitConverter.GetBytes).ToArray().AsMemory();
    }
}
