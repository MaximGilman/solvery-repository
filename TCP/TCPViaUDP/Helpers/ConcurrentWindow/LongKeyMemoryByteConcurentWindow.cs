using Microsoft.Extensions.Logging;
using TCPViaUDP.Models;
using TCPViaUDP.Models.DataBlocks;
using Utils.Guards;

namespace TCPViaUDP.Helpers.ConcurrentWindow;

public class LongKeyMemoryByteConcurrentWindow : ConcurrentDataBlockWindowWithLastAcknowledged<long, Memory<byte>>
{
    public LongKeyMemoryByteConcurrentWindow(int windowFrameSize, ILogger<ConcurrentDataBlockWindowWithLastAcknowledged<long, Memory<byte>>> logger) : base(windowFrameSize, logger)
    {
    }

    public override bool TryAddBlock(DataBlockWithId<long, Memory<byte>> blockWithId)
    {
        Guard.IsNotDefault(blockWithId);

        Guard.IsNotDefault(blockWithId.BlockId);
        Guard.IsGreater(blockWithId.BlockId, 0);

        Guard.IsNotDefault(blockWithId.Data);
        Guard.IsNotNull(blockWithId.Data);
        Guard.IsNotEmpty(blockWithId.Data);

        return base.TryAddBlock(blockWithId);
    }

    public override bool TryRemove(long blockId)
    {
        Guard.IsNotDefault(blockId);
        Guard.IsGreater(blockId, 0);
        return base.TryRemove(blockId);
    }
}
