using System.Numerics;

namespace TCPViaUDP.Helpers.BlockSelectors;

/// <summary>
/// Последовательный выбиратор блоков для сохранения.
/// </summary>
public class SequentialBlockSelector<TKey, TValue> : IBlockSelector<IEnumerable<TValue>> where TKey: INumberBase<TKey>
{
    private readonly Func<IEnumerable<TKey>> _getKeys;
    private readonly Func<IEnumerable<TKey>, IEnumerable<TValue>> _getValues;
    private TKey _currentBlockId = TKey.Zero;

    public SequentialBlockSelector(Func<IEnumerable<TKey>> getKeys, Func<IEnumerable<TKey>, IEnumerable<TValue>> getValues)
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

    private IEnumerable<TKey> GetSequentialKeysUntilPossible()
    {
        // Берем все ключи по порядку, пока берутся. Как только встречаем пробел в порядке ключей - останавливаемся.
        // 1, 2, 3, 4, 6, 7, 8 - возьмем 1 -4. Остальные пойдем загружать, когда появится 5й блок.
        var keys = _getKeys().Order().ToList();
        return keys.SkipWhile(n => n != _currentBlockId)
            .TakeWhile((n, i) => i == 0 || n == keys[keys.IndexOf(_currentBlockId) + i - 1] + TKey.One);
    }
}
