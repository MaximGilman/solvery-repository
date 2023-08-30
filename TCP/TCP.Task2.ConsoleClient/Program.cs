using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using TCP.Task2.Client;
using TCP.Utils.Helpers;
using System;

const int SERVER_PORT = 2123;
var serverIpAddress = IPAddress.Parse("127.0.0.1");
var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
var logger = loggerFactory.CreateLogger<NewVersionOfUdpSender>();
var tcpViaUdpSender = new NewVersionOfUdpSender(serverIpAddress, SERVER_PORT, logger);

var data =
    "1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 " +
    "1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 " +
    "1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890";
var dataBuffer = Encoding.ASCII.GetBytes(data);
MemoryStream stream = new MemoryStream(dataBuffer);

await tcpViaUdpSender.SendAsync(stream, CancellationToken.None);
