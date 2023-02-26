namespace UDPCopiesCount.Nodes;

/// <summary>
/// Узел.
/// </summary>
public interface INode
{

    /// <summary>
    /// Имитация работы.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task DoSomeWork(CancellationToken cancellationToken);

}
