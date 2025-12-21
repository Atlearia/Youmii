namespace Youmii.Core.Models;

/// <summary>
/// Represents a stored fact about the user.
/// </summary>
public sealed class Fact
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Well-known fact keys.
/// </summary>
public static class FactKeys
{
    public const string Name = "name";
    public const string Nickname = "nickname";
    public const string FavoriteColor = "favorite_color";
}
