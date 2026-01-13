namespace Youmii.Core.Models;

/// <summary>
/// Application configuration settings.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Type of brain client: "Auto", "Stub", "Http", or "Ollama"
    /// "Auto" will detect Ollama and fall back to Stub if unavailable.
    /// </summary>
    public string BrainClientType { get; set; } = "Auto";
    
    /// <summary>
    /// URL for the HTTP brain server (when using "Http" type).
    /// </summary>
    public string BrainServerUrl { get; set; } = "http://localhost:5180";
    
    /// <summary>
    /// URL for Ollama API (when using "Ollama" type).
    /// </summary>
    public string OllamaUrl { get; set; } = "http://localhost:11434";
    
    /// <summary>
    /// Model name to use with Ollama (e.g., "llama3.2", "mistral", "phi3").
    /// </summary>
    public string OllamaModel { get; set; } = "llama3.2";
    
    /// <summary>
    /// Maximum number of history messages to include in requests.
    /// </summary>
    public int MaxHistoryMessages { get; set; } = 20;
    
    /// <summary>
    /// Path to the SQLite database. Empty uses default location.
    /// </summary>
    public string DbPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Seconds before the speech bubble auto-hides.
    /// </summary>
    public int BubbleAutoHideSeconds { get; set; } = 8;
}

/// <summary>
/// Available brain client types.
/// </summary>
public static class BrainClientTypes
{
    /// <summary>Auto-detect: tries Ollama first, falls back to Stub.</summary>
    public const string Auto = "Auto";
    public const string Stub = "Stub";
    public const string Http = "Http";
    public const string Ollama = "Ollama";
}
