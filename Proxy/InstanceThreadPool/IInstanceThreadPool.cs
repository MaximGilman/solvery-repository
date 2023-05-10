namespace InstanceThreadPool;

/// <summary>
///
/// </summary>
public interface IInstanceThreadPool
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="action"></param>
    public void Execute(Action action);

    /// <summary>
    ///
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="action"></param>
    public void Execute(object parameter, Action<object> action);
}
