using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Infrastructure.Brain;

/// <summary>
/// Smart brain client that auto-detects available AI backends.
/// Tries Ollama first, falls back to Stub if unavailable.
/// </summary>
public sealed class SmartBrainClient : IBrainClient, IDisposable
{
    private readonly OllamaBrainClient _ollamaClient;
    private readonly StubBrainClient _stubClient;
    private readonly string _ollamaModel;
    
    private bool _useOllama;
    private bool _initialized;
    private readonly object _initLock = new();

    /// <inheritdoc />
    public string ClientName => _initialized 
        ? (_useOllama ? $"Ollama ({_ollamaModel})" : "Stub (Offline)") 
        : "Detecting...";

    public SmartBrainClient(string ollamaUrl = "http://localhost:11434", string ollamaModel = "llama3.2")
    {
        _ollamaModel = ollamaModel;
        _ollamaClient = new OllamaBrainClient(ollamaUrl, ollamaModel);
        _stubClient = new StubBrainClient();
    }

    /// <summary>
    /// Initializes the client by detecting available backends.
    /// Call this once at startup for immediate detection, or let it auto-detect on first message.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;

        lock (_initLock)
        {
            if (_initialized) return;
        }

        _useOllama = await _ollamaClient.IsAvailableAsync();
        _initialized = true;
    }

    /// <summary>
    /// Gets whether Ollama is being used (after initialization).
    /// </summary>
    public bool IsUsingOllama => _initialized && _useOllama;

    /// <summary>
    /// Gets whether the client has been initialized.
    /// </summary>
    public bool IsInitialized => _initialized;

    public async Task<BrainResponse> SendMessageAsync(BrainRequest request)
    {
        // Auto-initialize on first message if not already done
        if (!_initialized)
        {
            await InitializeAsync();
        }

        if (_useOllama)
        {
            var response = await _ollamaClient.SendMessageAsync(request);
            
            // If Ollama fails with connection error, switch to stub
            if (response.Reply.Contains("Cannot connect to Ollama") || 
                response.Reply.Contains("Ollama error"))
            {
                // Ollama went down, fall back to stub for this request
                var stubResponse = await _stubClient.SendMessageAsync(request);
                stubResponse.Reply += " (Ollama unavailable)";
                return stubResponse;
            }
            
            return response;
        }

        return await _stubClient.SendMessageAsync(request);
    }

    /// <summary>
    /// Re-checks Ollama availability. Call this if user starts Ollama after app launch.
    /// </summary>
    public async Task RefreshAvailabilityAsync()
    {
        _useOllama = await _ollamaClient.IsAvailableAsync();
        _initialized = true;
    }

    public void Dispose()
    {
        _ollamaClient.Dispose();
    }
}
