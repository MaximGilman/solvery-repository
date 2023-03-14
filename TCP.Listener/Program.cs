
using System.Net;
using System.Net.Sockets;
using TCP.Listener;
using Utils.Guards;

const int PORT = 8889;
Guard.IsValidPort(PORT);


try
{
    var ipEndPoint = new IPEndPoint(IPAddress.Loopback, PORT);

    Console.WriteLine("Start working...");
    using var client = new TcpClient(ipEndPoint);
    Console.WriteLine("TCP client bound");

    await client.ConnectAsync(IPAddress.Loopback, PORT);

    Console.WriteLine("TCP client connected");

    await using var stream = client.GetStream();
    var bytesRead = 0;
    var buffer = new byte[512];

    Console.WriteLine("Receiving buffer and stream created");
    Console.WriteLine($"Start listening on {client.Client.RemoteEndPoint}");
    do
    {
        bytesRead += stream.Read(buffer);
        Console.WriteLine($"bytes read: {bytesRead}");

    } while (client.Available > 0);
}

catch (Exception ex)
{
    Console.WriteLine(ex);
}