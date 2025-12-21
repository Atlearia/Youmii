using Youmii.Core.Interfaces;
using Youmii.Core.Models;
using Youmii.Core.Services;
using Youmii.Infrastructure.Brain;
using Youmii.Infrastructure.Configuration;
using Youmii.Infrastructure.Persistence;

namespace Youmii.Infrastructure;

/// <summary>
/// Factory for creating infrastructure services with proper dependencies.
/// </summary>
public sealed class ServiceFactory : IDisposable
{
    private readonly IConfigService _configService;
    private readonly DatabaseInitializer _databaseInitializer;
    private readonly HttpClient? _httpClient;
    private bool _initialized;

    public IConfigService ConfigService => _configService;
    public AppSettings Settings => _configService.Settings;

    public ServiceFactory(string? configPath = null)
    {
        _configService = new JsonConfigService(configPath);
        _databaseInitializer = new DatabaseInitializer(_configService.Settings.DbPath);
        
        if (_configService.Settings.BrainClientType.Equals("Http", StringComparison.OrdinalIgnoreCase))
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }
    }

    /// <summary>
    /// Initializes the database. Call this before using repositories.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;
        await _databaseInitializer.InitializeAsync();
        _initialized = true;
    }

    /// <summary>
    /// Creates a message repository.
    /// </summary>
    public IMessageRepository CreateMessageRepository()
    {
        return new SqliteMessageRepository(_databaseInitializer);
    }

    /// <summary>
    /// Creates a fact repository.
    /// </summary>
    public IFactRepository CreateFactRepository()
    {
        return new SqliteFactRepository(_databaseInitializer);
    }

    /// <summary>
    /// Creates a fact extractor.
    /// </summary>
    public IFactExtractor CreateFactExtractor()
    {
        return new SimpleFactExtractor();
    }

    /// <summary>
    /// Creates a brain client based on configuration.
    /// </summary>
    public IBrainClient CreateBrainClient()
    {
        if (_configService.Settings.BrainClientType.Equals("Http", StringComparison.OrdinalIgnoreCase))
        {
            return new HttpBrainClient(
                _httpClient!,
                _configService.Settings.BrainServerUrl,
                new StubBrainClient()); // Fallback
        }

        return new StubBrainClient();
    }

    /// <summary>
    /// Creates a conversation service with all dependencies.
    /// </summary>
    public ConversationService CreateConversationService()
    {
        return new ConversationService(
            CreateMessageRepository(),
            CreateFactRepository(),
            CreateFactExtractor(),
            _configService.Settings.MaxHistoryMessages);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _databaseInitializer.Dispose();
    }
}
