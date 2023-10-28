using System.Collections.Concurrent;
using System.Numerics;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Models;
using TCPViaUDP.Models.DataBlocks;

namespace TCPViaUDP.Helpers.ConcurrentWindow;

/// <summary>
/// Скользящее окно блоков данных в обработке.
/// </summary>
public abstract class ConcurrentBlockWindow<TKey, TValue> : IConcurrentBlockWindow<TKey, TValue> where TKey : INumber<TKey>
{
    public readonly AutoResetEvent OnCanAdd = new(true);

    private readonly int _windowFrameSize;
    protected readonly object _lock = new();
    protected readonly Dictionary<TKey, TValue> _blocksOnFly = new();
    private readonly ILogger<ConcurrentBlockWindow<TKey, TValue>> _logger;


    protected ConcurrentBlockWindow(int windowFrameSize, ILogger<ConcurrentBlockWindow<TKey, TValue>> logger)
    {
        _windowFrameSize = windowFrameSize;
        _logger = logger;
    }

    // TODO: Переписать все ошибки на русский
    public virtual bool TryAddBlock(DataBlockWithId<TKey, TValue> blockWithId)
    {
        lock (_lock)
        {
            if (_blocksOnFly.Count >= _windowFrameSize)
            {
                OnCanAdd.Reset();
                _logger.LogWarning("Блок с ключем {id} не был добавлен в скользящее окно, поскольку оно заполнено.", blockWithId.Id);
                return false;
            }


            _logger.LogInformation("Блок с ключем  {id} успешно добавлен в скользящее окно.", blockWithId.Id);
            _blocksOnFly[blockWithId.Id] = blockWithId.Block.Data;
            OnCanAdd.Set();
            return true;
        }
    }

    public virtual TValue GetValueOrDefault(TKey blockId)
    {
        lock (_lock)
        {
            var wasGet = _blocksOnFly.TryGetValue(blockId, out var value);
            if (wasGet)
            {
                _logger.LogInformation("Block with id {id} was read from window", blockId);
                return value;
            }
            else
            {
                _logger.LogInformation("Block with id {id} doesn't exists in window. Returned default value", blockId);
                return default;
            }
        }
    }

    public virtual int GetCurrentCount()
    {
        lock (_lock)
        {
            return _blocksOnFly.Count;
        }
    }

    public virtual int GetWindowFrameSize()
    {
        lock (_lock)
        {
            return _windowFrameSize;
        }
    }

    public virtual bool TryRemove(TKey blockId)
    {
        lock (_lock)
        {
            _logger.LogInformation("Block with id {id} was asked to remove from window", blockId);

            var isRemoved = _blocksOnFly.Remove(blockId, out _);
            if (isRemoved)
            {
                OnCanAdd.Set();
            }

            return isRemoved;
        }
    }
}
