namespace TCP.Utils.Helpers;

public static class ReliableUdpConstants
{
    public const int FINISH_SEQUENCE_NUMBER = -1;
    public const int BYTE_BUFFER_SIZE = 1024;
    public const int BYTE_BUFFER_DATA_SIZE = BYTE_BUFFER_SIZE - sizeof(int);

    public static readonly Memory<byte> FinishSequenceMemory = new(BitConverter.GetBytes(FINISH_SEQUENCE_NUMBER));
}
