namespace TCP.Task2.Listener;

public interface ITcpListener
{
    public Task HandleReceiveFile(string fileNameTarget, CancellationToken cancellationToken);
}
