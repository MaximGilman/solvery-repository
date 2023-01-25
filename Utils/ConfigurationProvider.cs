using Microsoft.Extensions.Configuration;

namespace Utils;

public static class ConfigurationProvider
{
    private static readonly IConfiguration _configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true).Build();

    public static T? GetValue<T>(string sectionName) => 
        _configuration.GetValue<T>(sectionName);
}