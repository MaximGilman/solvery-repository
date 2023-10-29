using Utils.Guards;

namespace TCPViaUDP.Models.NetworkBlockReceiverResults;

public record AcknowledgeNetworkBlockResult(Memory<byte> Data) : DataNetworkBlockResult(Data)
{
    public long GetValue()
    {
        Guard.IsFalse(() => IsEmpty, "В блоке с подтверждением, должно быть значение");
        var key = BitConverter.ToInt64(Data.Span);
        Guard.IsGreaterOrEqual(key, 0);
        return key;
    }
}
