namespace TCPViaUDP.Models.NetworkBlockReceiverResults;

public class DataNetworkBlockResult : NetworkBlockResultBase
{
    public Memory<byte> Data { get; init; }
}
