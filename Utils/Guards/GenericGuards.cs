namespace Utils.Guards
{
    /// <summary>
    /// Валидатор.
    /// </summary>
    public static partial class Guard
    {
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
        public static void IsLess<T>(T value, T maxValue) where T : IComparable
        {
            if (value?.CompareTo(maxValue) >= 0 || Comparer<T>.Default.Compare(value, maxValue) >= 0)
            {
                throw new ArgumentException($"Значение должно быть меньше указанного значения  {maxValue}", nameof(value));
            }
        }

        /// <summary>
        /// Проверить, что значение меньше.
        /// </summary>
        /// <typeparam name="T">Тип сравниваемых элементов.</typeparam>
        public static void IsLessOrEqual<T>(T value, T maxValue) where T : IComparable
        {
            if (value?.CompareTo(maxValue) > 0 || Comparer<T>.Default.Compare(value, maxValue) > 0)
            {
                throw new ArgumentException($"Значение должно быть меньше или равно указанного значения {maxValue}", nameof(value));
            }
        }

        /// <summary>
        /// Проверить, что значение входит в интервал.
        /// </summary>
        /// <param name="value">Сравниваемое значение.</param>
        /// <param name="leftBoundary">Минимальная граница диапазона.</param>
        /// <param name="rightBoundary">Максимальная граница диапазона.</param>
        /// <typeparam name="T">Тип сравниваемых элементов.</typeparam>
        public static void IsInInterval<T>(T value, T leftBoundary, T rightBoundary) where T : IComparable
        {
            IsGreater(value, leftBoundary);
            IsLess(value, rightBoundary);
        }

        /// <summary>
        /// Проверить, что значение входит в интервал.
        /// </summary>
        /// <param name="value">Сравниваемое значение.</param>
        /// <param name="leftBoundary">Минимальная граница диапазона.</param>
        /// <param name="rightBoundary">Максимальная граница диапазона.</param>
        /// /// <typeparam name="T">Тип сравниваемых элементов.</typeparam>
        public static void IsInIntervalOrEqual<T>(T value, T leftBoundary, T rightBoundary) where T : IComparable
        {
            IsGreaterOrEqual(value, leftBoundary);
            IsLessOrEqual(value, rightBoundary);
        }

        /// <summary>
        /// Проверить, что значение строго больше.
        /// </summary>
        /// <typeparam name="T">Тип сравниваемых элементов.</typeparam>
        public static void IsGreater<T>(T value, T minValue) where T : IComparable
        {
            if (value?.CompareTo(minValue) <= 0 || Comparer<T>.Default.Compare(value, minValue) <= 0)

            {
                throw new ArgumentException($"Значение должно быть больше указанного значения {minValue}", nameof(value));
            }
        }

        /// <summary>
        /// Проверить, что значение больше или равно.
        /// </summary>
        /// <typeparam name="T">Тип сравниваемых элементов.</typeparam>
        public static void IsGreaterOrEqual<T>(T value, T minValue) where T : IComparable
        {
            if (value?.CompareTo(minValue) < 0 || Comparer<T>.Default.Compare(value, minValue) < 0)
            {
                throw new ArgumentException($"Значение должно быть больше или равно указанного значения {minValue}", nameof(value));
            }
        }
    }
}
