using Microsoft.Extensions.Logging;
using TCP.Task2.Listener;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
const int PORT = 2123;
var server = new Listener(loggerFactory, PORT);
await server.HandleReceiveFile("Сюда.txt", CancellationToken.None);



Console.WriteLine("Для завершения работы нажмите Enter...");
Console.ReadLine();
