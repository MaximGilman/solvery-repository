using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xbehave;
using InstanceThreadPool;
using Xunit;

namespace InstanceThreadPoolTests;

/// <summary>
/// Тесты выполнения методов в пуле.
/// </summary>
public class ExecutionActionInstanceThreadPoolTests
{
    private IInstanceThreadPool _multipleThreadPool;


    [Background]
    public void Background()
    {
        "Дан пул потоков".x(() =>
        {
            _multipleThreadPool = new InstanceThreadPool.InstanceThreadPool(10);
        });
    }

    [Scenario]
    public void Execute_WithNullAction_ThrowsArgumentNullException(Exception exception)
    {
        "Когда метод на исполнение добавляется в пулл потоков с null - Action".x(async () =>
        {
            exception = await Record.ExceptionAsync(() =>
            {
                _multipleThreadPool.QueueExecute(null);
                return Task.CompletedTask;
            });

        });

        "Возникает ошибка ArgumentNullException".x(() =>
        {
            exception.Should().BeOfType<ArgumentException>();
        });
    }

    [Scenario]
    public void Execute_WithNonNullAction_CallsActionOnThread(bool wasCalled)
    {
        "Когда метод на исполнение добавляется в пулл потоков с notnull - Action".x(() =>
        {
            _multipleThreadPool.QueueExecute(() =>
            {
                wasCalled = true;
            });
            return Task.CompletedTask;
        });

        "Он будет выполнен".x(() =>
        {
            Thread.Sleep(100);
            wasCalled.Should().BeTrue();
        });
    }

    [Scenario]
    public void Execute_WithNonNullActionAndParameter_CallsActionWithCorrectParameter(string expectedParam, string actualParam)
    {
        "Дан параметр для Action".x(() =>
        {
            expectedParam = "Test";
        });

        "Когда метод на исполнение добавляется в пулл потоков".x(() =>
        {
            _multipleThreadPool.QueueExecute(expectedParam, param =>
            {
                actualParam = (string)param;
            });
        });


        "Он будет выполнен c указанным параметром".x(() =>
        {
            Thread.Sleep(100);
            actualParam.Should().Be(expectedParam);
        });
    }

    [Scenario]
    public void Execute_MultipleCalls_CallsActionsOnMultipleThreads(int actionsCount, List<int> expectedThreadIds,
        ConcurrentBag<int> actualThreadIds)
    {
        "Даны параметры для проверки".x(() =>
        {
            actionsCount = 20;
            expectedThreadIds = new List<int>();
            actualThreadIds = new ConcurrentBag<int>();
        });

        "Когда метод на исполнение добавляется в пулл потоков 20 раз".x(() =>
        {
            for (var i = 0; i < actionsCount; i++)
            {
                _multipleThreadPool.QueueExecute(() =>
                {
                    actualThreadIds.Add(Environment.CurrentManagedThreadId);
                });
            }
        });

        "Методы будут выполнены в запрошенном количестве".x(() =>
        {
            Thread.Sleep(100);
            actualThreadIds.Should().HaveCount(actionsCount);
        });
    }

    [Scenario]
    public void Execute_ActionThrowsException_LogsErrorAndThrowsException(Exception exception)
    {
        "Когда метод на исполнение добавляется в пулл потоков c действием, вызывающим исключение".x(() =>
        {
            _multipleThreadPool.QueueExecute(() =>
            {
                try
                {
                    throw new ApplicationException("Test");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

            });
            return Task.CompletedTask;
        });

        "Возникает ошибка ApplicationException".x(() =>
            {
                Thread.Sleep(100);
                exception.Should().NotBeNull();
                exception.Should().BeOfType<ApplicationException>();
                exception.Message.Should().Be("Test");
            })
            .Teardown(() => Trace.Listeners.Clear());
    }

    [Scenario]
    public void Execute_ActionWithParameterThrowsException_LogsErrorAndThrowsException(Exception exception)
    {

        "Когда метод на исполнение добавляется в пулл потоков c действием, вызывающим исключение".x(() =>
        {
            _multipleThreadPool.QueueExecute("Test parameter", (_) =>
            {
                try
                {
                    throw new ApplicationException("Test");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

            });
        });

        "Возникает ошибка ApplicationException".x(() =>
            {
                Thread.Sleep(100);
                exception.Should().NotBeNull();
                exception.Should().BeOfType<ApplicationException>();
                exception.Message.Should().Be("Test");
            })
            .Teardown(() => Trace.Listeners.Clear());
    }
}
