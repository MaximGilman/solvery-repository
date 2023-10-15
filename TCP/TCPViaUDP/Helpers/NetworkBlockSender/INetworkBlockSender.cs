using System.Numerics;
using TCPViaUDP.Models;

namespace TCPViaUDP.Helpers.NetworkBlockSender;

/// <summary>
/// Отправитель пакетов в сеть.
/// </summary>
public interface INetworkBlockSender<TKey, TValue> where TKey : INumber<TKey>
{
    /// <summary>
    /// Отправить по сети.
    /// </summary>
    public Task SendAsync(DataBlockWithId<TKey, TValue> dataBlockWithId, CancellationToken cancellationToken);
}
