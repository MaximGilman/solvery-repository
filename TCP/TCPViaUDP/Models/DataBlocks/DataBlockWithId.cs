using System.Numerics;
using Utils.Guards;

namespace TCPViaUDP.Models.DataBlocks;

public record DataBlockWithId<TKey, TValue> where TKey : INumber<TKey>
{
    public TKey Id { get; init; }
    public DataBlock<TValue> Block { get; init; }

    public DataBlockWithId(TKey id, TValue data)
    {
        Guard.IsNotDefault(id);
        Guard.IsNotDefault(data);
        Guard.IsNotNull(data);

        Id = id;
        Block = new DataBlock<TValue>(data);
    }

    public DataBlockWithId(TKey id, DataBlock<TValue> dataBlock)
    {
        Guard.IsNotDefault(id);
        Guard.IsNotDefault(dataBlock);
        Guard.IsNotNull(dataBlock);
        Guard.IsNotDefault(dataBlock.Data);
        Guard.IsNotNull(dataBlock.Data);

        Id = id;
        Block = dataBlock;
    }
}
