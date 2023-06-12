using Utils;
using Utils.Extensions;

namespace ThreadSafeQueue;

/// <summary>
/// Обработчик сообщений очереди.
/// </summary>
internal class QueueWorker
{
    #region Константы

    /// <summary>
    /// Максимальная длина строки для вывода.
    /// </summary>
    private const int MAX_DEBUG_MESSAGE_LENGTH = 5;

    /// <summary>
    /// Период ожидания обработчиков.
    /// </summary>
    private const int WAIT_PERIOD = 100;

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Идентификатор обработчика.
    /// </summary>
    private int _id { get; }

    /// <summary>
    /// Очередь для обработки.
    /// </summary>
    private ThreadSafeQueue _queue { get; }

    #endregion

    #region Конструктор

    public QueueWorker(ThreadSafeQueue queue)
    {
        _id = IntSequenceProvider.GetNext();
        _queue = queue;
    }

    #endregion

    #region Методы

    internal void HandleWrite(IEnumerable<string> messages)
    {
        foreach (var message in messages)
        {
            var threadSpecificMessage = $"{_id}_{message}";
            Thread.Sleep(WAIT_PERIOD);
            ConsoleWriter.WriteEvent($"Writer {_id} try to push new value:" +
                                     $" {threadSpecificMessage.CropUpToLength(MAX_DEBUG_MESSAGE_LENGTH)}...");

            var messageLength = _queue.Enqueue(threadSpecificMessage);
            if (messageLength == 0)
            {
                ConsoleWriter.WriteEvent($"Writer {_id} exited due to drop");
                return;
            }

            ConsoleWriter.WriteEvent($"Writer {_id} pushed value with length {messageLength}.");
        }
    }

    internal void HandleRead()
    {
        while (true)
        {
            ConsoleWriter.WriteEvent($"Reader {_id} waiting value.");
            var messageLength = _queue.Dequeue(out var message);
            if (message.Length == 0)
            {
                ConsoleWriter.WriteEvent($"Reader {_id} exited due to drop");
                return;
            }

            ConsoleWriter.WriteEvent(
                $"Reader {_id} read value {message.CropUpToLength(MAX_DEBUG_MESSAGE_LENGTH)}... with length of {messageLength}");
        }
    }

    #endregion
}
