namespace Utils.Constants;

public static class ExceptionHelpConstants
{
    public const string SocketExceptionHelpMessage =
        "You should handle this exception by retrying the operation or terminating the connection gracefully";

    public const string IOExceptionHelpMessage =
        "Check the file exists on the provided path and retry operation";

    public const string OperationCanceledExceptionHelpMessage =
        "You should handle this exception by checking whether the operation was canceled and taking appropriate action";
}
