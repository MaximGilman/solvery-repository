namespace Utils.Guards
{
    /// <summary>
    /// Валидатор.
    /// </summary>
    public static partial class Guard
    {

        /// <summary>
        /// Проверить, что два значения равны.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="expectedValue">Текущее значение.</param>
        /// <param name="valueToCompare">Значение для сравнения.</param>
        public static void IsEqual<T>(T expectedValue, T valueToCompare)
        {
            if (expectedValue?.Equals(valueToCompare) != true &&
                Comparer<T>.Default.Compare(expectedValue, valueToCompare) != 0)
            {
                throw new ArgumentException($" {expectedValue} должно быть равно {valueToCompare}");
            }
        }

        /// <summary>
        /// Проверить, что два значения не равны.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="expectedValue">Текущее значение.</param>
        /// <param name="valueToCompare">Значение для сравнения.</param>
        public static void IsNotEqual<T>(T expectedValue, T valueToCompare)
        {
            if (expectedValue?.Equals(valueToCompare) == true ||
                Comparer<T>.Default.Compare(expectedValue, valueToCompare) == 0)
            {
                throw new ArgumentException($" {expectedValue} Не должно быть равно {valueToCompare}");
            }
        }

        /// <summary>
        /// Проверить, что значение не равно значению по-умолчанию.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="expectedValue">Текущее значение.</param>
        public static void IsNotDefault<T>(T expectedValue)
        {
            if (expectedValue?.Equals(default(T)) == true)
            {
                throw new ArgumentException($" {expectedValue} Не должно быть равно значению по умолчанию",
                    nameof(expectedValue));
            }
        }

        /// <summary>
        /// Проверить, что значение не равно значению null  .
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="expectedValue">Текущее значение.</param>
        public static void IsNotNull<T>(T expectedValue)
        {
            if (expectedValue == null)
            {
                throw new ArgumentException($" {expectedValue} Не должно быть равно null",
                    nameof(expectedValue));
            }
        }

        /// <summary>
        /// Проверить, что коллекция не пустая.
        /// </summary>
        /// <typeparam name="T">Тип элемента коллекции.</typeparam>
        public static void IsNotEmpty<T>(IEnumerable<T> collection)
        {
            if (collection?.Any() != true)
            {
                throw new ArgumentException($"Коллекция не должна быть пустой", nameof(collection));
            }
        }

        /// <summary>
        /// Проверить, что значение меньше или равно.
        /// </summary>
        /// <typeparam name="T">Тип сравниваемых элементов.</typeparam>
        public static void IsLess<T>(T valueToCheck, T valueToCompare) where T : IComparable
        {
            if (valueToCheck?.CompareTo(valueToCompare) >= 0 || Comparer<T>.Default.Compare(valueToCheck, valueToCompare) >= 0)
            {
                throw new ArgumentException($"Значение должно быть меньше указанного значения  {valueToCompare}", nameof(valueToCheck));
            }
        }

        /// <summary>
        /// Проверить, что значение меньше.
        /// </summary>
        /// <typeparam name="T">Тип сравниваемых элементов.</typeparam>
        public static void IsLessOrEqual<T>(T valueToCheck, T valueToCompare) where T : IComparable
        {
            if (valueToCheck?.CompareTo(valueToCompare) > 0 || Comparer<T>.Default.Compare(valueToCheck, valueToCompare) > 0)
            {
                throw new ArgumentException($"Значение должно быть меньше или равно указанного значения {valueToCompare}", nameof(valueToCheck));
            }
        }

        /// <summary>
        /// Проверить, что значение строго больше.
        /// </summary>
        /// <typeparam name="T">Тип сравниваемых элементов.</typeparam>
        public static void IsGreater<T>(T valueToCheck, T valueToCompare) where T : IComparable
        {
            if (valueToCheck?.CompareTo(valueToCompare) <= 0 || Comparer<T>.Default.Compare(valueToCheck, valueToCompare) <= 0)

            {
                throw new ArgumentException($"Значение должно быть больше указанного значения {valueToCompare}", nameof(valueToCheck));
            }
        }

        /// <summary>
        /// Проверить, что значение больше или равно.
        /// </summary>
        /// <typeparam name="T">Тип сравниваемых элементов.</typeparam>
        public static void IsGreaterOrEqual<T>(T valueToCheck, T valueToCompare) where T : IComparable
        {
            if (valueToCheck?.CompareTo(valueToCompare) < 0 || Comparer<T>.Default.Compare(valueToCheck, valueToCompare) < 0)
            {
                throw new ArgumentException($"Значение должно быть больше или равно указанного значения {valueToCompare}", nameof(valueToCheck));
            }
        }
    }
}
