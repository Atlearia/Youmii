using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Infrastructure.Brain;

/// <summary>
/// Stub brain client for offline testing. Returns mock responses.
/// </summary>
public sealed class StubBrainClient : IBrainClient
{
    private static readonly string[] GenericResponses =
    [
        "That's interesting! Tell me more.",
        "I understand. What else would you like to talk about?",
        "Hmm, let me think about that...",
        "I'm here to help! What's on your mind?",
        "That's a great point!",
        "I see what you mean.",
        "Thanks for sharing that with me!",
    ];

    private static readonly Random _random = new();

    public Task<BrainResponse> SendMessageAsync(BrainRequest request)
    {
        string reply;

        // Personalized response if we know the user's name
        if (request.Facts.TryGetValue(FactKeys.Name, out var name))
        {
            reply = GeneratePersonalizedResponse(name, request.Message);
        }
        else
        {
            reply = GenerateGenericResponse(request.Message);
        }

        return Task.FromResult(new BrainResponse { Reply = reply });
    }

    private static string GeneratePersonalizedResponse(string name, string message)
    {
        var lowerMessage = message.ToLowerInvariant();

        if (lowerMessage.Contains("hello") || lowerMessage.Contains("hi"))
        {
            return $"Hello, {name}! It's great to see you again!";
        }

        if (lowerMessage.Contains("how are you"))
        {
            return $"I'm doing well, {name}! Thanks for asking. How about you?";
        }

        if (lowerMessage.Contains("bye") || lowerMessage.Contains("goodbye"))
        {
            return $"Goodbye, {name}! Talk to you soon!";
        }

        // Random personalized response
        var responses = new[]
        {
            $"That's really interesting, {name}!",
            $"Thanks for sharing, {name}!",
            $"I appreciate you telling me that, {name}.",
            $"Got it, {name}! Anything else?",
        };

        return responses[_random.Next(responses.Length)];
    }

    private static string GenerateGenericResponse(string message)
    {
        var lowerMessage = message.ToLowerInvariant();

        if (lowerMessage.Contains("hello") || lowerMessage.Contains("hi"))
        {
            return "Hello! Nice to meet you! What's your name?";
        }

        if (lowerMessage.Contains("how are you"))
        {
            return "I'm doing great, thank you for asking! How about you?";
        }

        if (lowerMessage.Contains("bye") || lowerMessage.Contains("goodbye"))
        {
            return "Goodbye! Come back anytime!";
        }

        return GenericResponses[_random.Next(GenericResponses.Length)];
    }
}
