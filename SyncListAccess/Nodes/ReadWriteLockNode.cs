namespace SyncListAccess;

public class ReadWriteLockNode<T> : Node<T>
{
    /// <summary>
    /// Объект блокировки элемента.
    /// </summary>
    public ReadWriteLock Lock { get; }

    /// <summary>
    /// Указатель на следующий элемент.
    /// </summary>
    public ReadWriteLockNode<T> Next { get; set; }


    public ReadWriteLockNode(T data) : base(data)
    {
        this.Lock = new ReadWriteLock();
    }
}
