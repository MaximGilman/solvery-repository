using System;
using FluentAssertions;
using Utils.Guards;
using Xbehave;
using Xunit;

namespace Utils.Tests;

public class GuardTests
{
    #region IsNotEqual

    [Scenario]
    [Example(0, 1)]
    [Example(1, 0)]
    [Example("1", "0")]
    [Example("0", null)]
    [Example("", null)]
    public void IsNotEqual_Success<T>(T left, T right, Exception? exception)
    {
        "Когда сравниваем два элемента".x(() =>
            exception = Record.Exception(() => Guard.IsNotEqual(left, right)));

        "Никаких ошибок не возникает".x(() => exception.Should().BeNull());
    }

    [Scenario]
    [Example(0, 0)]
    [Example("0", "0")]
    [Example(0, "0")]
    [Example("0", 0)]
    [Example(null, null)]
    [Example(null, default)]
    public void IsNotEqual_Error<T>(T left, T right, Exception? exception)
    {
        "Когда сравниваем два элемента".x(() =>
            exception = Record.Exception(() => Guard.IsNotEqual(right, left))); // L и R поменяны местами!

        "Возникает ошибка".x(() => exception.Should().BeOfType<ArgumentException>());
    }

    #endregion

    #region IsGreater

    [Scenario]
    [Example(1, 0)]
    [Example(1, null)]
    [Example("1", 0)]
    [Example("1", null)]
    public void IsGreater_Success(int left, int right, Type type, Exception? exception)
    {
        "Когда сравниваем два элемента".x(() =>
            exception = Record.Exception(() => Guard.IsGreater(left, right)));

        "Никаких ошибок не возникает".x(() => exception.Should().BeNull());
    }

    [Scenario]
    [Example(1, 0)]
    [Example(1, null)]
    [Example("1", 0)]
    [Example("1", null)]
    [Example(1, 1)]
    [Example("1", 1)]
    public void IsGreater_Error(int left, int right, Type type, Exception? exception)
    {
        "Когда сравниваем два элемента".x(() =>
            exception = Record.Exception(() => Guard.IsGreater(right, left))); // L и R поменяны местами!

        "Возникает ошибка".x(() => exception.Should().BeOfType<ArgumentException>());
    }

    #endregion
}
