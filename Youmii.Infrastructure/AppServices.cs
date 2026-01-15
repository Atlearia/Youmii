using Youmii.Core.Interfaces;
using Youmii.Core.Models;
using Youmii.Core.Services;
using Youmii.Infrastructure.Brain;
using Youmii.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

namespace Youmii.Infrastructure;

/// <summary>
/// Simple factory for creating application services.
/// No DI container - just explicit construction.
/// </summary>
public sealed class AppServices : IDisposable
{
    private readonly SqliteDatabaseInitializer _dbInitializer;
    private readonly IBrainClient _brainClient;
    private bool _disposed;

    public AppSettings Settings { get; }
    public IBrainClient BrainClient => _brainClient;

    public AppServices(string? configPath = null)
    {
        Settings = LoadSettings(configPath);
        _dbInitializer = new SqliteDatabaseInitializer(Settings.DbPath);
        _brainClient = CreateBrainClient();
    }

    private static AppSettings LoadSettings(string? configPath)
    {
        var builder = new ConfigurationBuilder();

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
                builder.AddJsonFile(path!, optional: true, reloadOnChange: false);
                break;
            }
        }

        var config = builder.Build();
        var settings = new AppSettings();
        config.Bind(settings);

        // Set default DB path if not specified
        if (string.IsNullOrEmpty(settings.DbPath))
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var youmiiPath = Path.Combine(appDataPath, "Youmii");
            Directory.CreateDirectory(youmiiPath);
            settings.DbPath = Path.Combine(youmiiPath, "youmii.db");
        }

        return settings;
    }

    private IBrainClient CreateBrainClient()
    {
        // Simplified: Auto or Ollama both use SmartBrainClient (with fallback)
        // Stub goes directly to StubBrainClient
        if (Settings.BrainClientType.Equals(BrainClientTypes.Stub, StringComparison.OrdinalIgnoreCase))
        {
            return new StubBrainClient();
        }

        // Default: Auto-detect Ollama, fallback to Stub
        return new SmartBrainClient(Settings.OllamaUrl, Settings.OllamaModel);
    }

    /// <summary>
    /// Initialize the database. Call once at startup.
    /// </summary>
    public Task InitializeAsync() => _dbInitializer.InitializeAsync();

    /// <summary>
    /// Creates a new ConversationService instance.
    /// </summary>
    public ConversationService CreateConversationService()
    {
        var messageRepo = new SqliteMessageRepository(_dbInitializer);
        var factRepo = new SqliteFactRepository(_dbInitializer);
        var factExtractor = new SimpleFactExtractor();

        return new ConversationService(
            messageRepo,
            factRepo,
            factExtractor,
            Settings.MaxHistoryMessages);
    }

    public void Dispose()
    {
        if (_disposed) return;

        (_brainClient as IDisposable)?.Dispose();
        _dbInitializer.Dispose();

        _disposed = true;
    }
}
