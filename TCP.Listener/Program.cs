using Microsoft.Extensions.Logging;
using TCP.Listener;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

var server = new MyTcpListener(loggerFactory, 80);
await server.Execute(CancellationToken.None);

