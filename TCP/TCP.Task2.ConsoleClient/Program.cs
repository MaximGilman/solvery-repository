using System.Net;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Sender;

Console.WriteLine("TBD");
var loggingFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
var ip = IPAddress.Broadcast;
var port = 60381;
var sender = new DataBlockSender(ip, port, loggingFactory);
var path = "C:\\Users\\Alef Computers\\OneDrive\\Рабочий стол\\UC на запуски диска.txt";
await sender.StartHandleAsync(path, CancellationToken.None);
