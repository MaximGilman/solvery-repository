namespace SyncListAccess.Nodes;

public abstract class Node<T>: IComparable<Node<T>>
{
    #region Поля и свойства

    /// <summary>
    /// Значение элемента.
    /// </summary>
    private T _data { get; }

    #endregion

    #region Конструктор

    protected Node(T data)
    {
        _data = data;
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

    #endregion
}
