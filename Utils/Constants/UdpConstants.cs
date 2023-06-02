namespace Utils.Constants;

public static class UdpConstants
{
    public const int MAX_MTU_SIZE_IN_BYTES = 1500;
    public const int MAX_IP_HEADER_SIZE_IN_BYTES = 60;
    public const int UDP_HEADER_SIZE_IN_BYTES = 8;


    /// <summary>
    /// Максимальный "безопасный" размер пакета UDP.
    /// </summary>
    /// <remarks>MTU — (Max IP Header Size) — (UDP Header Size) = 1500 — 60 — 8 = 1432 bytes.</remarks>
    public const int MAX_UDP_DATA_SIZE_IN_BYTES = MAX_MTU_SIZE_IN_BYTES - MAX_IP_HEADER_SIZE_IN_BYTES - UDP_HEADER_SIZE_IN_BYTES;
}
