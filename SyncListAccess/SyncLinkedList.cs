using System.Text;

namespace SyncListAccess;

public class SyncLinkedList<T> where T : IComparable
{
    private Node<T> _head;
    private int _count = 0;

    public void Add(T item)
    {
        var node = new Node<T>(item);


        if (_head is null)
        {
            _head = node;
        }
        else
        {
            lock (_head)
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
            return;

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

            lock (current.Mutex)
            lock (next.Mutex)
            {
                if (current.CompareTo(next) > 0)
                {
                    var secondNext = next.Next;
                    next.Next = current;
                    current.Next = secondNext;
                    if (preCurrentItem != default)
                    {
                        preCurrentItem.Next = next;
                    }
                }
            }

            preCurrentItem = current;
            current = next;
            next = next.Next;

            if (next == null)
            {
                preCurrentItem = default;
                current = _head;
                next = current.Next;
                sortCount++;
            }
        }
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        var currentItem = _head;
        Node<T> preCurrentItem = default;

        while (currentItem != null)
        {
            try
            {
                Monitor.Enter(currentItem.Mutex);
                if (preCurrentItem != default && Monitor.IsEntered(preCurrentItem.Mutex))
                {
                    Monitor.Exit(preCurrentItem.Mutex);
                }

                stringBuilder.Append(currentItem);

                preCurrentItem = currentItem;
                currentItem = currentItem.Next;

                if (currentItem != null)
                {
                    Monitor.Enter(currentItem.Mutex);
                }

                Monitor.Exit(preCurrentItem.Mutex);
            }
            finally
            {
                // Monitor.Exit(preCurrentItem.Mutex);
            }
        }


        stringBuilder.Append('X');
        return stringBuilder.ToString();
    }
}