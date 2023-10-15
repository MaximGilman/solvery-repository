using Utils.Guards;

namespace TCPViaUDP.Models;

public record DataBlock<TValue>
{
    public TValue Data { get; init; }

    public DataBlock(TValue data)
    {
        Guard.IsNotDefault(data);
        Guard.IsNotNull(data);
        Data = data;
    }

    public DataBlock(DataBlock<TValue> block)
    {
        Guard.IsNotDefault(block);
        Guard.IsNotNull(block);
        Guard.IsNotDefault(block.Data);
        Data = block.Data;
    }
}
