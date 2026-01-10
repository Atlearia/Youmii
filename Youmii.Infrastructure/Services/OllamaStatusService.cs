using Youmii.Infrastructure.Brain;

namespace Youmii.Infrastructure.Services;

/// <summary>
/// Service for checking Ollama availability and listing models.
/// Used by the UI to show Ollama status.
/// </summary>
public sealed class OllamaStatusService
{
    private readonly string _baseUrl;

    public OllamaStatusService(string baseUrl = "http://localhost:11434")
    {
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// Checks if Ollama is running and accessible.
    /// </summary>
    public async Task<OllamaStatus> CheckStatusAsync()
    {
        using var client = new OllamaBrainClient(_baseUrl);
        
        var isAvailable = await client.IsAvailableAsync();
        if (!isAvailable)
        {
            return new OllamaStatus
            {
                IsRunning = false,
                Message = "Ollama is not running. Start it with 'ollama serve' or install from ollama.ai"
            };
        }

        var models = await client.GetAvailableModelsAsync();
        return new OllamaStatus
        {
            IsRunning = true,
            AvailableModels = models,
            Message = models.Count > 0 
                ? $"Ollama running with {models.Count} model(s) available"
                : "Ollama running but no models installed. Run 'ollama pull llama3.2' to install a model."
        };
    }
}

/// <summary>
/// Represents the status of the Ollama service.
/// </summary>
public sealed class OllamaStatus
{
    public bool IsRunning { get; init; }
    public List<string> AvailableModels { get; init; } = [];
    public string Message { get; init; } = string.Empty;
}
