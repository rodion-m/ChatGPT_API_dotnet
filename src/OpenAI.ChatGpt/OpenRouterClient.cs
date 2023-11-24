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
    private const string DefaultHost = "https://openrouter.ai/api/v1/";
    
    public OpenRouterClient(string apiKey, string? host = DefaultHost) 
        : base(apiKey, host ?? DefaultHost)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        ChatCompletionModels.DisableModelNameValidation();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public OpenRouterClient(HttpClient httpClient) : base(httpClient)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        ChatCompletionModels.DisableModelNameValidation();
#pragma warning restore CS0618 // Type or member is obsolete
    }
}