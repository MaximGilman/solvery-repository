namespace InstanceThreadPool;

/// <summary>
/// Пул потоков, допускающий создание экземпляров.
/// </summary>
public interface IInstanceThreadPool
{
    /// <summary>
    /// Выполнить действие без параметров с возможностью отмены.
    /// </summary>
    /// <param name="action">Действие для выполнения.</param>
    /// <exception cref="Exception">При возникновении исключения во время выполнения действия.</exception>
    public void QueueExecute(Action action);

    /// <summary>
    /// Выполнить действие с параметром.
    /// </summary>
    /// <param name="parameter">Параметр для действия.</param>
    /// <param name="action">Действие для выполнения.</param>
    /// <exception cref="Exception">При возникновении исключения во время выполнения действия.</exception>
    public void QueueExecute(object parameter, Action<object> action);

    /// <summary>
    /// Выполнить действие без параметров с возможностью отмены.
    /// </summary>
    /// <param name="funcDelegate">Действие для выполнения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <exception cref="Exception">При возникновении исключения во время выполнения действия.</exception>
    public void QueueExecute(Action<CancellationToken> funcDelegate, CancellationToken cancellationToken);

    /// <summary>
    /// Выполнить действие с параметром с возможностью отмены.
    /// </summary>
    /// <param name="parameter">Параметр для действия.</param>
    /// <param name="funcDelegate">Действие для выполнения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <exception cref="Exception">При возникновении исключения во время выполнения действия.</exception>
    public void QueueExecute(object parameter, Action<object, CancellationToken> funcDelegate, CancellationToken cancellationToken);
}
