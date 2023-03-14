using System.Net;

namespace Utils.Guards;

public partial class Guard
{

    /// <summary>
    /// Проверить, что строка не пустая.
    /// </summary>
    /// <param name="port">Текущее значение.</param>
    public static void IsValidPort(int port)
    {
        Guard.IsGreater(port, IPEndPoint.MinPort);
        Guard.IsLess(port, IPEndPoint.MaxPort);
    }
}
