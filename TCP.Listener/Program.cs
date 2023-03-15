using TCP.Listener;

// Сервер на основе TcpListener. Принимает подключения
await MyTcpListener.Execute(13000, CancellationToken.None);

// Сервер на основе TcpClient. Не принимает подключения
// await TCP_Client.Execute(CancellationToken.None);
