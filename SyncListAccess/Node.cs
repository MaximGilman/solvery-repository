namespace SyncListAccess;

public class Node<T> : IComparable<Node<T>>
{
    public Node(T data)
    {
        Data = data;
    }

    private T Data { get; }

    public Node<T> Next { get; set; }

    public object Mutex { get; set; } = new();

    public int CompareTo(Node<T> other)
    {
        return Comparer<T>.Default.Compare(Data, other.Data);
    }

    public override string ToString() => Data == null ? string.Empty : $"{Data} - ";
}