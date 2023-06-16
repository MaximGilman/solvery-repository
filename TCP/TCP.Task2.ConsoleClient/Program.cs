using System.Net;
using Microsoft.Extensions.Logging;
using TCP.Task2.Client;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
const int SERVER_PORT = 2541;
var serverIpAddress = IPAddress.Parse("127.0.0.1");
var client = new Client(serverIpAddress, SERVER_PORT, loggerFactory);
await client.HandleSendingFile("Отсюда.txt", CancellationToken.None);
