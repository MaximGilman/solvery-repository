namespace TCP.Utils.Helpers.BlockSelectors;

public interface IBlockSelector<out TValue>
{
    public TValue SelectBlocksDataAsync();
}
