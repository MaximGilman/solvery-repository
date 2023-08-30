using Microsoft.Extensions.Logging;
using TCP.Utils.Helpers;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
const int PORT = 2123;

var logger = loggerFactory.CreateLogger<NewVersionOfUdpReceiver>();
var newVersionOfUdpReceiver = new NewVersionOfUdpReceiver(PORT, logger);

await newVersionOfUdpReceiver.HandleAllReceiveAsync(CancellationToken.None);
