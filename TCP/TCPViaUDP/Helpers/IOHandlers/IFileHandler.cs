namespace TCPViaUDP.Helpers.IOHandlers;

/// <summary>
/// Обработчик файла для получателя.
/// </summary>
public interface IFileHandler
{
    /// <summary>
    /// Обработать файл.
    /// </summary>
    public Task HandleAsync();
}
