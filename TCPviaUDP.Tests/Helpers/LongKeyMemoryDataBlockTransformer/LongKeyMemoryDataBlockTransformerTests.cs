using TCPViaUDP.Models;
using Xbehave;
using ImplementationTransformer = TCPViaUDP.Helpers.DataBlockTransformer.LongKeyMemoryDataBlockTransformer;

namespace TCPviaUDP.Tests.Helpers.LongKeyMemoryDataBlockTransformer;

public class LongKeyMemoryDataBlockTransformerTests
{
    // Пустые данные. Без ключа.
    private Memory<byte> _emptyMemoryData;

    // Пустой ключ. Без данных.
    private Memory<byte> _emptyMemoryKey;

    // Блок с пустым ключем и данными.
    private Memory<byte> _emptyMemoryDataWithEmptyId;

    // Блок без ключа, но с данными.
    private Memory<byte> _memoryDataWithEmptyId;

    // Блок без данных, но с ключем.
    private Memory<byte> _emptyMemoryDataWithId;

    // Данные. Без ключа.
    private byte[] _existedDataBytes;

    // Ключ. Без данных.
    private byte[] _existedKeyBytes;

    // Ключ с отрицательным значением.
    private byte[] _existedNegativeKeyBytes;

    // Блок с ключем и данными.
    private Memory<byte> _existedBlock;

    // Блок с отрицательным ключем и данными.
    private Memory<byte> _negativeExistedBlock;

    // Блок с неправильно позинионируемым ключем.
    private Memory<byte> _errorPositionedBlock;

    private const int LONG_SIZE = ImplementationTransformer.LONG_SIZE;

    /// <summary>
    /// Для каждого сценария создаем окно.
    /// </summary>
    [Background]
    public void Init()
    {
        "Даны блоки данных".x(() =>
        {
            _existedDataBytes = "Some value"u8.ToArray();
            _existedKeyBytes = BitConverter.GetBytes(1);
            _existedNegativeKeyBytes = BitConverter.GetBytes(-1);

            var correctPositionedBlock = new byte[LONG_SIZE + _existedDataBytes.Length];
            Buffer.BlockCopy(_existedKeyBytes, 0, correctPositionedBlock, 0, _existedKeyBytes.Length);
            Buffer.BlockCopy(_existedDataBytes, 0, correctPositionedBlock, LONG_SIZE, _existedDataBytes.Length);
            _existedBlock = new Memory<byte>(correctPositionedBlock);

            // Блок, который не отступает размер для типа ключа, а просто пишет по длине его значения.
            var wrongPositionedBlock = new byte[_existedKeyBytes.Length + _existedDataBytes.Length];
            Buffer.BlockCopy(_existedKeyBytes, 0, wrongPositionedBlock, 0, _existedKeyBytes.Length);
            Buffer.BlockCopy(_existedDataBytes, 0, wrongPositionedBlock, _existedKeyBytes.Length, _existedDataBytes.Length);
            _errorPositionedBlock = new Memory<byte>(wrongPositionedBlock);

            _existedBlock = new Memory<byte>(correctPositionedBlock);
            _negativeExistedBlock = new Memory<byte>(_existedNegativeKeyBytes);

            _emptyMemoryData = new();
            _emptyMemoryKey = new Memory<byte>();
            _emptyMemoryDataWithEmptyId = new();
            _memoryDataWithEmptyId = _existedDataBytes.ToArray();
            _emptyMemoryDataWithId = new Memory<byte>(BitConverter.GetBytes(1));
        });
    }

    #region ToMemory

    [Scenario]
    public void ToMemory_Success()
    {
    }

    [Scenario]
    public void ToMemory_EmptyKey()
    {
    }

    [Scenario]
    public void ToMemory_EmptyData()
    {
    }

    [Scenario]
    public void ToMemory_EmptyBlock()
    {
    }

    [Scenario]
    public void ToMemory_ErrorPositionedBlock()
    {
    }

    [Scenario]
    public void ToMemory_NegativeKeyBlock()
    {
    }

    #endregion

    #region ToBlock

    [Scenario]
    public void ToBlock_Success(LongKeyMemoryByteDataBlock block, Memory<byte> memory, Exception? exception)
    {
        "Дан блок".x(() => block = new LongKeyMemoryByteDataBlock(1, _existedDataBytes));

        "Когда преобразуется в память".x(() =>
        {
            exception = Record.Exception(() =>
            {
                memory = ImplementationTransformer.ToMemory(block);
            }) ?? null;
        });
        "Никаких ошибок не возникает".x(() => Assert.Null(exception));
        "Полученный блок нужного размера".x(() => Assert.Equal(memory.Length, LONG_SIZE + _existedDataBytes.Length));
    }

    [Scenario]
    public void ToBlock_DefaultKey(LongKeyMemoryByteDataBlock block, Memory<byte> memory, Exception? exception)
    {
        "Дан блок".x(() => block = new LongKeyMemoryByteDataBlock(default, _existedDataBytes));

        "Когда преобразуется в память".x(() =>
        {
            exception = Record.Exception(() =>
            {
                memory = ImplementationTransformer.ToMemory(block);
            }) ?? null;
        });
        "Возникает ошибка".x(() => Assert.NotNull(exception));
        "Полученный блок нужного размера".x(() => Assert.Equal(memory.Length, LONG_SIZE + _existedDataBytes.Length));
    }

    [Scenario]
    public void ToBlock_EmptyData()
    {
    }

    [Scenario]
    public void ToBlock_EmptyBlock()
    {
    }

    [Scenario]
    public void ToBlock_ErrorPositionedBlock()
    {
    }

    [Scenario]
    public void ToBlock_NegativeKeyBlock()
    {
    }

    #endregion
}
