namespace Utils
{
    /// <summary>
    /// Расширения.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Обрезать строку до указанной длины.
        /// </summary>
        /// <param name="str">Строка.</param>
        /// <param name="length">Максимальная длина.</param>
        public static string CropUpToLength(this string str, int length)
            => str.Length >= length ? str[..length] : str;
    }
}
