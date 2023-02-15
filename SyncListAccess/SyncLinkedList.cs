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


    /// <summary>
    /// Сортировать список по возрастанию
    /// </summary>
    public void Sort()
    {
        if (this.Count < 2)
        {
            return;
        }

        var sortCount = this.Count;
        for (var i = 0; i <= sortCount; i++)
        {
            // Лочим именно 2 раза, т.к. Head - это реальный первый элемент, prev для него нет.
            Monitor.Enter(_head.Mutex);
            Monitor.Enter(_head.Mutex);
            Monitor.Enter(_head.Next.Mutex);
            // На первом элементе предыдущий - это он сам.
            var prev = _head;
            var current = _head;
            var next = _head.Next;
            while (next != null)
            {
                if (current > next)
                {
                    var outgoingNext = next.Next;

                    if (current == _head)
                    {
                        _head = next;
                    }
                    else
                    {
                        prev.Next = next;
                    }

                    next.Next = current;
                    current.Next = outgoingNext;

                    Monitor.Exit(prev.Mutex);
                    Monitor.Exit(current.Mutex);
                    Monitor.Exit(next.Mutex);

                    // Поскольку мы поменяли порядок, передвигать не нужно, только восстановить prev и next.
                    prev = next;
                    next = current.Next;
                }
                else
                {
                    Monitor.Exit(prev.Mutex);
                    Monitor.Exit(current.Mutex);
                    Monitor.Exit(next.Mutex);

                    prev = current;
                    current = current.Next;
                    next = next.Next;
                }


                Monitor.Enter(prev.Mutex);
                Monitor.Enter(current.Mutex);
                if (next is { } _)
                {
                    Monitor.Enter(next.Mutex);
                }
            }

            Monitor.Exit(prev.Mutex);
            Monitor.Exit(current.Mutex);
        }
    }

    #endregion

    #region Базовый класс

    public override string ToString()
    {
        if (this.Count  == 1)
        {
            return $"{_head}X";
        }

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
                sb.Append(cur);
                Monitor.Exit(cur.Mutex);
                break;
            }
        }

        sb.Append('X'); // Конец списка
        return sb.ToString();
    }

    #endregion
}
