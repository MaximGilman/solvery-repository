
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class Guard
    {
        /// <summary>
        /// Проверить, что два значения не равны.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="expectedValue">Текущее значение.</param>
        /// <param name="valueToCompare">Значение для сравнения.</param>
        public static void IsNotEqual<T>(T? expectedValue, T? valueToCompare)
        {
            if (expectedValue?.Equals(valueToCompare) == true)
            {
                throw new ArgumentException($" {expectedValue} Не должно быть равно {valueToCompare}");
            }
        }
    
        /// <summary>
        /// Проверить, что значение не равно значению по-умолчанию.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="expectedValue">Текущее значение.</param>
        public static void IsNotDefault<T>(T? expectedValue)
        {
            if (expectedValue?.Equals(default(T)) == true)
            {
                throw new ArgumentException($" {expectedValue} Не должно быть равно значению по умолчанию", nameof(expectedValue));
            }
        }
    
        /// <summary>
        /// Проверить, что коллекция не пустая.
        /// </summary>
        /// <typeparam name="T">Тип элемента коллекции.</typeparam>
        public static void IsNotEmpty<T>(IEnumerable<T> collection)
        {
            if (!collection.Any())
            {
                throw new ArgumentException($"Коллекция не должна быть пустой", nameof(collection));
            }
        }
    }
}