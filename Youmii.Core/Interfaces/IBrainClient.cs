using Youmii.Core.Models;

namespace Youmii.Core.Interfaces;

/// <summary>
/// Client interface for communicating with the AI brain.
/// </summary>
public interface IBrainClient
{
    /// <summary>
    /// Gets the display name of this brain client (e.g., "Ollama", "Stub", "HTTP").
    /// </summary>
    string ClientName { get; }

    /// <summary>
    /// Sends a message to the brain and returns the response.
    /// </summary>
    /// <param name="request">The request containing message, history, and facts.</param>
    /// <returns>The brain's response.</returns>
    Task<BrainResponse> SendMessageAsync(BrainRequest request);
}
