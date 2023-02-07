namespace SyncListAccess;

public class Node<T> : IComparable<Node<T>>
{
    public Node(T data)
    {
        Data = data;
        Mutex = new();
    }

    private T Data { get; }

    public Node<T> Next { get; set; }

    public object Mutex { get; set; }

    public int CompareTo(Node<T> other)
    {
        return Comparer<T>.Default.Compare(Data, other.Data);
    }

    public override string ToString() => Data == null ? string.Empty : $"{Data} - ";
}