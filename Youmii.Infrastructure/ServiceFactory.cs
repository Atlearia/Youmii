using Youmii.Core.DependencyInjection;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;
using Youmii.Infrastructure.DependencyInjection;

namespace Youmii.Infrastructure;

/// <summary>
/// Factory for creating infrastructure services.
/// Acts as a facade over the ServiceContainer for simpler usage.
/// </summary>
[Obsolete("Use ServiceContainer directly for better DI support. This class is kept for backward compatibility.")]
public sealed class ServiceFactory : IDisposable
{
    private readonly ServiceContainer _container;

    public IConfigService ConfigService => _container.Resolve<IConfigService>();
    public AppSettings Settings => _container.Settings;

    public ServiceFactory(string? configPath = null)
    {
        _container = new ServiceContainer(configPath);
    }

    /// <summary>
    /// Initializes the database. Call this before using repositories.
    /// </summary>
    public Task InitializeAsync() => _container.InitializeAsync();

    /// <summary>
    /// Creates a message repository.
    /// </summary>
    public IMessageRepository CreateMessageRepository() => _container.Resolve<IMessageRepository>();

    /// <summary>
    /// Creates a fact repository.
    /// </summary>
    public IFactRepository CreateFactRepository() => _container.Resolve<IFactRepository>();

    /// <summary>
    /// Creates a fact extractor.
    /// </summary>
    public IFactExtractor CreateFactExtractor() => _container.Resolve<IFactExtractor>();

    /// <summary>
    /// Creates a brain client based on configuration.
    /// </summary>
    public IBrainClient CreateBrainClient() => _container.Resolve<IBrainClient>();

    /// <summary>
    /// Creates a conversation service with all dependencies.
    /// </summary>
    public IConversationService CreateConversationService() => _container.Resolve<IConversationService>();

    public void Dispose() => _container.Dispose();
}
