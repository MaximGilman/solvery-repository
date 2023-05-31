using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Targets;
using UDPCopiesCount.Nodes;
using Utils.Constants;

namespace UDPCopiesCount;

internal static class Program
{
    public static async Task Main()
    {
        var node = ConfigureAndCreateNode();
        var cancellationToken = CancellationToken.None;
        await Task.WhenAll(
            Task.Run(async () => await node.StartReceiveStatusAsync(cancellationToken), cancellationToken),
            Task.Run(async () => await node.StartSendingStatusAsync(cancellationToken), cancellationToken),
            Task.Run(async () => await node.StartUpdateIsAlive(cancellationToken), cancellationToken));

    }

    private static WatchableNode ConfigureAndCreateNode()
    {
        var setup = LogManager.Setup().LoadConfiguration(builder =>
        {
            builder.ForLogger().WriteTo(new ColoredConsoleTarget("logConsole")
            {
                Layout = "${longdate}|${pad:padding=5:inner=${level:uppercase=true}}|${message}",
                UseDefaultRowHighlightingRules = false,
                RowHighlightingRules =
                {
                    new ConsoleRowHighlightingRule()
                    {
                        Condition = "level == LogLevel.Debug",
                        ForegroundColor = ConsoleOutputColor.DarkGray
                    },
                    new ConsoleRowHighlightingRule()
                    {
                        Condition = "level == LogLevel.Info",
                        ForegroundColor = ConsoleOutputColor.White
                    },
                    new ConsoleRowHighlightingRule()
                    {
                        Condition = "level == LogLevel.Error",
                        ForegroundColor = ConsoleOutputColor.Red
                    },
                }
            });
        });

        var programLogger = setup.LogFactory.GetCurrentClassLogger();
        programLogger.Info("Start program. Configure services");

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
            programLogger.Error("Getting value from {appsettingsFile} cause error: {@error}",
                FileNameConstants.APP_SETTINGS_JSON, e);
        }

        var instanceId = Guid.NewGuid();
        return new WatchableNode(instanceId, settings.Port, setup.LogFactory);
    }
}
