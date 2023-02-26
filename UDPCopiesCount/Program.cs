using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;
using UDPCopiesCount.Nodes;
using Utils;
using Utils.Constants;

namespace UDPCopiesCount;

internal class Program
{
    public static async Task Main()
    {
        var node = ConfigureAndCreateNode();
        await node.DoSomeWork(CancellationToken.None);
    }

    private static INode ConfigureAndCreateNode()
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
        ReconfigureNlogPath(settings.LogsPath, instanceId);

        return settings.IsWatcher switch
        {
            true => new WatcherNode(instanceId, settings.GroupIp, settings.Port, loggerFactory.CreateLogger<WatcherNode>()),
            false => new Node(instanceId, settings.GroupIp, settings.Port, loggerFactory.CreateLogger<Node>())
        };
    }

    /// <summary>
    /// Переконфигурировать логгер на GUID инстанса.
    /// </summary>
    /// <param name="logsPath">Путь до папки с логами.</param>
    /// <param name="instanceGuid">ИД узла.</param>
    private static void ReconfigureNlogPath(string logsPath, Guid instanceGuid)
    {
        var target = (FileTarget)LogManager.Configuration.FindTargetByName("logfile");
        target.FileName = $"{logsPath}/{instanceGuid}-log.txt";
        LogManager.ReconfigExistingLoggers();
    }
}
