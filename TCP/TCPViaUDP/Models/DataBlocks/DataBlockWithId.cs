using System.Numerics;
using Utils.Guards;

namespace TCPViaUDP.Models;

public record DataBlockWithId<TKey, TValue> : DataBlock<TValue> where TKey : INumber<TKey>
{
    public TKey BlockId { get; init; }

    public DataBlockWithId(TKey blockId, TValue data) : base(data)
    {
        Guard.IsNotDefault(blockId);
        Guard.IsNotDefault(data);
        Guard.IsNotNull(data);

        BlockId = blockId;
    }

    public DataBlockWithId(TKey blockId, DataBlock<TValue> dataBlock) : base(dataBlock)
    {
        Guard.IsNotDefault(blockId);
        Guard.IsNotDefault(dataBlock);
        Guard.IsNotNull(dataBlock);
        Guard.IsNotDefault(dataBlock.Data);
        Guard.IsNotNull(dataBlock.Data);

        BlockId = blockId;
        Data = dataBlock.Data;
    }
}
