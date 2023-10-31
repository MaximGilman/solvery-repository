using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Helpers.ConcurrentWindow;
using TCPViaUDP.Helpers.DataBlockTransformer;

namespace TCPViaUDP.Receiver;

public class DataBlockReceiver
{
    private readonly ILogger<DataBlockReceiver> _logger;
    private readonly UdpClient _udpClient;
    private readonly LongKeyMemoryByteAcknowledgedConcurrentBlockWindow _concurrentBlockWindow;

    public DataBlockReceiver(int port, ILoggerFactory loggerFactory)
    {
        _udpClient = new UdpClient(port);

        _logger = loggerFactory.CreateLogger<DataBlockReceiver>();
        _concurrentBlockWindow =
            new LongKeyMemoryByteAcknowledgedConcurrentBlockWindow(int.MaxValue,
                loggerFactory.CreateLogger<LongKeyMemoryByteAcknowledgedConcurrentBlockWindow>());

    }

    public async Task StartHandleAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начало ожидания пакетов от отправителя");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // АПИ Залочилось на сообщение от UDP клиента
                    // ЕСЛИ
                    // спали с ожидания
                    // И
                    // смогли распарсить в блок
                    // И
                    // блок с ИД еще не добавлен в окно
                    // И
                    // блок не был ранее подтвержден и сгружен (в окне его уже не будет, но будет в файле)
                    // ТОГДА
                    // добавляем в окно и считаем блок "полученным", то есть его ключ будет отправляться обратно как подтвержденный
                var result = await _udpClient.ReceiveAsync(cancellationToken);
                var block = LongKeyMemoryDataBlockTransformer.ToBlock(result.Buffer);
                _logger.LogInformation("Блок с id {id} был получен", block.Id);
                _concurrentBlockWindow.TryAddBlock(block);
            }
            catch (Exception exception)
            {
                _logger.LogError("Произошла ошибка {ex}", exception.Message);
            }
        }
    }

    private async Task StartSaving(CancellationToken cancellationToken)
    {
        // Запустить цикл "сгружения" на диск.
            // Открыли поток,
            // Взяли блоки по порядку - сгрузили.
            // Закрываем поток
            // Ждем дальше

        // Каждый раз открываем поток, чтобы не блокировать файл между "сргужениями"
    }

    private async Task StartSendingAcknowledgement(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Запустить цикл обратной связи
                // По таумауту брать минимально полученный ключ по порядку и отправлять его.
                // См логику SequentialBlockSelector.GetSequentialKeysUntilPossible
                // 1 2 3 5 -> 3
                break;
            }

        }
    }
}
