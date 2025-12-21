using System.Net.Http.Json;
using System.Text.Json;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Infrastructure.Brain;

/// <summary>
/// HTTP brain client that calls a local server API.
/// </summary>
public sealed class HttpBrainClient : IBrainClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly IBrainClient _fallbackClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public HttpBrainClient(HttpClient httpClient, string baseUrl, IBrainClient? fallbackClient = null)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _fallbackClient = fallbackClient ?? new StubBrainClient();
    }

    public async Task<BrainResponse> SendMessageAsync(BrainRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/chat",
                request,
                JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BrainResponse>(JsonOptions);
                return result ?? new BrainResponse { Reply = "No response received." };
            }

            // Server error - fallback
            var fallbackResponse = await _fallbackClient.SendMessageAsync(request);
            fallbackResponse.Reply += " (offline)";
            return fallbackResponse;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Network error - fallback to stub
            var fallbackResponse = await _fallbackClient.SendMessageAsync(request);
            fallbackResponse.Reply += " (offline)";
            return fallbackResponse;
        }
    }
}
