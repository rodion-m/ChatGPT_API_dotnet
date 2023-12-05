using OpenAI.ChatGpt.Models.ChatCompletion;

namespace OpenAI.ChatGpt;

/// <summary>
/// OpenRouter.ai client
/// </summary>
/// <remarks>
/// Docs: https://openrouter.ai/docs
/// </remarks>
public class OpenRouterClient : OpenAiClient
{
    internal new const string DefaultHost = "https://openrouter.ai/api/v1/";
    
    /// <summary>
    /// Creates a new instance of <see cref="OpenRouterClient"/>
    /// </summary>
    /// <param name="apiKey">OpenRouter API key. Can be issued here: https://openrouter.ai/keys</param>
    /// <param name="host">OpenRouter API host. Default is: <see cref="DefaultHost"/></param>
    public OpenRouterClient(string apiKey, string? host = DefaultHost) 
        : base(apiKey, host ?? DefaultHost)
    {
    }

    public OpenRouterClient(HttpClient httpClient) 
        : base(httpClient, validateAuthorizationHeader: true, validateBaseAddress: true)
    {
    }
}