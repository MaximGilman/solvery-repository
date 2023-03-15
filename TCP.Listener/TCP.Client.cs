﻿using System.Net;
using System.Net.Sockets;
using Utils.Guards;

namespace TCP.Listener;

public static class TCP_Client
{
    private const int PORT = 13001;

    public static async Task Execute(CancellationToken cancellationToken)
    {
        Guard.IsValidPort(PORT);

        try
        {
            var clientSocketEndPoint = new IPEndPoint(IPAddress.Loopback, PORT);
            Console.WriteLine("Start working...");
            using var client = new TcpClient(clientSocketEndPoint); // Создали клиента и забайндили на эндпоинт
            Console.WriteLine("TCP client bound");

            var targetEndPoint = new IPEndPoint(IPAddress.Loopback, PORT + 1);

            // Ожидается, что будет уже ждать с той стороны сервер.
            await client.ConnectAsync(targetEndPoint, cancellationToken);
            Console.WriteLine($"TCP client connected: {client.Connected}");

            await using var stream = client.GetStream();
            var bytesRead = 0;
            var buffer = new byte[512];

            Console.WriteLine("Receiving buffer and stream created");


            Console.WriteLine($"Start listening on {client.Client.RemoteEndPoint}");
            do
            {
                bytesRead += await stream.ReadAsync(buffer, cancellationToken);
                Console.WriteLine($"bytes read: {bytesRead}");

            } while (client.Available > 0);
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}