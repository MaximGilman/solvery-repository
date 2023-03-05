using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using UDPCopiesCount.Nodes;
using Utils.Constants;

namespace UDPCopiesCount;

internal class Program
{
    public static async Task Main()
    {
        var node = ConfigureAndCreateNode();
        var ctn = CancellationToken.None;
        await Task.WhenAll(
            Task.Run(async () => await node.StartReceiveStatusAsync(ctn), ctn),
            Task.Run(async () => await node.StartSendingStatusAsync(ctn), ctn),
            Task.Run(async () => await node.StartUpdateIsAlive(ctn), ctn));

    }

    private static WatchableNode ConfigureAndCreateNode()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddNLog());
        var programLogger = loggerFactory.CreateLogger<Program>();
        programLogger.LogInformation("Start program. Configure services");

        var config = new ConfigurationBuilder()
            .AddJsonFile(FileNameConstants.APP_SETTINGS_JSON, optional: false, reloadOnChange: true)
            .Build();

        var settings = config.Get<Settings.Settings>();

        try
        {
            settings.Guard();
        }
        catch (Exception e)
        {
            programLogger.LogError("Getting value from {appsettingsFile} cause error: {@error}",
                FileNameConstants.APP_SETTINGS_JSON, e);
        }

        var instanceId = Guid.NewGuid();
        return new WatchableNode(instanceId, settings.Port, loggerFactory.CreateLogger<WatchableNode>());
    }
}
