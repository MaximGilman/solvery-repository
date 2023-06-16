using Microsoft.Extensions.Logging;
using TCP.Task2.Listener;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
var server = new Listener(loggerFactory);
await server.HandleReceiveFile("Сюда.txt", CancellationToken.None);
