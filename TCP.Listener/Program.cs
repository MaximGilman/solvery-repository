using TCP.Listener;



// Сервер на основе TcpListener. Принимает подключения
// await TCP_Listener.Execute(CancellationToken.None);

// Сервер на основе TcpClient. Не принимает подключения
await TCP_Client.Execute(CancellationToken.None);
