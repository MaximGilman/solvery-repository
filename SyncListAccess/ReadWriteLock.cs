namespace SyncListAccess;

public class ReadWriteLock
{
    private int _readerCount = 0;
    private int _writerCount = 0;
    private readonly object _lockObject;

    public ReadWriteLock(object lockObject)
    {
        _lockObject = lockObject;
    }

    /// <summary>
    /// Захватить поток на чтение.
    /// </summary>
    public void AcquireReaderLock()
    {
        lock (_lockObject)
        {
            while (_writerCount >0)
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
            if (_readerCount == 0)
            {
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
            while (_writerCount > 0 || _readerCount > 0)
            {
                Monitor.Exit(_lockObject);
            }

            _writerCount++;
        }
    }

    /// <summary>
    /// Отпустить поток на запись.
    /// </summary>
    public void ReleaseWriterLock()
    {
        lock (_lockObject)
        {
            _writerCount--;
            Monitor.PulseAll(_lockObject);
        }
    }
}
