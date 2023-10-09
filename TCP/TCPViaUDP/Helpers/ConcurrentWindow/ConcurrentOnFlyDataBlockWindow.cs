﻿using System.Collections.Concurrent;
using System.Numerics;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Models;

namespace TCPViaUDP.Helpers.ConcurrentWindow;

/// <summary>
/// Скользящее окно блоков данных в обработке.
/// </summary>
public class ConcurrentOnFlyDataBlockWindow<TKey, TValue> : IConcurrentOnFlyDataBlockWindow<TKey, TValue> where TKey: INumber<TKey>
{
    private readonly int _windowFrameSize;
    private readonly object _lock = new();
    private readonly ConcurrentDictionary<TKey, TValue> _blocksOnFly = new();
    private readonly ILogger<ConcurrentOnFlyDataBlockWindow<TKey, TValue>> _logger;

    public ConcurrentOnFlyDataBlockWindow(int windowFrameSize, ILogger<ConcurrentOnFlyDataBlockWindow<TKey, TValue>> logger)
    {
        _windowFrameSize = windowFrameSize;
        _logger = logger;
    }

    public bool TryAddBlock(DataBlockWithId<TKey, TValue> blockWithId)
    {

        lock (_lock)
        {
            if (_blocksOnFly.Count >= _windowFrameSize)
            {
                _logger.LogInformation("Block with id {id} was not added to window due to it's size exceeded", blockWithId.BlockId);
                return false;
            }


            _logger.LogInformation("Block with id {id} was added to window", blockWithId.BlockId);
            _blocksOnFly.AddOrUpdate(blockWithId.BlockId, blockWithId.Data, (_, _) => blockWithId.Data);
            return true;
        }
    }

    public TValue GetValueOrDefault(TKey blockId)
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

    public int GetCurrentCount() => _blocksOnFly.Count;

    public int GetWindowFrameSize() => _windowFrameSize;

    public bool TryRemove(TKey blockId)
    {
        _logger.LogInformation("Block with id {id} was asked to remove from window", blockId);
        return _blocksOnFly.TryRemove(blockId, out _);
    }
}
