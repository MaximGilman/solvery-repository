using System.Numerics;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Models;

namespace TCPViaUDP.Helpers.ConcurrentWindow;

public abstract class AcknowledgedConcurrentBlockWindow<TKey, TValue> : ConcurrentBlockWindow<TKey, TValue>,
    IAcknowledgedConcurrentBlockWindow<TKey, TValue> where TKey : INumber<TKey>
{
    protected AcknowledgedConcurrentBlockWindow(int windowFrameSize, ILogger<AcknowledgedConcurrentBlockWindow<TKey, TValue>> logger) :
        base(windowFrameSize,
            logger)
    {
    }

    private TKey _lastRemoved = TKey.Zero;

    public override bool TryRemove(TKey blockId)
    {
        lock (_lock)
        {
            // Считаем, что последний подтвержденный ключ (LastAcknowledged) - это ключ, до которого все обработаны.
            // Поэтому, если в окне есть ключ меньше удаляемого - не меняем LastAcknowledged.

            var existedNotAcknowledgedKey = TKey.Min(this._blocksOnFly.Keys.Min(), blockId);

            var wasRemoved = base.TryRemove(blockId);
            if (existedNotAcknowledgedKey == blockId && wasRemoved)
            {
                _lastRemoved = blockId;
            }

            return wasRemoved;
        }
    }

    public virtual TKey GetLastAcknowledged()
    {
        lock (_lock)
        {
            return _lastRemoved;
        }
    }
}
