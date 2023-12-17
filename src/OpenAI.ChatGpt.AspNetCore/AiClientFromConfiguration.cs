using Microsoft.Extensions.Configuration;

namespace OpenAI.ChatGpt.AspNetCore;

#pragma warning disable CS0618 // Type or member is obsolete
internal class AiClientFromConfiguration : IAiClient, IOpenAiClient
#pragma warning restore CS0618 // Type or member is obsolete
{
    private const string OpenAiProvider = "openai";
    private const string AzureOpenAiProvider = "azure_openai";
    private const string OpenRouterProvider = "openrouter";
    
    private static readonly string[] Providers =
    {
        OpenAiProvider, AzureOpenAiProvider, OpenRouterProvider
    };
    private readonly IAiClient _client;

    public AiClientFromConfiguration(
        AiClientFactory clientFactory, 
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(clientFactory);
        ArgumentNullException.ThrowIfNull(configuration);
        var provider = configuration.GetValue<string>("AIProvider")?.ToLower();
        provider ??= OpenAiProvider;
        if (!Providers.Contains(provider))
        {
            ThrowUnkownProviderException(provider);
        }
        _client = provider switch
        {
            OpenAiProvider => clientFactory.GetOpenAiClient(),
            AzureOpenAiProvider => clientFactory.GetAzureOpenAiClient(),
            OpenRouterProvider => clientFactory.GetOpenRouterClient(),
            _ => throw new InvalidOperationException($"Unknown provider: {provider}")
        };
    }


    private static void ThrowUnkownProviderException(string provider)
    {
        throw new ArgumentException($"Unknown AI provider: {provider}. " +
                                    $"Supported providers: {string.Join(", ", Providers)}");
    }

    /// <inheritdoc />
    public Task<string> GetChatCompletions(
        UserOrSystemMessage dialog,
        int? maxTokens = null,
        string model = ChatCompletionModels.Default, float temperature = ChatCompletionTemperatures.Default,
        string? user = null, bool jsonMode = false, long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null, CancellationToken cancellationToken = default)
    {
        return _client.GetChatCompletions(dialog, maxTokens, model, temperature, user, jsonMode, seed,
            requestModifier, rawResponseGetter, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> GetChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int? maxTokens = null,
        string model = ChatCompletionModels.Default, float temperature = ChatCompletionTemperatures.Default,
        string? user = null, bool jsonMode = false, long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null, CancellationToken cancellationToken = default)
    {
        return _client.GetChatCompletions(messages, maxTokens, model, temperature, user, jsonMode, seed,
            requestModifier, rawResponseGetter, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ChatCompletionResponse> GetChatCompletionsRaw(
        IEnumerable<ChatCompletionMessage> messages,
        int? maxTokens = null,
        string model = ChatCompletionModels.Default, float temperature = ChatCompletionTemperatures.Default,
        string? user = null, bool jsonMode = false, long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default)
    {
        return _client.GetChatCompletionsRaw(messages, maxTokens, model, temperature, user, jsonMode, seed,
            requestModifier, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<string> StreamChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int? maxTokens = null,
        string model = ChatCompletionModels.Default, float temperature = ChatCompletionTemperatures.Default,
        string? user = null, bool jsonMode = false, long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default)
    {
        return _client.StreamChatCompletions(messages, maxTokens, model, temperature, user, jsonMode, seed,
            requestModifier, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<string> StreamChatCompletions(
        UserOrSystemMessage messages,
        int? maxTokens = null, string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default, string? user = null, bool jsonMode = false,
        long? seed = null, Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default)
    {
        return _client.StreamChatCompletions(
            messages, maxTokens, model, temperature, user, jsonMode, seed, requestModifier, cancellationToken);
    }

    /// <inheritdoc />
    public int? GetDefaultMaxTokens(string model)
    {
        return _client.GetDefaultMaxTokens(model);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<string> StreamChatCompletions(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        return _client.StreamChatCompletions(request, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ChatCompletionResponse> StreamChatCompletionsRaw(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        return _client.StreamChatCompletionsRaw(request, cancellationToken);
    }

    public string GetOptimalModel(ChatCompletionMessage[] messages)
    {
        return _client.GetOptimalModel(messages);
    }

    internal IAiClient GetInnerClient() => _client;
}