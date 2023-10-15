namespace TCPViaUDP.Helpers.NetworkBlockAcknowledger;

/// <summary>
/// Подтверждатор полученных блоков из сети
/// </summary>
public interface INetworkBlockAcknowledger
{
    /// <summary>
    /// Дождаться подтверждения и сообщить о результате.
    /// </summary>
    public Task WaitAcknowledgeAndFire(CancellationToken cancellationToken);
}
