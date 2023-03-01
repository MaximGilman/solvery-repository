using System.Text;

namespace SyncListAccess.Lists;

/// <summary>
/// Многопоточный список на RW-lock.
/// </summary>
/// <typeparam name="T">Тип значений элементов списка.</typeparam>
public class SyncRwLinkedList<T> where T : IComparable
{
    #region Поля и свойства

    /// <summary>
    /// Головной узел.
    /// </summary>
    private readonly ReadWriteLockNode<T> _sentinelHead;

    /// <summary>
    /// Количество элементов.
    /// </summary>
    private int _count;

    public int Count
    {
        get
        {
            _sentinelHead.Lock.AcquireReaderLock();
            var count = _count;
            _sentinelHead.Lock.ReleaseReaderLock();
            return count;
        }
    }
    #endregion

    #region Конструктор

    public SyncRwLinkedList()
    {
        _sentinelHead = new ReadWriteLockNode<T>(default);
    }

    #endregion

    #region Методы

    /// <summary>
    /// Добавить элемент в список.
    /// </summary>
    /// <param name="item">Значение элемента.</param>
    public void Add(T item)
    {
        _sentinelHead.Lock.AcquireWriterLock();

        var itemNode = new ReadWriteLockNode<T>(item);

        itemNode.Lock.AcquireWriterLock();

        if (_count == 0)
        {
            _sentinelHead.Next = itemNode;
        }
        else
        {
            itemNode.Next = _sentinelHead.Next;
            _sentinelHead.Next = itemNode;
        }

        _count++;

        _sentinelHead.Lock.ReleaseWriterLock();
        itemNode.Lock.ReleaseWriterLock();
    }

    #endregion

    #region Базовый класс

    public override string ToString()
    {
        _sentinelHead.Lock.AcquireReaderLock();
        if (this._count == 0)
        {
            _sentinelHead.Lock.ReleaseReaderLock();
            return "Empty";
        }

        _sentinelHead.Next.Lock.AcquireReaderLock();
        if (this._count == 1)
        {
            _sentinelHead.Lock.ReleaseReaderLock();
            _sentinelHead.Next.Lock.AcquireReaderLock();
            return $"{_sentinelHead.Next}X";
        }

        var sb = new StringBuilder();
        var current = _sentinelHead.Next;
        var next = current.Next;
        while (next != null)
        {
            sb.Append(current);
            current.Lock.ReleaseReaderLock();
            current = next;
            if (next.Next != null)
            {
                next.Lock.AcquireReaderLock();
            }

            next = next.Next;
        }

        sb.Append(current);

        current.Lock.ReleaseReaderLock();
        sb.Append('X'); // Конец списка
        return sb.ToString();
    }

    #endregion
}
