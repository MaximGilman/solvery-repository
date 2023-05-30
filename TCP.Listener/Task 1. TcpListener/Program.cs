using Microsoft.Extensions.Logging;
using TCP.Listener.Task_1._TcpListener;

var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

try
{
    var server = new MyTcpListener(loggerFactory.CreateLogger<MyTcpListener>(), 8080);
    await server.Execute(CancellationToken.None);

}
catch (Exception ex)
{
    loggerFactory.CreateLogger<Program>().LogError("Возникло исключение {exception}", ex.Message);
}