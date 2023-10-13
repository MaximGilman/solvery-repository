using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TCPViaUDP.Helpers.ConcurrentWindow;
using TCPViaUDP.Models;
using Xbehave;

namespace TCPviaUDP.Tests.Helpers.ConcurrentOnFlyDataBlockWindow;

public class ConcurrentOnFlyDataBlockWindowTests
{
    private IConcurrentOnFlyDataBlockWindow<int, int> _concurrentWindow;
    private ILogger<ConcurrentOnFlyDataBlockWindow<int, int>> _logger;
    private const int WINDOW_SIZE = 2;

    /// <summary>
    /// Для каждого сценария создаем окно.
    /// </summary>
    [Background]
    public void Init()
    {
        "Дано скользящее окно".x(() =>
        {
            _logger = new Logger<ConcurrentOnFlyDataBlockWindow<int, int>>(new LoggerFactory());
            _concurrentWindow = new ConcurrentOnFlyDataBlockWindow<int, int>(WINDOW_SIZE, _logger);
        });
    }

    #region TryAddTests

    [Scenario]
    public void TryAddBlock_Success(bool isAllAdded)
    {
        "Когда добавляются элементы".x(() =>
        {
            var block1 = new DataBlockWithId<int, int>(1, 1);
            var block2 = new DataBlockWithId<int, int>(2, 2);

            var isFirstAdded = _concurrentWindow.TryAddBlock(block1);
            var isSecondAdded = _concurrentWindow.TryAddBlock(block2);

            isAllAdded = isFirstAdded && isSecondAdded;
        });

        "Все элементы добавлены".x(() => Assert.True(isAllAdded));
        "Количество элементов верное".x(() => Assert.Equal(WINDOW_SIZE, _concurrentWindow.GetCurrentCount()));
    }

    [Scenario]
    public void TryAddBlock_WithExistedKey(bool isAllAdded)
    {
        "Когда добавляются элементы".x(() =>
        {
            var block1 = new DataBlockWithId<int, int>(1, 1);
            var block1WithSameKey = block1 with { Data = 2 };

            var isKeyAdded = _concurrentWindow.TryAddBlock(block1);
            var isKeyAddedAgain = _concurrentWindow.TryAddBlock(block1WithSameKey);

            isAllAdded = isKeyAdded && isKeyAddedAgain;
        });

        "Не элементы добавлены, повторный ключ перезаписал значение".x(() =>
        {
            Assert.True(isAllAdded);
            Assert.Equal(2, _concurrentWindow.GetValueOrDefault(1));
        });
        "Количество элементов верное".x(() => Assert.Equal(1, _concurrentWindow.GetCurrentCount()));
    }

    [Scenario]
    public void TryAddBlock_WithSizeMoreThanWindow(bool isFirstAdded, bool isSecondAdded, bool isThirdAdded, bool isAllAdded)
    {
        "Когда добавляются элементы".x(() =>
        {
            var block1 = new DataBlockWithId<int, int>(1, 1);
            var block2 = new DataBlockWithId<int, int>(2, 2);
            var block3 = new DataBlockWithId<int, int>(3, 3);

            isFirstAdded = _concurrentWindow.TryAddBlock(block1);
            isSecondAdded = _concurrentWindow.TryAddBlock(block2);
            isThirdAdded = _concurrentWindow.TryAddBlock(block3);
            isAllAdded = isFirstAdded && isSecondAdded && isThirdAdded;
        });

        "Не элементы добавлены, ключ, который не вошел в окно проигнорирован".x(() =>
        {
            Assert.False(isAllAdded);
            Assert.True(isFirstAdded);
            Assert.True(isSecondAdded);
            Assert.False(isThirdAdded);
        });
        "Количество элементов верное, окно не превышено".x(() => Assert.Equal(WINDOW_SIZE, _concurrentWindow.GetCurrentCount()));
    }

    #endregion

    #region TryGetValueTests

