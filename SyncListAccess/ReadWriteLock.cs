using System.Net;

namespace SyncListAccess;

public sealed class ReadWriteLock
{
    private int _readerCount = 0;
    private bool _isWriterInLock = false;

    /// <summary>
    /// Флаг, ждет ли писатель, при работающих читателях.
    /// </summary>
    private bool _isWriterWaiting = false;

    /// <summary>
    /// Флаг, ждут ли читатели, при работающем писателе.
    /// </summary>
    private bool _isAnyReaderWaiting = false;

    /// <summary>
    /// Кто пропустит (читатель или писатель) в случае, если оба ждут.
    /// </summary>
    private ReadWriteLockActor _currentExpectant = ReadWriteLockActor.Reader;

    private readonly object _lockObject = new();

    /// <summary>
    /// Захватить поток на чтение.
    /// </summary>
    public void AcquireReaderLock()
    {
        lock (_lockObject)
        {
            _isAnyReaderWaiting = true;
            while (_isWriterInLock || !CanProceed(ReadWriteLockActor.Reader))
            {
                Monitor.Wait(_lockObject);
            }

            _readerCount++;
        }
    }

    /// <summary>
    /// Отпустить поток на чтение.
    /// </summary>
    public void ReleaseReaderLock()
    {
        lock (_lockObject)
        {
            _readerCount--;
            if (_readerCount <= 0)
            {
                _isAnyReaderWaiting = false;
                Monitor.PulseAll(_lockObject);
            }
        }
    }

    /// <summary>
    /// Захватить поток на запись.
    /// </summary>
    public void AcquireWriterLock()
    {
        lock (_lockObject)
        {
            _isWriterWaiting = true;
            while (_isWriterInLock || _readerCount > 0 || !CanProceed(ReadWriteLockActor.Writer))
            {
                Monitor.Wait(_lockObject);
            }

            _isWriterInLock = true;
        }
    }

    /// <summary>
    /// Отпустить поток на запись.
    /// </summary>
    public void ReleaseWriterLock()
    {
        lock (_lockObject)
        {
            _isWriterInLock = false;
            _isWriterWaiting = false;
            Monitor.PulseAll(_lockObject);
        }
    }

    /// <summary>
    /// Проверить, может ли операция войти в крит. секцию с т.з. честности.
    /// </summary>
    /// <param name="currentActor">Операция. Чтение или запись.</param>
    private bool CanProceed(ReadWriteLockActor currentActor)
    {
        switch (_isWriterWaiting, _isWriterWaiting)
        {
            // если ждут оба, смотрим на "пропускающего".
            // пропуская вперед, операция гарантирует себе вход следующей.
            case (true, true):
            {
                bool canCurrentProceed = currentActor != _currentExpectant;
                _currentExpectant = currentActor;
                return canCurrentProceed;
            }
            case (false, false):
            {
                throw new ApplicationException("Был вызван запрос на вход в критическую секцию, но ни один поток не ожидает входа.");
            }
            default:
            {
                return true;
            }
        }
    }
}
