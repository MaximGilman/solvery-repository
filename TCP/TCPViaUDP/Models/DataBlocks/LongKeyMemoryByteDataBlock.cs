using Utils.Constants;
using Utils.Guards;

namespace TCPViaUDP.Models.DataBlocks;

public record LongKeyMemoryByteDataBlock : DataBlockWithId<long, Memory<byte>>
{

    public LongKeyMemoryByteDataBlock(long id, DataBlock<Memory<byte>> dataBlock) : base(id, dataBlock)
    {
        Guard.IsGreater(id, 0);
        Guard.IsLessOrEqual(dataBlock.Data.Length, NetworkConstants.MTU_DATA_BLOCK_MAX_BYTE_SIZE);
    }

    public LongKeyMemoryByteDataBlock(long id, Memory<byte> dataValue) : base(id, dataValue)
    {
        Guard.IsGreater(id, 0);
        Guard.IsLessOrEqual(dataValue.Length, NetworkConstants.MTU_DATA_BLOCK_MAX_BYTE_SIZE);
    }
}
