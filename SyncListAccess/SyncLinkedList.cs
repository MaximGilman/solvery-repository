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

    /// <summary>
    /// Хвост списка.
    /// </summary>
    private Node<T> _tail;

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
            lock (_head.Mutex)
            {
                return _count;
            }
        }
    }

    #endregion

    #region Конструктор

    public SyncLinkedList()
    {
        _tail = new Node<T>(default);
        _head = new Node<T>(default);
        ;
        _head.Next = _tail;
    }

    #endregion

    #region Методы

    /// <summary>
    /// Добавить элемент в список.
    /// </summary>
    /// <param name="item">Значение элемента.</param>
    public void Add(T item)
    {
        lock (_head.Mutex)
        {
            var itemNode = new Node<T>(item);

            lock (itemNode.Mutex)
            {
                if (Count == 0)
                {
                    _head = itemNode;
                }
                else
                {
                    itemNode.Next = _head;
                    _head = itemNode;
                }

                _count++;
            }
        }
    }

    #endregion

    #region Переопределения

    public override string ToString()
    {
        var sb = new StringBuilder();
        Monitor.Enter(_head.Mutex);
        var cur = _head;
        Monitor.Enter(cur.Next.Mutex);
        var next = cur.Next;
        while (true)
        {
            sb.Append(cur);
            Monitor.Exit(cur.Mutex);
            cur = next;

            if (next.Next is { } newNext)
            {
                next = newNext;
                Monitor.Enter(newNext.Mutex);
            }
            else
            {
                Monitor.Exit(cur.Mutex);
                break;
            }
        }

        sb.Append('X'); // Конец списка
        return sb.ToString();
    }

    #endregion
}
