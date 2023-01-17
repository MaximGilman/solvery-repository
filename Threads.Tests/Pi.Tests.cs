using System;
using FluentAssertions;
using Xunit;

namespace Threads.Tests;

public class PiTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 0)]

    public void CalcPiTest_Errors(int threadCount, int iterationsCount)
    {
        Assert.Throws<ArgumentException>(() => Task8PiParallelCalc.CalculatePi(threadCount, iterationsCount));
    }
    
    [Theory]
    [InlineData(20, 200_000_000, 3.1415926560897485)]
    [InlineData(10, 200_000_000, 3.141592656089581)]
    [InlineData(20, 100_000_000, 3.1415926585897167)]
    [InlineData(1, 1000, 3.1420924036835283)]
    [InlineData(2, 1000, 3.1420924036835274)]
    [InlineData(1, 1, 3.466666666666667)]
    [InlineData(1000, 1, 3.466666666666667)]

    public void CalcPiTest_Success(int threadCount, int iterationsCount, double expectedPi)
    {
        var pi = Task8PiParallelCalc.CalculatePi(threadCount, iterationsCount);
        pi.Should().Be(expectedPi);
    }
}