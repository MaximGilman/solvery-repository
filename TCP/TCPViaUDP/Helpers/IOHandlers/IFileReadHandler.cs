namespace TCPViaUDP.Helpers.IOHandlers;

/// <summary>
/// Обработчик чтения файла.
/// </summary>
public interface IFileReadHandler
{
    /// <summary>
    /// Обработать чтение файла.
    /// </summary>
    public Task HandleAsync();
}
