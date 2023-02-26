namespace UDPCopiesCount.Nodes;

public interface IWatcherNode
{
    /// <summary>
    /// Слушать сообщения от соседей.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task StartReceiveStatusAsync(CancellationToken cancellationToken);
}
