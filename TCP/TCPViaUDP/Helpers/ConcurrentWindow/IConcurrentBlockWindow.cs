﻿using System.Numerics;
using TCPViaUDP.Models;
using TCPViaUDP.Models.DataBlocks;

namespace TCPViaUDP.Helpers.ConcurrentWindow;

/// <summary>
/// Скользящее окно блоков данных в обработке.
/// </summary>
public interface IConcurrentBlockWindow<TKey, TValue> where TKey : INumber<TKey>
{
    /// <summary>
    /// Попытаться добавить блок по ключу.
    /// </summary>
    /// <param name="blockWithId">Блок с данными и ключем.</param>
    public bool TryAddBlock(DataBlockWithId<TKey, TValue> blockWithId);

    /// <summary>
    /// Получить текущее количество элементов.
    /// </summary>
    public int GetCurrentCount();

    /// <summary>
    /// Получить размер скользящего окна.
    /// </summary>
    public int GetWindowFrameSize();

    /// <summary>
    /// Получить значение по ключу.
    /// </summary>
    /// <param name="key">Ключ блока.</param>
    /// <returns>Значение блока по ключу или default, если не найдено.</returns>
    public TValue GetValueOrDefault(TKey key);

    /// <summary>
    /// Получить первое по порядку значение.
    /// </summary>
    public (TKey, TValue) GetFirstValueOrDefault();

    /// <summary>
    /// Удалить значение по ключу.
    /// </summary>
    /// <param name="key">Ключ блока.</param>
    public bool TryRemove(TKey key);
}