using TCPViaUDP.Models;
using Xbehave;

namespace TCPviaUDP.Tests.Models;

public class LongKeyMemoryByteDataBlockTests : DataBlockTests
{
    private Memory<byte> _value;
    [Background]
    public void Init()
    {
        "Дано скользящее окно".x(() =>
        {
            _value = "Some value"u8.ToArray();
        });
    }
    [Scenario]
    public void Value_WhenValue_Success(LongKeyMemoryByteDataBlock block, Exception exception)
    {
        "Когда создается блок с ключем и значением".x(() =>
        {
            exception = Record.Exception(() => { new LongKeyMemoryByteDataBlock(1, _value); });
        });
        "Никаких ошибок не возникает".x(() => Assert.Null(exception));

    }
    [Scenario]
    public void Value_WhenBlock_Success(LongKeyMemoryByteDataBlock block, Exception exception)
    {
        "Когда создается блок с ключем и значением блока".x(() =>
        {
            exception = Record.Exception(() => { new LongKeyMemoryByteDataBlock(1, new DataBlock<Memory<byte>>(_value)); });
        });
        "Никаких ошибок не возникает".x(() => Assert.Null(exception));

    }

    [Scenario]
    public void NegativeValue_WhenValue_Error(LongKeyMemoryByteDataBlock block, Exception exception)
    {
        "Когда создается значение с отрицательным ключем".x(() =>
        {
            exception = Record.Exception(() => { new LongKeyMemoryByteDataBlock(-1, new Memory<byte>()); });
        });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }

    [Scenario]
    public void NegativeValue_WhenBlock_Error(LongKeyMemoryByteDataBlock block, Exception exception)
    {
        "Когда создается блок с отрицательным значением".x(() =>
        {
            exception = Record.Exception(() => { new LongKeyMemoryByteDataBlock(-1, new DataBlock<Memory<byte>>(new Memory<byte>())); });
        });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }


}
