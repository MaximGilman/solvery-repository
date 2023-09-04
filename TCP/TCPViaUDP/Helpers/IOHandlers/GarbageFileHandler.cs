using TCPViaUDP.Helpers.BlockSelectors;

namespace TCPViaUDP.Helpers.IOHandlers;

/// <summary>
/// Имитирует сохранение файла на диск. Просто выбрасывает блок данных.
/// </summary>
public class GarbageFileHandler : IFileHandler
{
    // TODO: сделать реальную загрузку на диск.
    private readonly IBlockSelector<IEnumerable<Memory<byte>>> _blockSelector;

    public GarbageFileHandler(IBlockSelector<IEnumerable<Memory<byte>>> blockSelector)
    {
        _blockSelector = blockSelector;
    }

    public Task HandleAsync()
    {
        var blocks = _blockSelector.SelectBlocksDataAsync();
        // Do nothing...
        return Task.CompletedTask;
    }
}
