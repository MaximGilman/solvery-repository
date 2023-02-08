namespace Utils
{
    /// <summary>
    /// Класс для условного вывода на консоль.
    /// </summary>
    public static class ConsoleWriter
    {
        /// <summary>
        /// Флаг, указывающий, выводить ли информацию в консоль.
        /// </summary>
        private static readonly bool _isDebug = ConfigurationProvider.GetValue<bool>("isDebug");

        public static void WriteEvent(string message)
        {
            if (_isDebug)
            {
                Console.WriteLine(message);
            }
        }
    }
}