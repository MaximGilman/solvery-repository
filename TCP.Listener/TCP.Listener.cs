using System.Net;
using System.Net.Sockets;
using Utils.Guards;

namespace TCP.Listener;

public static class TCP_Listener
{
    private const int PORT = 13000;
    public static async Task Execute(CancellationToken cancellationToken)
    {
        Guard.IsValidPort(PORT);

        var listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, PORT));

        try
        {
            listener.Start();

            var bytes = new byte[256];
            string data = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("Waiting for a connection... ");

                using var client = await listener.AcceptTcpClientAsync(cancellationToken);
                Console.WriteLine("Connected");
                var stream = client.GetStream();

                int i;

                while ((i = await stream.ReadAsync(bytes, cancellationToken)) != 0)
                {
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("Received: {0}", data);
                }
            }
        }
        finally
        {
            listener.Stop();
        }
    }
}
