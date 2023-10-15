using Utils.Guards;

namespace TCPViaUDP.Models;

public record LongKeyMemoryByteDataBlock : DataBlockWithId<long, Memory<byte>>
{
    public LongKeyMemoryByteDataBlock(long blockId, DataBlock<Memory<byte>> dataBlock) : base(blockId, dataBlock)
    {
        Guard.IsGreater(blockId, 0);
    }

    public LongKeyMemoryByteDataBlock(long blockId, Memory<byte> dataBlock) : base(blockId, dataBlock)
    {
        Guard.IsGreater(blockId, 0);
    }
}