    [Scenario]
    public void TryGetValue_Success()
    {
        "Даны элементы".x(() =>
        {
            var block1 = new DataBlockWithId<int, int>(1, 1);
            var block2 = new DataBlockWithId<int, int>(2, 2);

            _concurrentWindow.TryAddBlock(block1);
            _concurrentWindow.TryAddBlock(block2);
        });

        "Значение по имеющемуся ключу находится".x(() =>
        {
            var foundFirstValue = _concurrentWindow.GetValueOrDefault(1);
            var foundSecondValue = _concurrentWindow.GetValueOrDefault(2);

            Assert.Equal(1, foundFirstValue);
            Assert.Equal(2, foundSecondValue);
        });
    }

    [Scenario]
    public void TryGetValue_WhenNotExistedKey()
    {
        "Даны элементы".x(() =>
        {
            var block1 = new DataBlockWithId<int, int>(1, 1);
            var block2 = new DataBlockWithId<int, int>(2, 2);

            _concurrentWindow.TryAddBlock(block1);
            _concurrentWindow.TryAddBlock(block2);
        });

        "Значение по отсутсвующему ключу не находится - возвращается значение по умолчанию".x(() =>
        {
            var notFountValue = _concurrentWindow.GetValueOrDefault(3);

            Assert.Equal(default, notFountValue);
        });
    }

    [Scenario]
    public void TryGetValue_WhenEmpty()
    {
        "Значение по отсутсвующему ключу не находится - возвращается значение по умолчанию".x(() =>
        {
            var notFountValue = _concurrentWindow.GetValueOrDefault(1);

            Assert.Equal(default, notFountValue);
        });
    }

    #endregion

    #region TryRemoveTests

    [Scenario]
    public void TryRemove_Success()
    {
        "Даны элементы".x(() =>
        {
            var block1 = new DataBlockWithId<int, int>(1, 1);
            var block2 = new DataBlockWithId<int, int>(2, 2);

            _concurrentWindow.TryAddBlock(block1);
            _concurrentWindow.TryAddBlock(block2);
        });

        "Когда удаляется существующий элемент".x(() => _concurrentWindow.TryRemove(1));
        "Количество элементов уменьшается".x(() => Assert.Equal(1, _concurrentWindow.GetCurrentCount()));
    }

    [Scenario]
    public void TryRemove_WhenNotExists()
    {
        "Даны элементы".x(() =>
        {
            var block1 = new DataBlockWithId<int, int>(1, 1);
            var block2 = new DataBlockWithId<int, int>(2, 2);

            _concurrentWindow.TryAddBlock(block1);
            _concurrentWindow.TryAddBlock(block2);
        });

        "Когда удаляется не существующий элемент".x(() => _concurrentWindow.TryRemove(3));
        "Количество элементов не уменьшается".x(() => Assert.Equal(2, _concurrentWindow.GetCurrentCount()));
    }

    [Scenario]
    public void TryRemove_WhenEmpty()
    {
        "Когда удаляется не существующий элемент".x(() => _concurrentWindow.TryRemove(1));
        "Количество элементов не уменьшается".x(() => Assert.Equal(0, _concurrentWindow.GetCurrentCount()));
    }

    #endregion

    #region ConcurrentTests

    [Scenario]
    public async Task TestConcurrentAccess(List<Task> tasks, int maxCount)
    {
        "Дан список задач".x(() => tasks = new List<Task>());
        "Дано значение для подсчета количества элементов в окне".x(() => maxCount = 0);

        "Когда запускаем несколько потоков на работу".x (() =>
        {
            for (int i = 1; i <= 1000; i++)
            {
                int id = i;
                tasks.Add(Task.Run(() =>
                {
                    _concurrentWindow.TryAddBlock(new DataBlockWithId<int, int>(id, id));
                    var value = _concurrentWindow.GetValueOrDefault(id);
                    maxCount = Math.Max(maxCount, _concurrentWindow.GetCurrentCount());
                    _concurrentWindow.TryRemove(id);
                }));
            }
        });
        "После окончания выполнения окно пустое".x(async () =>
        {
            await Task.WhenAll(tasks);
            Assert.Equal(0, _concurrentWindow.GetCurrentCount());
        });
        "Размер окна не был превышен".x(() =>
        {
            Assert.True(maxCount <= _concurrentWindow.GetWindowFrameSize());
        });
    }

    #endregion
}
