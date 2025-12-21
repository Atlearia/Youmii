using System.Text.RegularExpressions;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Core.Services;

/// <summary>
/// Simple deterministic fact extractor using regex patterns.
/// </summary>
public sealed partial class SimpleFactExtractor : IFactExtractor
{
    // Pattern: "my name is X" or "I'm X" or "I am X" (where X is a name)
    [GeneratedRegex(@"(?:my name is|i'?m|i am)\s+([a-z]+)", RegexOptions.IgnoreCase)]
    private static partial Regex NamePattern();

    // Pattern: "call me X"
    [GeneratedRegex(@"call me\s+([a-z]+)", RegexOptions.IgnoreCase)]
    private static partial Regex NicknamePattern();

    // Pattern: "my favorite color is X" or "I like X color"
    [GeneratedRegex(@"(?:my )?favo(?:u)?rite colou?r is\s+([a-z]+)", RegexOptions.IgnoreCase)]
    private static partial Regex FavoriteColorPattern();

    public IReadOnlyDictionary<string, string> ExtractFacts(string userMessage)
    {
        var facts = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(userMessage))
            return facts;

        // Extract name
        var nameMatch = NamePattern().Match(userMessage);
        if (nameMatch.Success)
        {
            var name = nameMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                facts[FactKeys.Name] = CapitalizeFirst(name);
            }
        }

        // Extract nickname
        var nicknameMatch = NicknamePattern().Match(userMessage);
        if (nicknameMatch.Success)
        {
            var nickname = nicknameMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(nickname))
            {
                facts[FactKeys.Nickname] = CapitalizeFirst(nickname);
            }
        }

        // Extract favorite color
        var colorMatch = FavoriteColorPattern().Match(userMessage);
        if (colorMatch.Success)
        {
            var color = colorMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(color))
            {
                facts[FactKeys.FavoriteColor] = color.ToLowerInvariant();
            }
        }

        return facts;
    }

    private static string CapitalizeFirst(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant();
    }
}
