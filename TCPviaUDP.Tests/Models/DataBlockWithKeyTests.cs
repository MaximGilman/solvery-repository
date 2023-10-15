using TCPViaUDP.Models;
using TCPViaUDP.Models.DataBlocks;
using Xbehave;

namespace TCPviaUDP.Tests.Models;

public class DataBlockWithKeyTests : DataBlockTests
{
    [Scenario]
    public void KeyAndValue_Success(DataBlockWithId<int, int> dataBlock, Exception exception)
    {
        "Когда создается блок с ключем и значением".x(() => { exception = Record.Exception(() => { new DataBlockWithId<int, int>(1, 1); }); });
        "Никаких ошибок не возникает".x(() => Assert.Null(exception));
    }

    [Scenario]
    public void DefaultKeyWithValue_Error(DataBlock<int> dataBlock, Exception exception)
    {
        "Когда создается блок с default ключем, но со значением".x(() =>
        {
            exception = Record.Exception(() => { new DataBlockWithId<int, int>(default, 1); });
        });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }

    [Scenario]
    public void DefaultValueWithKey_WhenValue_Error(DataBlock<int> dataBlock, Exception exception)
    {
        "Когда создается блок с ключем, но с default значением".x(() =>
        {
            exception = Record.Exception(() => { new DataBlockWithId<int, int>(1, default(int)); });
        });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }

    [Scenario]
    public void DefaultValueWithKey_WhenBlock_Error(DataBlock<int> dataBlock, Exception exception)
    {
        "Когда создается блок с ключем, но с default значением блока".x(() =>
        {
            exception = Record.Exception(() => { new DataBlockWithId<int, int>(1, default(DataBlock<int>)); });
        });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }

    [Scenario]
    public void DefaultValueAndKey_Error(DataBlock<int> dataBlock, Exception exception)
    {
        "Когда создается блок с default ключем и default значением".x(() =>
        {
            exception = Record.Exception(() => { new DataBlockWithId<int, int>(default, default(int)); });
        });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }

    [Scenario]
    public void DefaultValueAndKey_WhenBlock_Error(DataBlock<int> dataBlock, Exception exception)
    {
        "Когда создается блок с default ключем и default значением блока".x(() =>
        {
            exception = Record.Exception(() => { new DataBlockWithId<int, int>(default, default(DataBlock<int>)); });
        });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }
}
