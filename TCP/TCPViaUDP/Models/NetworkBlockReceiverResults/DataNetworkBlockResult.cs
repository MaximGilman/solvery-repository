namespace TCPViaUDP.Models.NetworkBlockReceiverResults;

public record DataNetworkBlockResult(Memory<byte> Data) : NetworkBlockResultBase(Data.IsEmpty)
{
}
