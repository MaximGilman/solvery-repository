using Microsoft.Extensions.Logging;
using TCPViaUDP.Helpers.ConcurrentWindow;
using TCPViaUDP.Models;
using TCPViaUDP.Models.DataBlocks;
using Xbehave;

namespace TCPviaUDP.Tests.Helpers.ConcurrentDataBlockWindow;

public class ConcurrentDataBlockWindowTests
{
    private IAcknowledgedConcurrentBlockWindow<long, Memory<byte>> _acknowledgedConcurrentWindow;
    private ILogger<AcknowledgedConcurrentBlockWindow<long, Memory<byte>>> _logger;
    private const int WINDOW_SIZE = 2;
    private Memory<byte> _value;

    /// <summary>
    /// Для каждого сценария создаем окно.
    /// </summary>
    [Background]
    public void Init()
    {
        "Дано скользящее окно".x(() =>
        {
            _logger = new Logger<AcknowledgedConcurrentBlockWindow<long, Memory<byte>>>(new LoggerFactory());
            _acknowledgedConcurrentWindow = new LongKeyMemoryByteAcknowledgedConcurrentBlockWindow(WINDOW_SIZE, _logger);
            _value = "Some value"u8.ToArray();
        });
    }

    #region TryAddTests

    [Scenario]
    public void TryAddBlock_Success(bool isAllAdded)
    {
        "Когда добавляются элементы".x(() =>
        {
            var block1 = new LongKeyMemoryByteDataBlock(1, _value);
            var block2 = new LongKeyMemoryByteDataBlock(2, _value);

            var isFirstAdded = _acknowledgedConcurrentWindow.TryAddBlock(block1);
            var isSecondAdded = _acknowledgedConcurrentWindow.TryAddBlock(block2);

            isAllAdded = isFirstAdded && isSecondAdded;
        });

        "Все элементы добавлены".x(() => Assert.True(isAllAdded));
        "Количество элементов верное".x(() => Assert.Equal(WINDOW_SIZE, _acknowledgedConcurrentWindow.GetCurrentCount()));
    }

    [Scenario]
    public void TryAddBlock_WithExistedKey(bool isAllAdded)
    {
        "Когда добавляются элементы".x(() =>
        {
            var block1 = new LongKeyMemoryByteDataBlock(1, _value);
            var block1WithSameKey = block1 with { Block = new DataBlock<Memory<byte>>(_value) };

            var isKeyAdded = _acknowledgedConcurrentWindow.TryAddBlock(block1);
            var isKeyAddedAgain = _acknowledgedConcurrentWindow.TryAddBlock(block1WithSameKey);

            isAllAdded = isKeyAdded && isKeyAddedAgain;
        });

        "Не элементы добавлены, повторный ключ перезаписал значение".x(() =>
        {
            Assert.True(isAllAdded);
            Assert.Equal(_value, _acknowledgedConcurrentWindow.GetValueOrDefault(1));
        });
        "Количество элементов верное".x(() => Assert.Equal(1, _acknowledgedConcurrentWindow.GetCurrentCount()));
    }

    [Scenario]
    public void TryAddBlock_WithSizeMoreThanWindow(bool isFirstAdded, bool isSecondAdded, bool isThirdAdded, bool isAllAdded)
    {
        "Когда добавляются элементы".x(() =>
        {
            var block1 = new LongKeyMemoryByteDataBlock(1, _value);
            var block2 = new LongKeyMemoryByteDataBlock(2, _value);
            var block3 = new LongKeyMemoryByteDataBlock(3, _value);

            isFirstAdded = _acknowledgedConcurrentWindow.TryAddBlock(block1);
            isSecondAdded = _acknowledgedConcurrentWindow.TryAddBlock(block2);
            isThirdAdded = _acknowledgedConcurrentWindow.TryAddBlock(block3);
            isAllAdded = isFirstAdded && isSecondAdded && isThirdAdded;
        });

        "Не элементы добавлены, ключ, который не вошел в окно проигнорирован".x(() =>
        {
            Assert.False(isAllAdded);
            Assert.True(isFirstAdded);
            Assert.True(isSecondAdded);
            Assert.False(isThirdAdded);
        });
        "Количество элементов верное, окно не превышено".x(() => Assert.Equal(WINDOW_SIZE, _acknowledgedConcurrentWindow.GetCurrentCount()));
    }

    #endregion

    #region TryGetValueTests

