using System.Net.Sockets;
using Utils.Constants;
using Microsoft.Extensions.Logging;

namespace TCP.Utils;

public  class NetworkExceptionHandler
{    private ILogger _logger { get; }

    public NetworkExceptionHandler(ILogger logger)
    {
        _logger = logger;
    }

    public  void HandleException(Exception exception)
    {
        DoHandleException((dynamic)exception);
    }

    private void DoHandleException(SocketException exception)
    {
        _logger.LogError("Exception: {ex}. {help}", exception.Message,
            ExceptionHelpConstants.SocketExceptionHelpMessage);
    }

    private void DoHandleException(IOException exception)
    {
        _logger.LogError("Exception: {ex}. {help}", exception.Message, ExceptionHelpConstants.IOExceptionHelpMessage);
    }

    private void DoHandleException(OperationCanceledException exception)
    {
        _logger.LogError("Exception: {ex}. {help}", exception.Message,
            ExceptionHelpConstants.OperationCanceledExceptionHelpMessage);
    }

    private void DoHandleException(Exception exception)
    {
        _logger.LogError("Unhandled exception. {ex}", exception.Message);
    }
}
