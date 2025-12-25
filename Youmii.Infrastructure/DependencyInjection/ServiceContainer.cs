using Youmii.Core.DependencyInjection;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;
using Youmii.Core.Services;
using Youmii.Infrastructure.Brain;
using Youmii.Infrastructure.Configuration;
using Youmii.Infrastructure.Persistence;

namespace Youmii.Infrastructure.DependencyInjection;

/// <summary>
/// Service container that manages all application dependencies.
/// Provides a simple composition root for the application.
/// </summary>
public sealed class ServiceContainer : IServiceLocator, IApplicationLifetime, IDisposable
{
    private readonly Dictionary<Type, Func<object>> _factories = new();
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly List<IDisposable> _disposables = new();
    private bool _initialized;
    private bool _disposed;

    public AppSettings Settings { get; }

    public ServiceContainer(string? configPath = null)
    {
        // Load configuration first
        var configService = new JsonConfigService(configPath);
        Settings = configService.Settings;
        
        RegisterServices(configService);
    }

    private void RegisterServices(IConfigService configService)
    {
        // Register configuration
        RegisterSingleton<IConfigService>(configService);
        RegisterSingleton(Settings);

        // Register database
        var dbInitializer = new SqliteDatabaseInitializer(Settings.DbPath);
        _disposables.Add(dbInitializer);
        RegisterSingleton<IDatabaseInitializer>(dbInitializer);
        RegisterSingleton<ISqliteConnectionFactory>(dbInitializer);

        // Register repositories
        RegisterFactory<IMessageRepository>(() => 
            new SqliteMessageRepository(Resolve<ISqliteConnectionFactory>()));
        RegisterFactory<IFactRepository>(() => 
            new SqliteFactRepository(Resolve<ISqliteConnectionFactory>()));

        // Register services
        RegisterSingleton<IFactExtractor>(new SimpleFactExtractor());
        RegisterFactory<IConversationService>(() => 
            new ConversationService(
                Resolve<IMessageRepository>(),
                Resolve<IFactRepository>(),
                Resolve<IFactExtractor>(),
                Settings.MaxHistoryMessages));

        // Register brain client
        RegisterBrainClient();
    }

    private void RegisterBrainClient()
    {
        if (Settings.BrainClientType.Equals("Http", StringComparison.OrdinalIgnoreCase))
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _disposables.Add(httpClient);

            var fallback = new StubBrainClient();
            var httpBrainClient = new HttpBrainClient(httpClient, Settings.BrainServerUrl, fallback);
            RegisterSingleton<IBrainClient>(httpBrainClient);
        }
        else
        {
            RegisterSingleton<IBrainClient>(new StubBrainClient());
        }
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        ThrowIfDisposed();
        
        if (_initialized) return;

        var dbInitializer = Resolve<IDatabaseInitializer>();
        await dbInitializer.InitializeAsync();

        _initialized = true;
    }

    /// <inheritdoc />
    public Task ShutdownAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public T Resolve<T>() where T : class
    {
        ThrowIfDisposed();
        
        var type = typeof(T);

        // Check singletons first
        if (_singletons.TryGetValue(type, out var singleton))
        {
            return (T)singleton;
        }

        // Then check factories
        if (_factories.TryGetValue(type, out var factory))
        {
            return (T)factory();
        }

        throw new InvalidOperationException($"Service of type {type.Name} is not registered.");
    }

    /// <inheritdoc />
    public T? ResolveOptional<T>() where T : class
    {
        ThrowIfDisposed();
        
        var type = typeof(T);

        if (_singletons.TryGetValue(type, out var singleton))
        {
            return (T)singleton;
        }

        if (_factories.TryGetValue(type, out var factory))
        {
            return (T)factory();
        }

        return null;
    }

    private void RegisterSingleton<T>(T instance) where T : class
    {
        _singletons[typeof(T)] = instance;
    }

    private void RegisterFactory<T>(Func<T> factory) where T : class
    {
        _factories[typeof(T)] = () => factory();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
        _disposables.Clear();
        _singletons.Clear();
        _factories.Clear();

        _disposed = true;
    }
}
