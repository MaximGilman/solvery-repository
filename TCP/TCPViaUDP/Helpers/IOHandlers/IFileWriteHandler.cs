namespace TCPViaUDP.Helpers.IOHandlers;

/// <summary>
/// Обработчик записи файла.
/// </summary>
public interface IFileWriteHandler
{
    /// <summary>
    /// Обработать запись файла.
    /// </summary>
    public Task HandleAsync(Memory<byte> data, CancellationToken cancellationToken);

}
