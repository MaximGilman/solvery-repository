using System.Collections.Concurrent;

namespace TCP.Utils.Helpers.BlockSelectors;

/// <summary>
/// Последовательный выбиратор блоков для сохранения.
/// </summary>
public class SequentialBlockSelector<TValue> : IBlockSelector<IEnumerable<TValue>>
{
    private readonly Func<IEnumerable<int>> _getKeys;
    private readonly Func<IEnumerable<int>, IEnumerable<TValue>> _getValues;
    private int _currentBlockId = 0;

    public SequentialBlockSelector(Func<IEnumerable<int>> getKeys, Func<IEnumerable<int>, IEnumerable<TValue>> getValues)
    {
        _getKeys = getKeys;
        _getValues = getValues;
    }

    public IEnumerable<TValue> SelectBlocksDataAsync()
    {
        var keys = GetSequentialKeysUntilPossible().ToList();
        _currentBlockId = keys.Max();
        return _getValues(keys);
    }

    private IEnumerable<int> GetSequentialKeysUntilPossible()
    {
        // Берем все ключи по порядку, пока берутся. Как только встречаем пробел в порядке ключей - останавливаемся.
        // 1, 2, 3, 4, 6, 7, 8 - возьмем 1 -4. Остальные пойдем загружать, когда появится 5й блок.
        var keys = _getKeys().ToList();
        return keys.SkipWhile(n => n != _currentBlockId)
            .TakeWhile((n, i) => i == 0 || n == keys[keys.IndexOf(_currentBlockId) + i - 1] + 1);
    }
}
