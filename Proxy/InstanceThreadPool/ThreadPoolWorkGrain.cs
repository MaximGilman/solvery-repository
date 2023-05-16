namespace InstanceThreadPool;

internal class ThreadPoolWorkGrain
{
    internal ThreadPoolWorkGrain(object parameter, Action<object, CancellationToken> action, CancellationToken cancellationToken)
    {
        this._parameter = parameter;
        this._action = action;
        this._cancellationToken = cancellationToken;
    }

    internal void Deconstruct(out object parameter, out Action<object, CancellationToken> funcDelegate, out CancellationToken cancellationToken)
    {
        parameter = _parameter;
        funcDelegate = _action;
        cancellationToken = _cancellationToken;
    }

    private object _parameter { init; get; }
    private Action<object, CancellationToken> _action { init; get; }
    private CancellationToken _cancellationToken { init; get; }
}
