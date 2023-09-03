using Microsoft.Extensions.Logging;
using TCP.Utils.Helpers;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
const int PORT = 60381;

var logger = loggerFactory.CreateLogger<TCPViaUDPReceiver>();
var newVersionOfUdpReceiver = new TCPViaUDPReceiver(PORT, logger);

await newVersionOfUdpReceiver.HandleAllReceiveAsync(CancellationToken.None);
