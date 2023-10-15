using System.Numerics;

namespace TCPViaUDP.Helpers.ConcurrentWindow;

public interface IConcurrentDataBlockWindowWithLastAcknowledged<TKey, TValue> : IConcurrentDataBlockWindow<TKey, TValue> where TKey : INumber<TKey>
{
    /// <summary>
    /// Получить последний подтвержденный ключ.
    /// </summary>
    public TKey GetLastAcknowledged();
}
