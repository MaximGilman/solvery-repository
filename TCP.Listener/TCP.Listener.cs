using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TCP.Listener.Utils;

namespace TCP.Listener;

public class MyTcpListener
{
    private ILogger _logger { get; }
    private TcpListener _server { get; }

    public MyTcpListener(ILoggerFactory loggerFactory, int? port = null)
    {
        this._logger = loggerFactory.CreateLogger<MyTcpListener>();

        try
        {
            if (port == null)
            {
                throw new ArgumentException("Port is not provided");
            }

            var listener = TcpListener.Create(port.Value);
            if (!TcpPortUtils.IsTcpPortAvailable(port.Value))
            {
                throw new ApplicationException("Port is busy");
            }
            _server = listener;
        }
        catch (ArgumentException)
        {
            _logger.LogWarning("Port is not provided. It will be set automatically on TcpListener.Start()");
            this._server = new TcpListener(new IPEndPoint(IPAddress.Any, port: 0));
        }
        catch
        {
            _logger.LogWarning("Error accured while accessing port: {port}. It will be set automatically on TcpListener.Start()", port);
            this._server = new TcpListener(new IPEndPoint(IPAddress.Any, port: 0));
        }
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        try
        {
            this._server.Start();
            var ipEndpoint = (IPEndPoint)this._server.LocalEndpoint;
            this._logger.LogInformation("Listener start listening on {address}:{port}", ipEndpoint.Address, ipEndpoint.Port);

            var bytes = new byte[256];

            while (!cancellationToken.IsCancellationRequested)
            {
                this._logger.LogInformation("Listener waiting for a connection...");

                using var client = await _server.AcceptTcpClientAsync(cancellationToken);
                this._logger.LogInformation("Tcp client connected");

                var stream = client.GetStream();

                int bytesRead;
                long totalBytesTransferred = 0;
                double currentTransferSpeed = 0;
                double averageTransferSpeed = 0;
                TimeSpan totalTime = TimeSpan.Zero;
                Stopwatch sw = new Stopwatch();
                sw.Start();

                while ((bytesRead = await stream.ReadAsync(bytes, cancellationToken)) != 0)
                {
                    totalBytesTransferred += bytesRead;
                    currentTransferSpeed = bytesRead / sw.Elapsed.TotalSeconds;
                    totalTime += sw.Elapsed;

                    averageTransferSpeed = totalBytesTransferred / totalTime.TotalSeconds;

                    var data =System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRead);

                    this._logger.LogInformation("Received: {data}", data);
                    this._logger.LogInformation("Inst. speed: {currentTransferSpeed} bytes/s", currentTransferSpeed);
                    this._logger.LogInformation("Avg. speed: {averageTransferSpeed} bytes/s", averageTransferSpeed);

                    if (!sw.IsRunning || sw.Elapsed.TotalSeconds > 1)
                    {
                        sw.Restart();
                    }
                }
            }
        }
        finally
        {
            _server.Stop();
            this._logger.LogInformation("Listener server stopped");

        }
    }
}
