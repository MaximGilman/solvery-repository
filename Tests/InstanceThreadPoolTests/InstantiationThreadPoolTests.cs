using System;
using FluentAssertions;
using InstanceThreadPool;
using Xbehave;
using Xunit;

namespace InstanceThreadPoolTests;

/// <summary>
/// Тесты создания пула потоков.
/// </summary>
public class InstantiationThreadPoolTests
{
    [Scenario]
    [Example(1)]
    [Example(10)]
    [Example(100)]
    public void Constructor_WithPositiveIntegerThreadCountArgument_CreatesPool(int threadCount, IInstanceThreadPool threadPool, Exception exception)
    {
        "Когда создается пул потоков c позитивным количеством потоков".x(() =>
            {
                exception = Record.Exception(() =>
                {
                    threadPool = new InstanceThreadPool.InstanceThreadPool(threadCount);
                });
            }
        );
        "Никаких ошибок не возникает".x(() => exception.Should().BeNull());
        "Пул успешно создастя".x(() => threadPool.Should().NotBeNull());
    }

    [Scenario]
    [Example(-1)]
    [Example(0)]
    public void Constructor_WithNegativeOrZeroIntegerThreadCountArgument_ThrowsArgumentException(int threadCount,  Exception exception)
    {
        "Когда создается пул потоков c позитивным количеством потоков".x(() =>
            {
                exception = Record.Exception(() =>
                {
                    var instanceThreadPool = new InstanceThreadPool.InstanceThreadPool(threadCount);
                });
            }
        );
        "Возникнет ошибка валидации в конструкторе".x(() => exception.Should().BeOfType<ArgumentException>());
    }
}
