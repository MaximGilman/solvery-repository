using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TCP.Listener.Task_1._TcpListener;
using Xunit;
using Moq;

namespace Task1.TcpListener.Tests;

public class TcpListenerTests
{
    [Fact]
    public async Task Execute_Success()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var listener = new MyTcpListener(logger.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        const string TEST_MESSAGE = "Test message";

        // Act
        var task = listener.ExecuteAsync(cancellationToken);
        await Task.Delay(1000, cancellationToken: cancellationToken);
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, listener.GetPort(), cancellationToken);

        var netStream = client.GetStream();
        var sendBuffer = Encoding.UTF8.GetBytes(TEST_MESSAGE);
        netStream.Write(sendBuffer);

        // Assert
        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(6));

        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString().StartsWith("Listener start listening on ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Listener waiting for a connection..."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Tcp client connected"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == $"Received: {TEST_MESSAGE}"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString().StartsWith("Inst. speed:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString().StartsWith("Avg. speed:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

 [Fact]
    public async Task Execute_Error()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var listener = new MyTcpListener(logger.Object);

        // Act
        var task = listener.ExecuteAsync(CancellationToken.None);
        var samePortListener =  new MyTcpListener(logger.Object, listener.GetPort());

        // Assert
        await Assert.ThrowsAsync<SocketException>(async () =>
        {
            await samePortListener.ExecuteAsync(CancellationToken.None);
        });
    }

    [Fact]
    public async Task Execute_Cancel()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var listener = new MyTcpListener(logger.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        var task = listener.ExecuteAsync(cancellationToken);
        await Task.Delay(1000, cancellationToken: cancellationToken);
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, listener.GetPort(), cancellationToken);
        cancellationTokenSource.CancelAfter(5000);

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);


        // Assert
        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString().StartsWith("Listener start listening on ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Listener waiting for a connection..."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Tcp client connected"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Listener server stopped"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
