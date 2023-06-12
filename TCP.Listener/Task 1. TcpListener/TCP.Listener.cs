﻿using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Utils.Guards;

namespace TCP.Listener.Task_1._TcpListener;

public class MyTcpListener
{
    private ILogger _logger { get; }
    private TcpListener _server { get; }
    private readonly AverageCalculator _averageCalculator = new();
    private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Create(MAX_BUFFER_SIZE, MAX_BUCKET_ARRAY_AMOUNT);
    private readonly StopWatchMeasurer _stopWatchMeasurer = new();

    private const int BUFFER_SIZE = 256;
    private const int MAX_BUFFER_SIZE = BUFFER_SIZE * 2;
    private const int MAX_BUCKET_ARRAY_AMOUNT = 10;

    public MyTcpListener(ILogger logger, int? port = null)
    {
        this._logger = logger;

        if (port.HasValue)
        {
            Guard.IsValidClientPort(port.Value);
        }
        _server = TcpListener.Create(port ?? 0);
    }

    public int GetPort() => ((IPEndPoint)this._server.LocalEndpoint).Port;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Видится, что Array Pool вообще не нужен, т.к. мы аллоцируем 1 раз массив и его только заполняем / читаем.
        // Нам нет смысла его возвращать/рентить заново, если он уже аллоцирован в памяти и готов к работе - дешевле его уже держать, чем крутить цикл взятия/возврата.
        var bytesBuffer = _arrayPool.Rent(BUFFER_SIZE);
        try
        {
            this._server.Start();
            var ipEndpoint = (IPEndPoint)this._server.LocalEndpoint;
            this._logger.LogInformation("Listener start listening on {address}:{port}", ipEndpoint.Address, ipEndpoint.Port);


            while (!cancellationToken.IsCancellationRequested)
            {
                this._logger.LogInformation("Listener waiting for a connection...");

                using var tcpClient = await _server.AcceptTcpClientAsync(cancellationToken);
                this._logger.LogInformation("Tcp client connected");

                await using var stream = tcpClient.GetStream();

                var bytesRead = 0;
                while (true)
                {
                    (bytesRead, var elapsedTime) =
                        await this._stopWatchMeasurer.MeasureAsync(() => stream.ReadAsync(bytesBuffer, cancellationToken));

                    if (bytesRead == 0)
                    {
                        return;
                    }

                    _averageCalculator.AppendTotalTime(elapsedTime);
                    _averageCalculator.AppendTotalTransferredBytesAmount(bytesRead);

                    var receivedEncodedStreamData = System.Text.Encoding.ASCII.GetString(bytesBuffer, 0, bytesRead);

                    this._logger.LogInformation("Received: {data}", receivedEncodedStreamData);
                    this._logger.LogInformation("Inst. speed: {currentTransferSpeed} bytes/s",
                        AverageCalculator.CalculateCurrentSpeedValue(bytesRead, elapsedTime));
                    this._logger.LogInformation("Avg. speed: {averageTransferSpeed} bytes/s",
                        _averageCalculator.CalculateAverageSpeedValue());
                }
            }
        }
        catch (SocketException)
        {
            var endPoint = (IPEndPoint)this._server.LocalEndpoint;
            this._logger.LogError("Provided port {port} is busy. Try another or repeat later...", endPoint.Port);
            throw;
        }
        finally
        {
            _arrayPool.Return(bytesBuffer, true);
            _server.Stop();
            this._logger.LogInformation("Listener server stopped");
        }
    }
}
