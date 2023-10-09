using FluentAssertions;
using TCPViaUDP.Models;
using TCPviaUDP.Tests.Models;
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
    private const int CORRECT_BLOCK_ID = 1;
    private const int INCORRECT_BLOCK_ID = -1;

    /// <summary>
    /// Для каждого сценария создаем окно.
    /// </summary>
    [Background]
    public void Init()
    {
        "Даны блоки данных".x(() =>
        {
            _existedDataBytes = "Some value"u8.ToArray();
            _existedKeyBytes = BitConverter.GetBytes(CORRECT_BLOCK_ID);
            _existedNegativeKeyBytes = BitConverter.GetBytes(INCORRECT_BLOCK_ID);

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
            _emptyMemoryDataWithId = new Memory<byte>(BitConverter.GetBytes(CORRECT_BLOCK_ID));
        });
    }

    #region ToMemory

    [Scenario]
    public void ToMemory_Success(LongKeyMemoryByteDataBlock block, Exception exception)
    {
        "Когда преобразуется в блок".x(() =>
        {
            exception = Record.Exception(() =>
            {
                block = ImplementationTransformer.ToBlock(_existedBlock);
            });
        });

        "Никаких ошибок не возникает".x(() => Assert.Null(exception));
        "Полученный блок содержит нужные данные".x(() =>
        {
            Assert.Equal(CORRECT_BLOCK_ID, block.BlockId);
            Assert.Equal(_existedDataBytes.Length, block.Data.Length);
            foreach (var (exp, act) in _existedDataBytes.Zip(block.Data.ToArray()))
            {
                Assert.Equal(exp, act);
            }
        });
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
    public void ToMemory_ErrorPositionedBlock(LongKeyMemoryByteDataBlock block, Exception exception)
    {
// длина большая из-за данных - их порезать
        "Когда преобразуется в блок".x(() =>
        {
            exception = Record.Exception(() =>
            {
                block = ImplementationTransformer.ToBlock(_errorPositionedBlock);
            });
        });

        "Никаких ошибок не возникает".x(() => Assert.Null(exception));
        "Полученный блок содержит неверные данные".x(() =>
        {
            Assert.Equal(CORRECT_BLOCK_ID, block.BlockId);
            Assert.Equal(_existedDataBytes.Length, block.Data.Length);
            foreach (var (exp, act) in _existedDataBytes.Zip(block.Data.ToArray()))
            {
                Assert.Equal(exp, act);
            }
        });
    }

    [Scenario]
    public void ToMemory_NegativeKeyBlock()
    {
    }

    #endregion

    #region ToBlock

    [Scenario]
    public void ToBlock_Success(LongKeyMemoryByteDataBlock block, Memory<byte> memory, Exception exception)
    {
        "Дан блок".x(() => block = new LongKeyMemoryByteDataBlock(CORRECT_BLOCK_ID, _existedDataBytes));

        "Когда преобразуется в память".x(() =>
        {
            exception = Record.Exception(() =>
            {
                memory = ImplementationTransformer.ToMemory(block);
            });
        });
        "Никаких ошибок не возникает".x(() => Assert.Null(exception));
        "Полученная память нужного размера".x(() => Assert.Equal(memory.Length, LONG_SIZE + _existedDataBytes.Length));
    }

    [Scenario]
    public void ToBlock_Null_Error(Exception exception)
    {
        "Когда null преобразуется в память".x(() =>
        {
            exception = Record.Exception(() =>
            {
                 ImplementationTransformer.ToMemory(null);
            });
        });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }
    #endregion
}
