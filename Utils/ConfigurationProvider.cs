using Microsoft.Extensions.Configuration;
using Utils.Constants;

namespace Utils;

/// <summary>
/// Поставщик информации о конфигурации.
/// </summary>
public static class ConfigurationProvider
{
    /// <summary>
    /// Конфигурация.
    /// </summary>
    private static readonly IConfiguration _configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile(FileNameConstants.APP_SETTINGS_JSON, optional: true).Build();

    /// <summary>
    /// Получить значение из секции конфигурации по имени.
    /// </summary>
    /// <param name="sectionName">Имя секции.</param>
    /// <typeparam name="T">Возвращаемый тип значения.</typeparam>
    public static T GetValue<T>(string sectionName) =>
        _configuration.GetValue<T>(sectionName);
}