using Utils;
using Utils.Guards;

namespace Philosophers;

internal class Philosopher
{
    #region Поля и свойства

    private int _id { get; }

    /// <summary>
    /// Правая вилка.
    /// </summary>
    private object _firstFork { get; }

    /// <summary>
    /// Левая вилка.
    /// </summary>
    private object _secondFork { get; }

    private readonly Random _rand = new();

    /// <summary>
    /// Период ожидания в м.сек.
    /// </summary>
    private const int TIME_SLEEP_PERIOD = 1000;

    #endregion

    #region Методы

    internal void Meal(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Wait("thinking");
            lock (_firstFork)
            {
                lock (_secondFork)
                {
                    Wait("eating");
                }
            }
        }
    }

    private void Wait(string waitingClause)
    {
        Guard.IsNotNullOrWhiteSpace(waitingClause);
        var sleepPeriod = _rand.Next(TIME_SLEEP_PERIOD);
        ConsoleWriter.WriteEvent($"Philosopher {_id} {waitingClause} for {sleepPeriod}");
        Thread.Sleep(sleepPeriod);
    }

    #endregion

    #region Конструктор

    internal Philosopher(IReadOnlyList<object> forks, int id)
    {
        Guard.IsNotDefault(forks);
        Guard.IsNotEmpty(forks);
        Guard.IsGreater(id, 0);
        Guard.IsLess(id, forks.Count);

        _firstFork = forks[id];
        _secondFork = forks[(id + 1) % forks.Count];

        // У последнего философа меняем вилки.
        if (id == forks.Count - 1)
        {
            _secondFork = forks[id];
            _firstFork = forks[(id + 1) % forks.Count];
        }


        _id = id;
    }

    #endregion
}
