using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using TCPViaUDP.Helpers;

const int REMOTE_PORT_SEND = 60381;
const int REMOTE_PORT_RECEIVE = 60382;

//// Обсудить №3. Часть запросов не доходит до второго узла. Нужно посмотреть настройки брендмауера.
var serverIpAddress = IPAddress.Loopback;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
var logger = loggerFactory.CreateLogger<TCPViaUDPSender>();
var tcpViaUdpSender = new TCPViaUDPSender(serverIpAddress, REMOTE_PORT_SEND, REMOTE_PORT_RECEIVE, logger);

// Имитация данных.
var data = Enumerable.Repeat("1234567890", 10000);
var stringData = string.Join('\n', data);
var dataBuffer = Encoding.ASCII.GetBytes(stringData);
var stream = new MemoryStream(dataBuffer);

await tcpViaUdpSender.SendAsync(stream, CancellationToken.None);
