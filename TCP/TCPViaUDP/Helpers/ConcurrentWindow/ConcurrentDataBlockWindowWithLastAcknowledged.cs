using System.Numerics;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Models;

namespace TCPViaUDP.Helpers.ConcurrentWindow;

public abstract class ConcurrentDataBlockWindowWithLastAcknowledged<TKey, TValue> : ConcurrentDataBlockWindow<TKey, TValue>,
    IConcurrentDataBlockWindowWithLastAcknowledged<TKey, TValue> where TKey : INumber<TKey>
{
    protected ConcurrentDataBlockWindowWithLastAcknowledged(int windowFrameSize, ILogger<ConcurrentDataBlockWindowWithLastAcknowledged<TKey, TValue>> logger) : base(windowFrameSize,
        logger)
    {
    }

    public virtual TKey GetLastAcknowledged() => this._blocksOnFly.Keys.Min() - TKey.One;
}