    [Scenario]
    public void TryGetValue_Success()
    {
        "Даны элементы".x(() =>
        {
            var block1 = new LongKeyMemoryByteDataBlock(1, _value);
            var block2 = new LongKeyMemoryByteDataBlock(2, _value);

            _acknowledgedConcurrentWindow.TryAddBlock(block1);
            _acknowledgedConcurrentWindow.TryAddBlock(block2);
        });

        "Значение по имеющемуся ключу находится".x(() =>
        {
            var foundFirstValue = _acknowledgedConcurrentWindow.GetValueOrDefault(1);
            var foundSecondValue = _acknowledgedConcurrentWindow.GetValueOrDefault(2);

            Assert.Equal(_value, foundFirstValue);
            Assert.Equal(_value, foundSecondValue);
        });
    }

    [Scenario]
    public void TryGetValue_WhenNotExistedKey()
    {
        "Даны элементы".x(() =>
        {
            var block1 = new LongKeyMemoryByteDataBlock(1, _value);
            var block2 = new LongKeyMemoryByteDataBlock(2, _value);

            _acknowledgedConcurrentWindow.TryAddBlock(block1);
            _acknowledgedConcurrentWindow.TryAddBlock(block2);
        });

        "Значение по отсутсвующему ключу не находится - возвращается значение по умолчанию".x(() =>
        {
            var notFoundValue = _acknowledgedConcurrentWindow.GetValueOrDefault(3);

            Assert.Equal(default, notFoundValue);
        });
    }

    [Scenario]
    public void TryGetValue_WhenEmpty()
    {
        "Значение по отсутсвующему ключу не находится - возвращается значение по умолчанию".x(() =>
        {
            var notFountValue = _acknowledgedConcurrentWindow.GetValueOrDefault(1);

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
            var block1 = new LongKeyMemoryByteDataBlock(1, _value);
            var block2 = new LongKeyMemoryByteDataBlock(2, _value);

            _acknowledgedConcurrentWindow.TryAddBlock(block1);
            _acknowledgedConcurrentWindow.TryAddBlock(block2);
        });

        "Когда удаляется существующий элемент".x(() => _acknowledgedConcurrentWindow.TryRemove(1));
        "Количество элементов уменьшается".x(() => Assert.Equal(1, _acknowledgedConcurrentWindow.GetCurrentCount()));
    }

    [Scenario]
    public void TryRemove_WhenNotExists()
    {
        "Даны элементы".x(() =>
        {
            var block1 = new LongKeyMemoryByteDataBlock(1, _value);
            var block2 = new LongKeyMemoryByteDataBlock(2, _value);

            _acknowledgedConcurrentWindow.TryAddBlock(block1);
            _acknowledgedConcurrentWindow.TryAddBlock(block2);
        });

        "Когда удаляется не существующий элемент".x(() => _acknowledgedConcurrentWindow.TryRemove(3));
        "Количество элементов не уменьшается".x(() => Assert.Equal(2, _acknowledgedConcurrentWindow.GetCurrentCount()));
    }

    [Scenario]
    public void TryRemove_WhenEmpty()
    {
        "Когда удаляется не существующий элемент".x(() => _acknowledgedConcurrentWindow.TryRemove(1));
        "Количество элементов не уменьшается".x(() => Assert.Equal(0, _acknowledgedConcurrentWindow.GetCurrentCount()));
    }

    #endregion

    #region ConcurrentTests

    [Scenario]
    public void TestConcurrentAccess(List<Task> tasks, int maxCount)
    {
        "Дан список задач".x(() => tasks = new List<Task>());
        "Дано значение для подсчета количества элементов в окне".x(() => maxCount = 0);

        "Когда запускаем несколько потоков на работу".x(() =>
        {
            for (int i = 1; i <= 1000; i++)
            {
                int id = i;
                // Поток для добавления и чтения.
                tasks.Add(Task.Run(() =>
                {
                    _acknowledgedConcurrentWindow.TryAddBlock(new LongKeyMemoryByteDataBlock(id, _value));
                    _acknowledgedConcurrentWindow.GetValueOrDefault(id);
                    maxCount = Math.Max(maxCount, _acknowledgedConcurrentWindow.GetCurrentCount());
                }));

                // Поток для удаления.
                tasks.Add(Task.Run(() =>
                {
                    var deletedSuccessfully = false;
                    while (!deletedSuccessfully)
                    {
                        if (!_acknowledgedConcurrentWindow.GetValueOrDefault(id).IsEmpty)
                        {
                            deletedSuccessfully =  _acknowledgedConcurrentWindow.TryRemove(id);
                        }
                    }
                }));
            }
        });
        "После окончания выполнения окно пустое".x(async () =>
        {
            await Task.WhenAll(tasks);
            Assert.Equal(0, _acknowledgedConcurrentWindow.GetCurrentCount());
        });
        "Размер окна не был превышен".x(() => { Assert.True(maxCount <= _acknowledgedConcurrentWindow.GetWindowFrameSize()); });
    }

    #endregion
}
