using System;
using TCP.Listener;
using Xunit;

namespace Task1.TcpListener.Tests;

public class AverageCalculatorTests
{
    private const double EXPECTED_BYTES_TRANSFERRED_VALUE = 1024D;
    private const int CURRENT_BYTES_TRANSFERRED_VALUE = 1024;
    private readonly TimeSpan _currentElapsedTime = TimeSpan.FromSeconds(1);

    #region Success cases

    [Fact]
    public void CalculateAverageSpeedValue_WithTotalTime()
    {
        var calculator = new AverageCalculator();

        calculator.AppendTotalTime(_currentElapsedTime);
        calculator.AppendTotalTransferredBytesAmount(CURRENT_BYTES_TRANSFERRED_VALUE);

        Assert.Equal(EXPECTED_BYTES_TRANSFERRED_VALUE, calculator.CalculateAverageSpeedValue());
    }

    [Fact]
    public void CalculateCurrentSpeedValue_WithIterationTime()
    {
        var actualValue = AverageCalculator.CalculateCurrentSpeedValue(CURRENT_BYTES_TRANSFERRED_VALUE, _currentElapsedTime);

        Assert.Equal(EXPECTED_BYTES_TRANSFERRED_VALUE, actualValue);
    }

    [Fact]
    public void AppendTotalTime_WithTotalTime()
    {
        var calculator = new AverageCalculator();

        calculator.AppendTotalTime(_currentElapsedTime);

        Assert.Equal(_currentElapsedTime, calculator.TotalTimeElapsed);
    }

    [Fact]
    public void AppendTotalTransferredBytesAmount_WithTotalBytes()
    {
        var calculator = new AverageCalculator();

        calculator.AppendTotalTransferredBytesAmount(CURRENT_BYTES_TRANSFERRED_VALUE);

        Assert.Equal(EXPECTED_BYTES_TRANSFERRED_VALUE, calculator.TotalBytesTransferred);
    }

    #endregion

    #region Exception cases

    [Fact]
    public void CalculateCurrentSpeedValue_WithoutIterationTime()
    {
        Assert.Throws<ArgumentException>(() =>
            AverageCalculator.CalculateCurrentSpeedValue(CURRENT_BYTES_TRANSFERRED_VALUE, TimeSpan.Zero));
    }

    [Fact]
    public void CalculateCurrentSpeedValue_WithNegativeIterationTime()
    {
        Assert.Throws<ArgumentException>(() =>
            AverageCalculator.CalculateCurrentSpeedValue(CURRENT_BYTES_TRANSFERRED_VALUE, TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void CalculateAverageSpeedValue_WithoutTotalTime()
    {
        var calculator = new AverageCalculator();

        Assert.Throws<ArgumentException>(() => calculator.CalculateAverageSpeedValue());
    }

    [Fact]
    public void CalculateAverageSpeedValue_WithNegativeTotalTime()
    {
        var calculator = new AverageCalculator();

        Assert.Throws<ArgumentException>(() =>
        {
            calculator.AppendTotalTime(TimeSpan.FromSeconds(-1));
            return calculator.CalculateAverageSpeedValue();
        });
    }

    [Fact]
    public void AppendTotalTime_WithZeroIterationTime()
    {
        var calculator = new AverageCalculator();

        Assert.Throws<ArgumentException>(() => calculator.AppendTotalTime(TimeSpan.Zero));
    }

    [Fact]
    public void AppendTotalTransferredBytesAmount_WithNegativeBytesAmount()
    {
        var calculator = new AverageCalculator();

        Assert.Throws<ArgumentException>(() => calculator.AppendTotalTransferredBytesAmount(-1));
    }

    #endregion
}
