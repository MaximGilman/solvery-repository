using System;
using System.Threading;
using System.Threading.Tasks;
using TCP.Listener;
using Xunit;
namespace Task1.TcpListener.Tests;

public class StopWatchMeasurerTests
{
    private readonly TimeSpan _expectedElapsedTime = TimeSpan.FromSeconds(1);
    private const string RESULT = "result";
    [Fact]
    public async Task MeasureAsync()
    {
        var measurer = new StopWatchMeasurer();

        var result = await measurer.MeasureAsync(async () =>
        {
            await Task.Delay(_expectedElapsedTime);
            return RESULT;
        });

        Assert.Equal(RESULT, result.ActionResult);
        Assert.True(result.ElapsedTime >= _expectedElapsedTime);
    }

    [Fact]
    public void Measure()
    {
        var measurer = new StopWatchMeasurer();

        var result = measurer.Measure(() =>
        {
            Thread.Sleep(_expectedElapsedTime);
            return RESULT;
        });

        Assert.Equal(RESULT, result.ActionResult);
        Assert.True(result.ElapsedTime >= _expectedElapsedTime);
    }
}
