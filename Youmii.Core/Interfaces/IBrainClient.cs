using Youmii.Core.Models;

namespace Youmii.Core.Interfaces;

/// <summary>
/// Client interface for communicating with the AI brain.
/// </summary>
public interface IBrainClient
{
    /// <summary>
    /// Sends a message to the brain and returns the response.
    /// </summary>
    /// <param name="request">The request containing message, history, and facts.</param>
    /// <returns>The brain's response.</returns>
    Task<BrainResponse> SendMessageAsync(BrainRequest request);
}
