namespace SyncListAccess;

public class FineGrainedNode<T>: Node<T>
{
    /// <summary>
    /// Объект блокировки элемента.
    /// </summary>
    public object Mutex { get; }

    /// <summary>
    /// Указатель на следующий элемент.
    /// </summary>
    public FineGrainedNode<T> Next { get; set; }

    public FineGrainedNode(T data) : base(data)
    {
        this.Mutex = new object();
    }
}
