using Microsoft.Extensions.Logging;
using TCPViaUDP.Receiver;

// TODO:
// Время ожидания сделать динамическим!!!
// Поставили константку, затем меряем скорость

// File read handler можно рассмотреть цепочка ответственности. Только прочитать, не исправлять


const int port = 60381;
var loggingFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

var receiver = new DataBlockReceiver(port, loggingFactory);

await receiver.StartHandleAsync(CancellationToken.None);
