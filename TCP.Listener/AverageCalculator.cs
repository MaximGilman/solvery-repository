namespace TCP.Listener;

public class AverageCalculator
{
    private TimeSpan _totalTimeElapsed = TimeSpan.Zero;
    private int _totalBytesTransferred = 0;

    public double CalculateAverageSpeedValue() => _totalBytesTransferred / _totalTimeElapsed.TotalSeconds;

    public static double CalculateCurrentSpeedValue(int iterationBytesReadAmount, TimeSpan iterationElapsedTime) =>
        iterationBytesReadAmount / iterationElapsedTime.TotalSeconds;

    public void AppendTotalTime(TimeSpan iterationElapsedTime)
    {
        _totalTimeElapsed += iterationElapsedTime;
    }

    public void AppendTotalTransferredBytesAmount(int iterationBytesReadAmount)
    {
        _totalBytesTransferred += iterationBytesReadAmount;
    }
}
