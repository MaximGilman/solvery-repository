namespace TCPViaUDP.Helpers.IOHandlers;

/// <summary>
/// Обработчик записи файла.
/// </summary>
public interface IFileWriteHandler
{
    /// <summary>
    /// Обработать запись файла.
    /// </summary>
    // TODO: Пока не ясно как на клиенте будет приходить файл.
    public Task HandleAsync();

}
