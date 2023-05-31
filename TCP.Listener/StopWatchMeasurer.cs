using System.Diagnostics;
using TCP.Listener.Task_1._TcpListener.Models;
namespace TCP.Listener;

public class StopWatchMeasurer
{
    private readonly Stopwatch _stopwatch = new();

    public async Task<TimerActionResult<TResult>> MeasureAsync<TResult>(Func<ValueTask<TResult>> action)
    {
        _stopwatch.Start();
        var actionResult = await action();
        _stopwatch.Stop();
        var elapsedTime = _stopwatch.Elapsed;
        _stopwatch.Reset();
        return new TimerActionResult<TResult>()
        {
            ActionResult = actionResult,
            ElapsedTime = elapsedTime
        };
    }

    public TimerActionResult<TResult> Measure<TResult>(Func<TResult> action)
    {
        _stopwatch.Start();
        var actionResult = action();
        _stopwatch.Stop();
        var elapsedTime = _stopwatch.Elapsed;
        _stopwatch.Reset();
        return new TimerActionResult<TResult>()
        {
            ActionResult = actionResult,
            ElapsedTime = elapsedTime
        };
    }
}
