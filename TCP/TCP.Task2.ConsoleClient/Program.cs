using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using TCP.Utils.Helpers;

const int SERVER_PORT = 60381;
const int SERVER_PORT_RECEIVE = 60382;
var serverIpAddress = IPAddress.Broadcast;
var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
var logger = loggerFactory.CreateLogger<NewVersionOfUdpSender>();
var tcpViaUdpSender = new NewVersionOfUdpSender(serverIpAddress, SERVER_PORT, SERVER_PORT_RECEIVE, logger);

var data =
    "1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 " +
    "1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 " +
    "1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890";
var dataBuffer = Encoding.ASCII.GetBytes(data);
MemoryStream stream = new MemoryStream(dataBuffer);

await tcpViaUdpSender.SendAsync(stream, CancellationToken.None);
