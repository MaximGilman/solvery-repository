namespace Utils
{
    public static class ConsoleWriter
    {
        /// <summary>
        /// Флаг, указывающий, выводить ли информацию в консоль.
        /// </summary>
        private static readonly bool IsDebug = Utils.ConfigurationProvider.GetValue<bool>("isDebug");

        public static void WriteEvent(string message)
        {
            if (IsDebug)
            {
                Console.WriteLine(message);
            }
        }
    }
}