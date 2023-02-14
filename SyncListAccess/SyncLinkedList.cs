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
            lock (_head)
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
        var itemNode = new Node<T>(item);
        lock (itemNode.Mutex)
        {
            lock (_head.Mutex)
            {
                var prevHead = _head;
                itemNode.Next = prevHead;
                _head = itemNode;
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
        var prev = _head;
        Monitor.Enter(prev.Next.Mutex);
        var current = prev.Next;
        while (current != _tail)
        {
            sb.Append(prev);
            Monitor.Exit(prev.Mutex);
            prev = current;
            current = current.Next;
            Monitor.Enter(current.Mutex);
        }

        sb.Append('X'); // Конец списка
        Monitor.Exit(current.Mutex);
        return sb.ToString();
    }

    #endregion
}
