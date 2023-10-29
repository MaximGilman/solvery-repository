﻿namespace TCPViaUDP.Helpers.IOHandlers;

/// <summary>
/// Обработчик чтения файла.
/// </summary>
public interface IFileReadHandler
{
    /// <summary>
    /// Обработать чтение файла.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="bufferSize"></param>
    /// <param name="cancellationToken"></param>
    public Task HandleAsync(string filePath, int bufferSize, CancellationToken cancellationToken);
}
