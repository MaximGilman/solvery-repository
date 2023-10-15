namespace TCPViaUDP.Helpers.IOHandlers;

/// <summary>
/// Обработчик чтения файла.
/// </summary>
public interface IFileReadHandler
{
    /// <summary>
    /// Обработать файл.
    /// </summary>
    public Task HandleAsync();
}
