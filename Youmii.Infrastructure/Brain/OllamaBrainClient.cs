using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Infrastructure.Brain;

/// <summary>
/// Brain client that uses Ollama for local LLM inference.
/// Ollama must be running locally (default: http://localhost:11434).
/// </summary>
public sealed class OllamaBrainClient : IBrainClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly string _systemPrompt;

    public OllamaBrainClient(string baseUrl = "http://localhost:11434", string model = "llama3.2")
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _model = model;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5) // LLMs can be slow
        };

        _systemPrompt = BuildSystemPrompt();
    }

    /// <summary>
    /// Checks if Ollama is running and accessible.
    /// </summary>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets list of available models from Ollama.
    /// </summary>
    public async Task<List<string>> GetAvailableModelsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaTagsResponse>(json);
            return result?.Models?.Select(m => m.Name).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<BrainResponse> SendMessageAsync(BrainRequest request)
    {
        try
        {
            var messages = BuildMessages(request);

            var ollamaRequest = new OllamaChatRequest
            {
                Model = _model,
                Messages = messages,
                Stream = false,
                Options = new OllamaOptions
                {
                    Temperature = 0.7,
                    TopP = 0.9,
                    NumPredict = 512
                }
            };

            var json = JsonSerializer.Serialize(ollamaRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/chat", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new BrainResponse
                {
                    Reply = $"Ollama error: {response.StatusCode}. Make sure Ollama is running and the model '{_model}' is installed."
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var ollamaResponse = JsonSerializer.Deserialize<OllamaChatResponse>(responseJson);

            return new BrainResponse
            {
                Reply = ollamaResponse?.Message?.Content?.Trim() ?? "I couldn't generate a response."
            };
        }
        catch (HttpRequestException)
        {
            return new BrainResponse
            {
                Reply = "Cannot connect to Ollama. Please make sure Ollama is running (ollama serve) and try again."
            };
        }
        catch (TaskCanceledException)
        {
            return new BrainResponse
            {
                Reply = "The request timed out. The model might be loading or processing a complex request."
            };
        }
        catch (Exception ex)
        {
            return new BrainResponse
            {
                Reply = $"Error communicating with Ollama: {ex.Message}"
            };
        }
    }

    private List<OllamaMessage> BuildMessages(BrainRequest request)
    {
        var messages = new List<OllamaMessage>();

        // System prompt with user facts
        var systemPrompt = _systemPrompt;
        if (request.Facts.Count > 0)
        {
            var factsText = string.Join("\n", request.Facts.Select(f => $"- {f.Key}: {f.Value}"));
            systemPrompt += $"\n\nHere's what you know about the user:\n{factsText}";
        }

        messages.Add(new OllamaMessage { Role = "system", Content = systemPrompt });

        // Add conversation history
        foreach (var historyItem in request.History.TakeLast(10)) // Limit history for context window
        {
            messages.Add(new OllamaMessage
            {
                Role = historyItem.Role == "user" ? "user" : "assistant",
                Content = historyItem.Content
            });
        }

        // Add current message
        messages.Add(new OllamaMessage { Role = "user", Content = request.Message });

        return messages;
    }

    private static string BuildSystemPrompt()
    {
        return """
            You are Youmii, a friendly and helpful desktop companion. You live on the user's screen as a cute overlay character.
            
            Personality traits:
            - Warm, supportive, and encouraging
            - Playful but not annoying
            - Genuinely interested in the user's wellbeing
            - Can be a bit quirky and have fun with conversations
            - Remember details about the user and reference them naturally
            
            Guidelines:
            - Keep responses concise (1-3 sentences usually) since you appear in a speech bubble
            - Use casual, friendly language
            - Occasionally use cute expressions or emoticons sparingly
            - If the user seems stressed, be supportive
            - You can play games with the user (chess, tic-tac-toe, memory match, snake)
            - Be helpful but also fun to talk to
            
            Remember: You're a companion, not just an assistant. Build a relationship with the user!
            """;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

#region Ollama API Models

internal sealed class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OllamaMessage> Messages { get; set; } = [];

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("options")]
    public OllamaOptions? Options { get; set; }
}

internal sealed class OllamaMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

internal sealed class OllamaOptions
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;

    [JsonPropertyName("top_p")]
    public double TopP { get; set; } = 0.9;

    [JsonPropertyName("num_predict")]
    public int NumPredict { get; set; } = 512;
}

internal sealed class OllamaChatResponse
{
    [JsonPropertyName("message")]
    public OllamaMessage? Message { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}

internal sealed class OllamaTagsResponse
{
    [JsonPropertyName("models")]
    public List<OllamaModelInfo>? Models { get; set; }
}

internal sealed class OllamaModelInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }
}

#endregion
