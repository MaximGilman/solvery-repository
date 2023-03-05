namespace UDPCopiesCount.Settings;

public class Settings
{
    public int Port { get; set; }

    public void Guard()
    {
        Utils.Guards.Guard.IsNotDefault(Port);
        Utils.Guards.Guard.IsLessOrEqual(Port, 9999);
        Utils.Guards.Guard.IsGreater(Port, 0);
    }
}
