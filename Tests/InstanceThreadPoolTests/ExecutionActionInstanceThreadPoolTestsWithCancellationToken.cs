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
public class ExecutionActionInstanceThreadPoolTestsWithCancellationToken
{
    private IInstanceThreadPool _multipleThreadPool;
    private CancellationTokenSource _cancellationTokenSource;

    [Background]
    public void Background()
    {
        "Дан пул потоков и источник токенов".x(() =>
        {
            _multipleThreadPool = new InstanceThreadPool.InstanceThreadPool(10);
            _cancellationTokenSource = new CancellationTokenSource();
        });
    }

    #region Not Canceled Token

    [Scenario]
    public void Execute_WithNullAction_ThrowsArgumentNullException(Exception exception)
    {
        "Когда метод на исполнение добавляется в пулл потоков с null - Action".x(async () =>
        {
            exception = await Record.ExceptionAsync(() =>
            {
                _multipleThreadPool.QueueExecute(null, _cancellationTokenSource.Token);
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
            _multipleThreadPool.QueueExecute((_) =>
            {
                wasCalled = true;
            }, _cancellationTokenSource.Token);
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
            _multipleThreadPool.QueueExecute(expectedParam, (param, _) =>
            {
                actualParam = (string)param;
            }, _cancellationTokenSource.Token);
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
                _multipleThreadPool.QueueExecute((_) =>
                {
                    actualThreadIds.Add(Environment.CurrentManagedThreadId);
                }, _cancellationTokenSource.Token);
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
            _multipleThreadPool.QueueExecute((_) =>
            {
                try
                {
                    throw new ApplicationException("Test");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

            }, _cancellationTokenSource.Token);
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
            _multipleThreadPool.QueueExecute("Test parameter", (_, _) =>
            {
                try
                {
                    throw new ApplicationException("Test");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

            }, _cancellationTokenSource.Token);
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

    #endregion

    #region Canceled Token

    [Scenario]
    public void Execute_WithNullAction_Canceled_NotThrowsArgumentNullException(Exception exception)
    {
        "Токен отменен".x(() => _cancellationTokenSource.Cancel());

        "Когда метод на исполнение добавляется в пулл потоков с null - Action".x(async () =>
        {
            exception = await Record.ExceptionAsync(() =>
            {
                _multipleThreadPool.QueueExecute(null, _cancellationTokenSource.Token);
                return Task.CompletedTask;
            });

        });

        "Код не выполняется. Ошибка не возникает".x(() =>
        {
            exception.Should().BeNull();
        });
    }

    [Scenario]
    public void Execute_WithNonNullAction_Canceled_CallsActionOnThread(bool wasCalled)
    {
        "Токен отменен".x(() => _cancellationTokenSource.Cancel());

        "Когда метод на исполнение добавляется в пулл потоков с notnull - Action".x(() =>
        {
            _multipleThreadPool.QueueExecute((_) =>
            {
                wasCalled = true;
            }, _cancellationTokenSource.Token);
            return Task.CompletedTask;
        });

        "Метод не будет выполнен".x(() =>
        {
            wasCalled.Should().BeFalse();
        });
    }

    [Scenario]
    public void Execute_WithNonNullActionAndParameter_Canceled_CallsActionWithCorrectParameter(string expectedParam,
        string actualParam)
    {
        "Токен отменен".x(() => _cancellationTokenSource.Cancel());

        "Дан параметр для Action".x(() =>
        {
            expectedParam = "Test";
        });

        "Когда метод на исполнение добавляется в пулл потоков".x(() =>
        {
            _multipleThreadPool.QueueExecute(expectedParam, (param, _) =>
            {
                actualParam = (string)param;
            }, _cancellationTokenSource.Token);
        });


        "Он не будет выполнен c указанным параметром".x(() =>
        {
            actualParam.Should().BeNull();
        });
    }

    [Scenario]
    public void Execute_MultipleCalls_Canceled_CallsActionsOnMultipleThreads(int actionsCount, List<int> expectedThreadIds,
        ConcurrentBag<int> actualThreadIds)
    {
        "Токен отменен".x(() => _cancellationTokenSource.Cancel());

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
                _multipleThreadPool.QueueExecute((_) =>
                {
                    actualThreadIds.Add(Environment.CurrentManagedThreadId);
                }, _cancellationTokenSource.Token);
            }
        });

        "Методы будут не выполнены. Их количество будет 0".x(() =>
        {
            actualThreadIds.Should().HaveCount(0);
        });
    }

    [Scenario]
    public void Execute_ActionThrowsException_Canceled_LogsErrorAndThrowsException(Exception exception)
    {
        "Токен отменен".x(() => _cancellationTokenSource.Cancel());

        "Когда метод на исполнение добавляется в пулл потоков c действием, вызывающим исключение".x(() =>
        {
            _multipleThreadPool.QueueExecute((_) =>
            {
                try
                {
                    throw new ApplicationException("Test");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

            }, _cancellationTokenSource.Token);
            return Task.CompletedTask;
        });

        "Не возникает ошибка ApplicationException".x(() =>
        {
            exception.Should().BeNull();
        });
    }

    [Scenario]
    public void Execute_ActionWithParameterThrowsException_Canceled_LogsErrorAndThrowsException(Exception exception)
    {
        "Токен отменен".x(() => _cancellationTokenSource.Cancel());

        "Когда метод на исполнение добавляется в пулл потоков c действием, вызывающим исключение".x(() =>
        {
            _multipleThreadPool.QueueExecute("Test parameter", (_, _) =>
            {
                try
                {
                    throw new ApplicationException("Test");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

            }, _cancellationTokenSource.Token);
        });

        "Не возникает ошибка ApplicationException".x(() =>
        {
            exception.Should().BeNull();
        });
    }

    #endregion
}
