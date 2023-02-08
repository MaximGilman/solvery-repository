using System.Text;

namespace SyncListAccess;

/// <summary>
/// Многопоточный список.
/// </summary>
/// <typeparam name="T">Тип значений элементов списка.</typeparam>
public class SyncLinkedList<T> where T : IComparable
{
    #region Поля и свойства

    /// <summary>
    /// Головной узел.
    /// </summary>
    private Node<T> _head;

    private object _headObject = new();

    private object _lastMockObject = new();
    /// <summary>
    /// Количество элементов.
    /// </summary>
    private int _count;

    #endregion

    public void Add(T item)
    {
        Monitor.Enter(_headObject);
        var prevHeadObject = _headObject;
        var node = new Node<T>(item);
        _count++;

        if (_head is null)
        {
            _head = node;
            _headObject = _head.Mutex;
        }
        else
        {
            node.Next = _head;
            _head = node;
            _headObject = _head.Mutex;
        }

        Monitor.Exit(prevHeadObject);
    }

    private void CheckNoLock()
    {
        {
            var current = _head;
            while (current != null)
            {
                if (Monitor.IsEntered(current.Mutex)) throw new Exception("Остался залоченн" + current.ToString());
                else
                {
                    current = current.Next;
                }
            }
        }
    }

    public override string ToString()
    {
        // поскольку ссылки на головы меняются - надо после захода в лок брать значение в переменную и ее закрывать

        Monitor.Enter(_headObject);
        var sb = new StringBuilder();

        if (_head?.Next == null)
        {
            Monitor.Exit(_headObject);
            return $"{_head?.ToString() ?? "Empty"}";
        }
        var currentHeadObject = _headObject; // отпустим, когда возьмем следующий
        var currentValue = _head;

        while (currentValue != null)
        {
            sb.Append(currentValue);

            var nextObject = currentValue.Next?.Mutex ?? _lastMockObject;
            Monitor.Enter(nextObject);
            Monitor.Exit(currentHeadObject);
            currentHeadObject = nextObject;
            currentValue = currentValue.Next;

        }
        Monitor.Exit(currentHeadObject);

        return sb.ToString();
    }
}
