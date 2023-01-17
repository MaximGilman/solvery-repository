using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace Threads.Tests;

public class SleepSort_Tests
{
    private readonly StringWriter _stringWriter = new();
    private readonly Fixture _fixture = new();
    public SleepSort_Tests()
    {
        Console.SetOut(_stringWriter);
    }

    [Theory]
    [InlineData(null)]
    public void SortTest_Error(IEnumerable<string> items)
    {
        Assert.Throws<ArgumentNullException>(() => Task6SleepSort.SleepSort(items));
    }

    [Fact(Skip = "Пока не прикрутил чтение из консоли.")]
    public void SortTest()
    {
        var items = _fixture.Create<List<string>>();
        Task6SleepSort.SleepSort(items);

        var expectedItems = items.OrderBy(x => x.Length);

        Thread.Sleep(items.Count * Task6SleepSort.ProportionalityFactor + 1000);
        string[] actualItems = _stringWriter.ToString().Split("\n");

        var actualItemsLengths = actualItems.Select(x => x.Length);
        var expectedItemsLengths = expectedItems.Select(x => x.Length);

        actualItemsLengths.Should().BeEquivalentTo(expectedItemsLengths);
    }
}