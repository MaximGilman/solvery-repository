using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using TCP.Listener.Task_1._TcpListener;
using Xunit;
using Xbehave;
using Moq;

namespace Task1.TcpListener.Tests;

public class UnitTest1
{
    private MyTcpListener _tcpListener;
    private CancellationTokenSource _cancellationTokenSource;
    private byte[] _messageBytes = System.Text.Encoding.ASCII.GetBytes("Test message");
    private Mock<Logger<MyTcpListener>> _logger;

    [Background]
    public void Init()
    {
        _logger = new Mock<Logger<MyTcpListener>>();

        _tcpListener = new MyTcpListener(_logger.Object);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [Scenario]
    public void ShouldListenConnections(TcpClient client)
    {
        "Когда слушатель начинает слушать".x(() => _tcpListener.Execute(_cancellationTokenSource.Token));

        "И подключается второй клиент".x(async () =>
        {
            client = new TcpClient();
            await client.ConnectAsync("localhost", _tcpListener.GetPort());
        });

        "Когда отправляется сообщение и заканчиается выполнение".x(async () =>
        {
            await client.GetStream().WriteAsync(_messageBytes);
            _cancellationTokenSource.Cancel();
            client.Dispose();
        });

        "Клиент получил сообщение и отписался в лог".x(() =>
        {
            _logger.Verify(
                x => x.LogInformation(""),
                Times.Exactly(6));

        });
    }
}
