using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using TCP.Listener.Utils;
using Utils.Guards;

namespace TCP.Listener;

public static class MyTcpListener
{
    public static async Task Execute(int port, CancellationToken cancellationToken)
    {
        Guard.IsValidPort(port);
        var listener = createTcpListener(port);
        Console.WriteLine($"TcpListener created on {listener.LocalEndpoint}");

        try
        {
            Console.WriteLine("Start listening");
            listener.Start();

            var bytes = new byte[256];

            while (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Waiting for a connection... ");

                using var client = await listener.AcceptTcpClientAsync(cancellationToken);
                Console.WriteLine("Connected");
                var stream = client.GetStream();

                int bytesRead;
                long totalBytesTransferred = 0;
                double currentTransferSpeed = 0;
                double averageTransferSpeed = 0;
                var totalTime = TimeSpan.Zero;
                var sw = new Stopwatch();
                sw.Start();

                while ((bytesRead = await stream.ReadAsync(bytes, cancellationToken)) != 0)
                {
                    totalBytesTransferred += bytesRead;
                    currentTransferSpeed = bytesRead / sw.Elapsed.TotalSeconds;
                    totalTime += sw.Elapsed;

                    averageTransferSpeed = totalBytesTransferred / totalTime.TotalSeconds;

                    var data = System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRead);

                    Console.Clear();
                    Console.WriteLine($"Received: {data}");
                    Console.WriteLine($"Inst. speed: {currentTransferSpeed} bytes/s");
                    Console.WriteLine($"Avg. speed: {averageTransferSpeed} bytes/s");

                    if (!sw.IsRunning || sw.Elapsed.TotalSeconds > 1)
                    {
                        sw.Restart();
                    }
                }
            }
        }
        finally
        {
            listener.Stop();
        }
    }

    private static TcpListener createTcpListener(int port)
    {
        if (TcpPortUtils.IsTcpPortAvailable(port))
        {
            return new TcpListener(IPAddress.Loopback, port);
        }

        Console.WriteLine($"Port {port} was not available. Create on some other free");
        return new TcpListener(IPAddress.Loopback, 0);
    }
}
