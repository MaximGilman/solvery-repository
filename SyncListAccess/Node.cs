namespace SyncListAccess;

public class Node<T> : IComparable<Node<T>>
{
    #region Поля и свойства

    /// <summary>
    /// Значение элемента.
    /// </summary>
    private T _data { get; }

    /// <summary>
    /// Указатель на следующий элемент.
    /// </summary>
    public Node<T> Next { get; set; }

    /// <summary>
    /// Объект блокировки элемента.
    /// </summary>
    public object Mutex { get; }

    #endregion

    #region Конструктор

    public Node(T data)
    {
        _data = data;
        Mutex = new object();
    }

    #endregion

    #region IComparable

    public int CompareTo(Node<T> other)
    {
        return (this, other) switch
        {
            (null, null) => 0,
            (_, null) => 1,
            (null, _) => -1,
            _ => Comparer<T>.Default.Compare(_data, other._data)
        };
    }

    #endregion

    #region Базовый класс

    public override string ToString() => _data == null ? string.Empty : $"{_data} - ";

    public static bool operator >(Node<T> left, Node<T> right) => left.CompareTo(right) > 0;

    public static bool operator <(Node<T> left, Node<T> right) => left.CompareTo(right) < 0;

    #endregion
}
