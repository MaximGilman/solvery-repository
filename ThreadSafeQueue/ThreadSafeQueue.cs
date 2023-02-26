using Utils;
using Utils.Extensions;
using Utils.Guards;
namespace ThreadSafeQueue;

/// <summary>
/// Многопоточная очередь.
/// </summary>
internal class ThreadSafeQueue
{
    #region Константы

    /// <summary>
    /// Максимальная длина очереди
    /// </summary>
    private const int MAX_LENGTH = 10;

    /// <summary>
    /// Максимальная длина сообщения в очереди.
    /// </summary>
    private const int MESSAGE_MAX_LENGTH = 80;

    #endregion

    private readonly string[] _items = new string[MAX_LENGTH];
    private readonly object _lockTarget = new();

    private bool _isDropped;
    private int _head, _tail, _count;

    #region Методы

    /// <summary>
    /// Добавить элемент в очередь.
    /// </summary>
    /// <param name="item">Значение элемента.</param>
    internal int Enqueue(string item)
    {
        Guard.IsNotNullOrWhiteSpace(item);
        lock (_lockTarget)
        {
            if (_isDropped)
            {
                return 0;
            }

            while (_count == MAX_LENGTH)
            {
                Monitor.Wait(_lockTarget);
            }

            if (_isDropped)
            {
                return 0;
            }

            if (_count == 1)
            {
                Monitor.PulseAll(_lockTarget);
            }

            _head = (_head + 1) % MAX_LENGTH;
            _items[_head] = item.CropUpToLength(MESSAGE_MAX_LENGTH);
            _count++;
            return _items[_head].Length;
        }
    }

    /// <summary>
    /// Считать элемент из очереди.
    /// </summary>
    /// <param name="item">Возвращаемое значение.</param>
    internal int Dequeue(out string item)
    {
        lock (_lockTarget)
        {
            if (_isDropped)
            {
                item = string.Empty;
                return 0;
            }

            if (_count == MAX_LENGTH)
            {
                Monitor.PulseAll(_lockTarget);
            }

            while (_count == 0)
            {
                Monitor.Wait(_lockTarget);
            }

            if (_isDropped)
            {
                item = string.Empty;
                return 0;
            }

            _tail = (_tail + 1) % MAX_LENGTH;

            item = _items[_tail];
            _count--;
            return item.Length;
        }
    }

    internal void Drop()
    {
        lock (_lockTarget)
        {
            if (_isDropped)
            {
                return;
            }

            _isDropped = true;

            Monitor.PulseAll(_lockTarget);
            ConsoleWriter.WriteEvent("Queue Dropped!");
        }
    }

    #endregion
}
