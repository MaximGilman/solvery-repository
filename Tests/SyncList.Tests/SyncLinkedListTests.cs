using System;
using System.Linq;
using System.Threading;
using Xunit;
using Xbehave;
using FluentAssertions;
using SyncListAccess.Lists;
using Utils.Guards;

namespace SyncList.Tests;

/// <summary>
/// Тесты многопоточного списка
/// </summary>
public class SyncLinkedListTests
{
    private SyncLinkedList<string> _syncStringList;

    /// <summary>
    /// Для каждого сценария создаем коллекцию.
    /// </summary>
    [Background]
    public void InitCollection()
    {
        "Дана коллекция".x(() =>
        {
            _syncStringList = new SyncLinkedList<string>();
        });
    }


    [Scenario]
    [Example(0)]
    [Example(1)]
    [Example(10)]
    [Example(1000)]
    public void ToStringEquals(int count, SyncLinkedList<int> list)
    {
        "Дан список".x(() =>
        {
            list = new SyncLinkedList<int>();
            for (int i = 0; i < count; i++)
            {
                list.Add(i);
            }
        });

        "После сортировки".x(() => list.Sort());

        "Вывод равен ожидаемому".x(() =>
        {
            var expected = count switch
            {
                0 => "Empty",
                1 => "0 - X",
                _ => $"{string.Join(" - ", Enumerable.Range(0, count))} - X"
            };
            expected.Should().BeEquivalentTo(list.ToString());
        });
    }


    /// <summary>
    /// Добавление элементов в список
    /// </summary>
    /// <param name="repeatCount">Количество повторений.</param>
    /// <param name="threadCount">Количество потоков.</param>
    /// <param name="exception">Контейнер для исключений.</param>
    [Scenario]
    [Example(1, 1)]
    [Example(1, 10)]
    [Example(10, 1)]
    [Example(10, 10)]
    [Example(100, 100)]
    public void MultiThreadAdd(int repeatCount, int threadCount, Exception exception)
    {
        "Проверены входные значения".x(() =>
        {
            Guard.IsGreater(repeatCount, 0);
            Guard.IsGreater(threadCount, 0);
        });

        "Когда добавляются элементы".x(() =>
        {
            exception = Record.Exception(() =>
            {
                exception = Record.Exception(() =>
                {
                    for (var repeatIndex = 0; repeatIndex < repeatCount; repeatIndex++)
                    {
                        for (var threadIndex = 0; threadIndex < threadCount; threadIndex++)
                        {
                            var itemValue = threadIndex.ToString();
                            var addThread = new Thread(() => { _syncStringList.Add(itemValue); });
                            addThread.Start();
                        }
                    }
                });
            });
        });

        "Никаких ошибок не возникает".x(() => exception.Should().BeNull());
        "Количество элементов должно быть равно количеству операция добавления".x(() =>
            _syncStringList.Count.Should().Be(repeatCount * threadCount));
    }


    /// <summary>
    /// Чтение записей из нескольких потоков
    /// </summary>
    /// <param name="itemsCount">Изначальное количество элементов в списке.</param>
    /// <param name="repeatCount">Количество повторений.</param>
    /// <param name="threadCount">Количество потоков.</param>
    /// <param name="exception">Контейнер для исключений.</param>
    /// <param name="expectedResults">Контейнер для ожидаемого вывода.</param>
    [Scenario(Skip = "Error prone")]
    [Example(1, 1, 1)]
    [Example(1, 1, 10)]
    [Example(1, 10, 1)]
    [Example(10, 1, 1)]
    [Example(10, 10, 10)]
    [Example(10, 1, 10)]
    [Example(10, 10, 1)]
    public void MultiThreadIteration_WithExistedList(int itemsCount, int repeatCount, int threadCount,
        string[] expectedResults, Exception exception)
    {
        "Проверены входные значения".x(() =>
        {
            Guard.IsGreater(itemsCount, 0);
            Guard.IsGreater(repeatCount, 0);
            Guard.IsGreater(threadCount, 0);
        });

        "Дан существующий список".x(() =>
        {
            var rawItems = Enumerable.Range(0, itemsCount).Select(x => x.ToString()).ToList();
            rawItems.ForEach(x => _syncStringList.Add(x));
        });

        "Инициализирована коллекция для возвращаемых строк".x(() =>
        {
            expectedResults = new string[repeatCount];
        });

        "Когда выводятся элементы".x(() =>
        {
            exception = Record.Exception(() =>
            {
                for (var repeatIndex = 0; repeatIndex < repeatCount; repeatIndex++)
                {
                    for (var threadIndex = 0; threadIndex < threadCount; threadIndex++)
                    {
                        var answerIndex = repeatIndex;
                        var addThread = new Thread(() =>
                        {
                            expectedResults[answerIndex] = _syncStringList.ToString();
                        });
                        addThread.Start();
                    }
                }
            });
        });

        "Никаких ошибок не возникает".x(() => exception.Should().BeNull());
        "Количество элементов не должно измениться".x(() => _syncStringList.Count.Should().Be(itemsCount));
        "Все полученные строки должны быть равны".x(() =>
            Assert.All(expectedResults, item => item.Should().BeEquivalentTo(_syncStringList.ToString())));
    }


    /// <summary>
    /// Чтение записей из нескольких потоков
    /// </summary>
    /// <param name="repeatCount">Количество повторений.</param>
    /// <param name="threadCount">Количество потоков.</param>
    /// <param name="exception">Контейнер для исключений.</param>
    /// <param name="expectedResults">Контейнер для ожидаемого вывода.</param>
    [Scenario]
    [Example(1, 1)]
    [Example(1, 10)]
    [Example(10, 1)]
    [Example(10, 10)]
    public void MultiThreadIteration_WithEmptyList(int repeatCount, int threadCount,
        string[] expectedResults, Exception exception)
    {
        "Проверены входные значения".x(() =>
        {
            Guard.IsGreater(repeatCount, 0);
            Guard.IsGreater(threadCount, 0);
        });

        "Инициализирована коллекция для возвращаемых строк".x(() =>
        {
            expectedResults = new string[repeatCount];
        });

        "Когда выводятся элементы".x(() =>
        {
            exception = Record.Exception(() =>
            {
                exception = Record.Exception(() =>
                {
                    for (var repeatIndex = 0; repeatIndex < repeatCount; repeatIndex++)
                    {
                        for (var threadIndex = 0; threadIndex < threadCount; threadIndex++)
                        {
                            var answerIndex = repeatIndex;
                            var addThread = new Thread(() =>
                            {
                                expectedResults[answerIndex] = _syncStringList.ToString();
                            });
                            addThread.Start();
                        }
                    }
                });
            });
        });

        "Никаких ошибок не возникает".x(() => exception.Should().BeNull());
        "Количество элементов не должно измениться".x(() => _syncStringList.Count.Should().Be(0));
        @"Все полученные строки должны быть равны 'пустой выдаче'".x(() =>
            Assert.All(expectedResults, item => item.Should().BeEquivalentTo("Empty")));
    }
}
