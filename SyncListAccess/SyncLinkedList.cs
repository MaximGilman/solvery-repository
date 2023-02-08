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
}
