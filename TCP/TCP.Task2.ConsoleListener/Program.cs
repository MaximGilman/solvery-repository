using Microsoft.Extensions.Logging;
using TCPViaUDP.Helpers;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
var logger = loggerFactory.CreateLogger<TCPViaUDPReceiver>();

const int PORT_RECEIVE = 60381;

var newVersionOfUdpReceiver = new TCPViaUDPReceiver(PORT_RECEIVE, logger);

await newVersionOfUdpReceiver.HandleAllReceiveAsync(CancellationToken.None);
