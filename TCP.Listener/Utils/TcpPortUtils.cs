using System.Net.NetworkInformation;
using Utils.Guards;

namespace TCP.Listener.Utils;

public static class TcpPortUtils
{
    public static bool IsPortAvailable(int portNumber)
    {
        return IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().All(p => p.Port != portNumber);
    }
}
