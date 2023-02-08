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
    /// Количество элементов.
    /// </summary>
    private int _count;

    #endregion

    #region Методы

    public void Add(T item)
    {
        var node = new Node<T>(item);


        if (_head is null)
        {
            _head = node;
        }
        else
        {
            lock (_head.Mutex)
            {
                var prevHead = _head;
                node.Next = prevHead;
                _head = node;
            }
        }

        _count++;
    }

    public void Sort()
    {
        if (_head?.Next == null)
        {
            return;
        }

        Node<T> preCurrentItem = default;
        var current = _head;
        var next = current.Next;
        var sortCount = 0;

        while (sortCount < _count)
        {
            if (preCurrentItem != default)
            {
                Monitor.Enter(preCurrentItem.Mutex);
            }

            Monitor.Enter(current.Mutex);
            Monitor.Enter(next.Mutex);

            if (current.CompareTo(next) > 0)
            {
                if (preCurrentItem != default)
                {
                    preCurrentItem.Next = next;
                }
                else
                {
                    _head = next;
                }

                var secondNext = next.Next;
                next.Next = current;
                current.Next = secondNext;
            }

            preCurrentItem = current;
            current = next;
            next = next.Next;


            Monitor.Exit(preCurrentItem.Mutex);
            Monitor.Exit(current.Mutex);

            if (next != default)
            {
                Monitor.Exit(next.Mutex);
            }
            // Если прошли до конца.
            if (next == null)
            {
                preCurrentItem = default;
                current = _head;
                next = current.Next;
                sortCount++;
            }
        }
    }

    #endregion

    #region Базовый клас

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        var currentItem = _head;
        Node<T> preCurrentItem = default;

        while (currentItem != null)
        {
            Monitor.Enter(currentItem.Mutex);
            if (preCurrentItem != default && Monitor.IsEntered(preCurrentItem.Mutex))
            {
                Monitor.Exit(preCurrentItem.Mutex);
            }

            stringBuilder.Append(currentItem);

            preCurrentItem = currentItem;
            currentItem = currentItem.Next;

            if (currentItem != default)
            {
                Monitor.Enter(currentItem.Mutex);
            }

            Monitor.Exit(preCurrentItem.Mutex);
        }


        stringBuilder.Append('X');
        return stringBuilder.ToString();
    }

    #endregion
}