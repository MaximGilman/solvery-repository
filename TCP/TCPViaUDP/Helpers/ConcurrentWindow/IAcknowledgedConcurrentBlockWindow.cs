using System.Numerics;

namespace TCPViaUDP.Helpers.ConcurrentWindow;

public interface IAcknowledgedConcurrentBlockWindow<TKey, TValue> : IConcurrentBlockWindow<TKey, TValue> where TKey : INumber<TKey>
{
    /// <summary>
    /// Получить последний подтвержденный ключ.
    /// </summary>
    public TKey GetLastAcknowledgedKey();
}
