using Microsoft.Extensions.Logging;
using TCPViaUDP.Models;
using TCPViaUDP.Models.DataBlocks;
using Utils.Guards;

namespace TCPViaUDP.Helpers.ConcurrentWindow;

public class LongKeyMemoryByteAcknowledgedConcurrentBlockWindow : AcknowledgedConcurrentBlockWindow<long, Memory<byte>>
{
    public LongKeyMemoryByteAcknowledgedConcurrentBlockWindow(int windowFrameSize, ILogger<AcknowledgedConcurrentBlockWindow<long, Memory<byte>>> logger) : base(windowFrameSize, logger)
    {
    }

    public override bool TryAddBlock(DataBlockWithId<long, Memory<byte>> blockWithId)
    {
        Guard.IsNotDefault(blockWithId);

        Guard.IsNotDefault(blockWithId.Id);
        Guard.IsGreater(blockWithId.Id, 0);

        Guard.IsNotDefault(blockWithId.Block.Data);
        Guard.IsNotNull(blockWithId.Block.Data);
        Guard.IsNotEmpty(blockWithId.Block.Data);

        return base.TryAddBlock(blockWithId);
    }

    public override bool TryRemove(long blockId)
    {
        Guard.IsNotDefault(blockId);
        Guard.IsGreater(blockId, 0);
        return base.TryRemove(blockId);
    }
}
