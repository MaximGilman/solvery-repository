using System.Collections.Concurrent;
using System.Numerics;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Models;
using TCPViaUDP.Models.DataBlocks;

namespace TCPViaUDP.Helpers.ConcurrentWindow;

/// <summary>
/// Скользящее окно блоков данных в обработке.
/// </summary>
public abstract class ConcurrentDataBlockWindow<TKey, TValue> : IConcurrentDataBlockWindow<TKey, TValue> where TKey: INumber<TKey>
{
    private readonly int _windowFrameSize;
    private readonly object _lock = new();
    protected readonly ConcurrentDictionary<TKey, TValue> _blocksOnFly = new();
    private readonly ILogger<ConcurrentDataBlockWindow<TKey, TValue>> _logger;

    protected ConcurrentDataBlockWindow(int windowFrameSize, ILogger<ConcurrentDataBlockWindow<TKey, TValue>> logger)
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
                _logger.LogInformation("Block with id {id} was not added to window due to it's size exceeded", blockWithId.Id);
                return false;
            }


            _logger.LogInformation("Block with id {id} was added to window", blockWithId.Id);
            _blocksOnFly.AddOrUpdate(blockWithId.Id, blockWithId.Block.Data, (_, _) => blockWithId.Block.Data);
            return true;
        }
    }

    public virtual TValue GetValueOrDefault(TKey blockId)
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

    public virtual int GetCurrentCount() => _blocksOnFly.Count;

    public virtual int GetWindowFrameSize() => _windowFrameSize;

    public virtual bool TryRemove(TKey blockId)
    {
        _logger.LogInformation("Block with id {id} was asked to remove from window", blockId);
        return _blocksOnFly.TryRemove(blockId, out _);
    }
}
