using System.Net;
using Microsoft.Extensions.Logging;
using TCP.Listener.Task_2._File_sender;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

var server = new TCP_Server(loggerFactory, 13000);
var client = new TCP_Client(IPAddress.Loopback, 13000, loggerFactory);

await Task.WhenAll(
    Task.Run(async () => await server.HandleReceiveFile("Сюда.txt", CancellationToken.None), CancellationToken.None),
    Task.Run(async () => await client.HandleSendingFile("Отсюда.txt", CancellationToken.None), CancellationToken.None));
