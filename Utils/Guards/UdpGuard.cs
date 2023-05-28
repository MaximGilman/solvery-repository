using Utils.Constants;

namespace Utils.Guards;

/// <summary>
/// UDP проверки.
/// </summary>
public static class UdpGuard
{
    /// <summary>
    /// Проверить, что размер буффера не привышает максимальный размер данных UDP датаграммы.
    /// </summary>
    public static void IsNoMaxDataSizeExceeded(int bufferLength)
    {
        if (bufferLength >= UdpConstants.MAX_UDP_DATA_SIZE_IN_BYTES)
        {
            throw new ArgumentException($"Значение данных в пакете UDP должно быть меньше указанного значения {bufferLength}", nameof(bufferLength));
        }
    }
}
