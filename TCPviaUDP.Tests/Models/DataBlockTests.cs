using TCPViaUDP.Models;
using TCPViaUDP.Models.DataBlocks;
using Xbehave;

namespace TCPviaUDP.Tests.Models;

public class DataBlockTests
{
    [Scenario]
    public void Value_Success(DataBlock<int> dataBlock, Exception exception)
    {
        "Когда создается блок со значением".x(() => { exception = Record.Exception(() => { new DataBlock<int>(1); }); });
        "Никаких ошибок не возникает".x(() => Assert.Null(exception));
    }

    [Scenario]
    public void DefaultValue_Error(DataBlock<int> dataBlock, Exception exception)
    {
        "Когда создается блок с default значением".x(() => { exception = Record.Exception(() => { new DataBlock<int>(default(int)); }); });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }

    [Scenario]
    public void DefaultValue_WhenBlock_Error(DataBlock<int> dataBlock, Exception exception)
    {
        "Когда создается блок с default значением блока".x(() => { exception = Record.Exception(() => { new DataBlock<int>(default(DataBlock<int>)); }); });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }

    [Scenario]
    public void NullValue_Error(DataBlock<int> dataBlock, Exception exception)
    {
        "Когда создается блок с null значением".x(() => { exception = Record.Exception(() => { new DataBlock<int>(null); }); });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }

    [Scenario]
    public void NullValue_WhenBlock_Error(DataBlock<int> dataBlock, Exception exception)
    {
        "Когда создается блок с null значением блока".x(() => { exception = Record.Exception(() => { new DataBlock<int>(null); }); });
        "Возникает ошибка".x(() =>
        {
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        });
    }
}
