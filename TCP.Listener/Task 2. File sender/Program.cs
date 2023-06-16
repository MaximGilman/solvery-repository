using System.Net;
using Microsoft.Extensions.Logging;
using TCP.Listener.Task_2._File_sender;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

var serverPort = 13000;
var clientPort = 13000;

var server = new FileSenderTcpServer(loggerFactory, serverPort);
var client = new FileSenderTcpClient(IPAddress.Loopback, clientPort, loggerFactory);

await Task.WhenAll(
    Task.Run(async () => await server.HandleReceiveFile("Сюда.txt", CancellationToken.None), CancellationToken.None),
    Task.Run(async () => await client.HandleSendingFile("Отсюда.txt", CancellationToken.None), CancellationToken.None));
