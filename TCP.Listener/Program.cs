using Microsoft.Extensions.Logging;
using TCP.Listener;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

var server = new MyTcpListener(loggerFactory, 80);


await server.Execute(CancellationToken.None);

// Сервер на основе TcpClient. Не принимает подключения
// await TCP_Client.Execute(CancellationToken.None);
