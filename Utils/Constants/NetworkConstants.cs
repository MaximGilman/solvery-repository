namespace Utils.Constants;

public static class NetworkConstants
{
    // ВОПРОС: Все же мы должны ограничивать по MTU или по UDP размеру?

    public const int MTU_MAX_BYTE_SIZE = 1500;
    public const int UDP_PACKET_MAX_BYTE_SIZE = 65507;


    /// <summary>
    /// Максимальный размер данных для блока UDP.
    /// </summary>
    /// <remarks>К реальным данным будет добавлен ИД блока, поэтому, чтобы влезть в UDP, размер уменьшен на размер типа ключа.</remarks>
    public const int UDP_DATA_BLOCK_MAX_BYTE_SIZE = UDP_PACKET_MAX_BYTE_SIZE - TypeSizeConstants.LONG_SIZE;

    /// <summary>
    /// Максимальный размер данных для отправки без фрагментации.
    /// </summary>
    /// <remarks>К реальным данным будет добавлен ИД блока, поэтому, чтобы влезть в MTU, размер уменьшен на размер типа ключа.</remarks>
    public const int MTU_DATA_BLOCK_MAX_BYTE_SIZE = MTU_MAX_BYTE_SIZE - TypeSizeConstants.LONG_SIZE;
}
