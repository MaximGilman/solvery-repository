namespace UDPCopiesCount.Settings;

public class Settings
{
    public int Port { get; set; }
    public string GroupIp { get; set; }
    public bool IsWatcher { get; set; }
    public string LogsPath { get; set; }

    private const string IP_REGEX = @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$";

    public void Guard()
    {
        Utils.Guards.Guard.IsNotDefault(Port);
        Utils.Guards.Guard.IsLessOrEqual(Port, 9999);
        Utils.Guards.Guard.IsGreater(Port, 0);

        Utils.Guards.Guard.IsNotNullOrWhiteSpace(GroupIp);
        Utils.Guards.Guard.IsMatch(GroupIp, IP_REGEX);

        try
        {
            Utils.Guards.Guard.IsNotNullOrWhiteSpace(LogsPath);
        }
        catch
        {
            Directory.CreateDirectory(LogsPath);
        }
    }
}
