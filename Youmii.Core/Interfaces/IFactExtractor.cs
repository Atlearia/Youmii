using Youmii.Core.Models;

namespace Youmii.Core.Interfaces;

/// <summary>
/// Service for extracting facts from user messages.
/// </summary>
public interface IFactExtractor
{
    /// <summary>
    /// Extracts facts from a user message.
    /// </summary>
    /// <param name="userMessage">The user's message text.</param>
    /// <returns>Dictionary of extracted facts (key-value pairs).</returns>
    IReadOnlyDictionary<string, string> ExtractFacts(string userMessage);
}
