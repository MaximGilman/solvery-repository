using System.Net.NetworkInformation;

namespace TCP.Utils;

public static class TcpPortUtils
{
    public static bool IsTcpPortAvailable(int portNumber)
    {
        return IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().All(p => p.Port != portNumber);
    }
}
