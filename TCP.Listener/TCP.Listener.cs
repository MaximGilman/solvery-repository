using System.Diagnostics;
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

            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("Waiting for a connection... ");

                using var client = await listener.AcceptTcpClientAsync(cancellationToken);
                Console.WriteLine("Connected");
                var stream = client.GetStream();

                int bytesRead;
                long totalBytesTransferred = 0;
                double currentTransferSpeed = 0;
                double averageransferSpeed = 0;
                var totalTime = TimeSpan.Zero;
                var sw = new Stopwatch();

                sw.Start();
                while ((bytesRead = await stream.ReadAsync(bytes, cancellationToken)) != 0)
                {
                    totalBytesTransferred += bytesRead;
                    currentTransferSpeed = bytesRead / sw.Elapsed.TotalSeconds;
                    totalTime += sw.Elapsed;

                    averageransferSpeed = totalBytesTransferred / totalTime.TotalSeconds;

                    var data = System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRead);
                    Console.WriteLine($"Received: {data}");
                    Console.WriteLine($"Inst. speed: {currentTransferSpeed} bytes/s");
                    Console.WriteLine($"Avg. speed: {averageransferSpeed} bytes/s");

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
}
