using Microsoft.Extensions.Configuration;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Infrastructure.Configuration;

/// <summary>
/// Configuration service that reads from appsettings.json.
/// </summary>
public sealed class JsonConfigService : IConfigService
{
    public AppSettings Settings { get; }

    public JsonConfigService(string? configPath = null)
    {
        var builder = new ConfigurationBuilder();

        // Look for config file in current directory or app directory
        var paths = new[]
        {
            configPath,
            "appsettings.json",
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")
        };

        foreach (var path in paths.Where(p => !string.IsNullOrEmpty(p)))
        {
            if (File.Exists(path))
            {
                builder.AddJsonFile(path, optional: true, reloadOnChange: false);
                break;
            }
        }

        var config = builder.Build();

        Settings = new AppSettings();
        config.Bind(Settings);

        // Set default DB path if not specified
        if (string.IsNullOrEmpty(Settings.DbPath))
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var youmiiPath = Path.Combine(appDataPath, "Youmii");
            Directory.CreateDirectory(youmiiPath);
            Settings.DbPath = Path.Combine(youmiiPath, "youmii.db");
        }
    }
}
