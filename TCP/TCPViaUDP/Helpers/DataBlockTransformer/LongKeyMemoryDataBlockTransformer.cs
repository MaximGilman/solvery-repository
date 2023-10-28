using TCPViaUDP.Models.DataBlocks;
using Utils.Constants;
using Utils.Guards;

namespace TCPViaUDP.Helpers.DataBlockTransformer;

public static class LongKeyMemoryDataBlockTransformer
{
    /// <summary>
    /// Преобразовать информацию о блоке в массив байт.
    /// </summary>
    /// <param name="dataBlock">Информация о блоке.</param>
    public static Memory<byte> ToMemory(LongKeyMemoryByteDataBlock dataBlock)
    {
        Guard.IsNotDefault(dataBlock);
        Guard.IsNotNull(dataBlock);
        Guard.IsNotDefault(dataBlock.Id);
        Guard.IsGreater(dataBlock.Id, 0);
        Guard.IsNotDefault(dataBlock.Block.Data);
        Guard.IsNotEmpty(dataBlock.Block.Data);
        return AddBlockIdToData(dataBlock.Id, dataBlock.Block.Data);
    }

    public static LongKeyMemoryByteDataBlock ToBlock(Memory<byte> memory)
    {
        Guard.IsNotDefault(memory);
        Guard.IsNotNull(memory);
        Guard.IsNotEmpty(memory);
        Guard.IsGreater(memory.Length, TypeSizeConstants.LONG_SIZE);
        var (blockId, blockData) = ExtractBlockIdAndData(memory);

        Guard.IsNotDefault(blockId);
        Guard.IsNotDefault(blockData);
        Guard.IsNotEmpty(blockData);

        return new LongKeyMemoryByteDataBlock(blockId, blockData);
    }

    /// <summary>
    /// Добавить индекс текущего блока в начало его данных.
    /// </summary>
    private static Memory<byte> AddBlockIdToData(long blockId, Memory<byte> data)
    {
        var resultMemory = new byte[TypeSizeConstants.LONG_SIZE + data.Length];
        var blockIdBytes = BitConverter.GetBytes(blockId);
        var dataArray = data.ToArray();
        Buffer.BlockCopy(blockIdBytes, 0, resultMemory, 0, blockIdBytes.Length);
        Buffer.BlockCopy(dataArray, 0, resultMemory, TypeSizeConstants.LONG_SIZE, dataArray.Length);
        return resultMemory;
    }

    /// <summary>
    /// Достать из памяти ключ и значение блока.
    /// </summary>
    private static (long blockId, Memory<byte> blockData) ExtractBlockIdAndData(Memory<byte> blockDataWithKey)
    {
        Memory<byte> keyMemory = blockDataWithKey[..TypeSizeConstants.LONG_SIZE];
        long blockId = BitConverter.ToInt64(keyMemory.Span);
        Guard.IsGreater(blockId, 0);

        Memory<byte> blockData = blockDataWithKey[TypeSizeConstants.LONG_SIZE..];

        return (blockId, blockData);

    }
}
