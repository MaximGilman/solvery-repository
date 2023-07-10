namespace TCP.Task2.Listener;

public interface ITcpListener
{
    internal const int BYTE_BUFFER_SIZE = 4096 * 8;

    public Task HandleReceiveFile(string fileNameTarget, CancellationToken cancellationToken);
}
