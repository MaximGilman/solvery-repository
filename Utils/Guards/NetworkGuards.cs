using System.Net;

namespace Utils.Guards;

public partial class Guard
{
    private const int MIN_PORT = 1024;
    private const int MAX_PORT = 65535;
    /// <summary>
    /// Проверить, что порт находится в пределах клиентских портов.
    /// </summary>
    /// <param name="port">Текущее значение.</param>
    public static void IsValidClientPort(int port)
    {
        Guard.IsGreater(port, MIN_PORT);
        Guard.IsLess(port, MAX_PORT);
    }
}
