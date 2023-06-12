using Utils.Guards;

namespace TCP.Listener;

public record AverageCalculator
{
    public TimeSpan TotalTimeElapsed { private set; get; } = TimeSpan.Zero;
    public int TotalBytesTransferred { private set; get; } = 0;

    public double CalculateAverageSpeedValue()
    {
        Guard.IsGreater(TotalTimeElapsed, TimeSpan.Zero);
        Guard.IsGreaterZero(TotalTimeElapsed.TotalSeconds);
        return TotalBytesTransferred / TotalTimeElapsed.TotalSeconds;
    }

    public static double CalculateCurrentSpeedValue(int iterationBytesReadAmount, TimeSpan iterationElapsedTime)
    {
        Guard.IsGreater(iterationElapsedTime, TimeSpan.Zero);
        Guard.IsGreaterZero(iterationElapsedTime.TotalSeconds);

        return iterationBytesReadAmount / iterationElapsedTime.TotalSeconds;
    }

    public void AppendTotalTime(TimeSpan iterationElapsedTime)
    {
        Guard.IsGreater(iterationElapsedTime, TimeSpan.Zero);
        TotalTimeElapsed += iterationElapsedTime;
    }

    public void AppendTotalTransferredBytesAmount(int iterationBytesReadAmount)
    {
        Guard.IsGreaterZero(iterationBytesReadAmount);
        TotalBytesTransferred += iterationBytesReadAmount;
    }
}
