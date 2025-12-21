namespace Youmii.Core.Models;

/// <summary>
/// Application configuration settings.
/// </summary>
public sealed class AppSettings
{
    public string BrainClientType { get; set; } = "Stub";
    public string BrainServerUrl { get; set; } = "http://localhost:5180";
    public int MaxHistoryMessages { get; set; } = 20;
    public string DbPath { get; set; } = string.Empty;
    public int BubbleAutoHideSeconds { get; set; } = 8;
}
