using System.Text;

namespace SyncListAccess;
// TODO: Переделать head и Tail

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
    private Node<T> _sentinelHead;

    /// <summary>
    /// Хвост списка.
    /// </summary>
    private Node<T> _sentinelTail;

    /// <summary>
    /// Количество элементов.
    /// </summary>
    private int _count;

    /// <summary>
    /// Количество элементов.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_sentinelHead.Mutex)
            {
                lock (_sentinelHead.Next.Mutex)
                {
                    return _count;
                }
            }
        }
    }

    #endregion

    #region Конструктор

    public SyncLinkedList()
    {
        _sentinelTail = new Node<T>(default);
        _sentinelHead = new Node<T>(default)
        {
            Next = _sentinelTail
        };
    }

    #endregion

    #region Методы

    /// <summary>
    /// Добавить элемент в список.
    /// </summary>
    /// <param name="item">Значение элемента.</param>
    public void Add(T item)
    {
        lock (_sentinelHead.Mutex)
        {
            var itemNode = new Node<T>(item);

            lock (itemNode.Mutex)
            {
                if (Count == 0)
                {
                    _sentinelHead.Next = itemNode;
                    itemNode.Next = _sentinelTail;
                }
                else
                {
                    itemNode.Next = _sentinelHead.Next;
                    _sentinelHead.Next = itemNode;
                }

                _count++;
            }
        }
    }


    /// <summary>
    /// Сортировать список по возрастанию
    /// </summary>
    public void Sort()
    {
        var sortCount = this.Count;
        if (sortCount < 2)
        {
            return;
        }

        for (var i = 0; i <= sortCount; i++)
        {
            Monitor.Enter(_sentinelHead.Mutex);
            Monitor.Enter(_sentinelHead.Next.Mutex);
            Monitor.Enter(_sentinelHead.Next.Next.Mutex);

            var prev = _sentinelHead;
            var current = _sentinelHead.Next;
            var next = _sentinelHead.Next.Next;

            while (next != _sentinelTail)
            {
                var outgoingNext = next.Next;

                if (current.CompareTo(next) > 0)
                {
                    prev.Next = next;
                    next.Next = current;
                    current.Next = outgoingNext;
                }

                Monitor.Enter(outgoingNext.Mutex);

                var outgoingPrev = prev;
                prev = prev.Next;
                current = prev.Next;
                next = current.Next;
                Monitor.Exit(outgoingPrev.Mutex);
            }

            Monitor.Exit(prev.Mutex);
            Monitor.Exit(current.Mutex);
            Monitor.Exit(next.Mutex);
        }
    }

    #endregion

    #region Базовый класс

    public override string ToString()
    {
        lock (_sentinelHead.Mutex)
        {
            lock (_sentinelHead.Next.Mutex)
            {
                switch (this.Count)
                {
                    case 0:
                        return "Empty";
                    case 1:
                    {
                        return $"{_sentinelHead.Next}X";
                    }
                }
            }
        }

        var sb = new StringBuilder();
        Monitor.Enter(_sentinelHead.Mutex);
        Monitor.Enter(_sentinelHead.Next.Mutex);

        var current = _sentinelHead.Next;

        Monitor.Exit(_sentinelHead.Mutex);
        Monitor.Enter(current.Next.Mutex);
        var next = current.Next;
        while (next != _sentinelTail)
        {
            sb.Append(current);
            Monitor.Exit(current.Mutex);
            current = next;
            Monitor.Enter(next.Next.Mutex);
            next = next.Next;
        }

        sb.Append(current);

        Monitor.Exit(current.Mutex);
        Monitor.Exit(_sentinelTail.Mutex);

        sb.Append('X'); // Конец списка
        return sb.ToString();
    }

    #endregion
}
