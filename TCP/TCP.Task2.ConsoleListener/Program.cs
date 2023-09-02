using Microsoft.Extensions.Logging;
using TCP.Utils.Helpers;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
const int PORT = 60381;
const int PORT_SEND = 60382;

var logger = loggerFactory.CreateLogger<NewVersionOfUdpReceiver>();
var newVersionOfUdpReceiver = new NewVersionOfUdpReceiver(PORT, PORT_SEND, logger);

await newVersionOfUdpReceiver.HandleAllReceiveAsync(CancellationToken.None);
