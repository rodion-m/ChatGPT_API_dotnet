using Microsoft.Extensions.Options;
using OpenAI.ChatGpt.AspNetCore.Models;

namespace OpenAI.ChatGpt.AspNetCore;

/// <summary>
///  Factory for creating <see cref="ChatGPT" /> instances from DI.
/// </summary>
/// <example>
/// builder.Services.AddHttpClient&lt;ChatGPTFactory&lt;(client =>
///     {
///         client.DefaultRequestHeaders.Authorization = builder.Configuration["ChatGPTCredentials:ApiKey"];
///     })
///     .AddPolicyHandler(GetRetryPolicy())
///     .AddPolicyHandler(GetCircuitBreakerPolicy());
/// </example>
// ReSharper disable once InconsistentNaming
public class ChatGPTFactory : IDisposable
{
    private readonly OpenAiClient _client;
    private readonly ChatCompletionsConfig _config;
    private readonly IChatHistoryStorage _chatHistoryStorage;
    private readonly ITimeProvider _clock;
    private bool _ensureStorageCreatedCalled;

    public ChatGPTFactory(
        IHttpClientFactory httpClientFactory,
        IOptions<ChatGptCredentials> credentials,
        IOptions<ChatCompletionsConfig> config,
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock)
    {
        if (httpClientFactory == null) throw new ArgumentNullException(nameof(httpClientFactory));
        if (credentials?.Value == null) throw new ArgumentNullException(nameof(credentials));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _chatHistoryStorage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _client = CreateOpenAiClient(httpClientFactory, credentials);
    }

    public ChatGPTFactory(
        IOptions<ChatGptCredentials> credentials,
        IOptions<ChatCompletionsConfig> config,
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock)
    {
        if (credentials?.Value == null) throw new ArgumentNullException(nameof(credentials));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _chatHistoryStorage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _client = new OpenAiClient(credentials.Value.ApiKey);
    }

    public ChatGPTFactory(
        string apiKey,
        IChatHistoryStorage chatHistoryStorage, 
        ITimeProvider? clock = null, 
        ChatCompletionsConfig? config = null)
    {
        if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
        _client = new OpenAiClient(apiKey);
        _config = config ?? ChatCompletionsConfig.Default;
        _chatHistoryStorage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? new TimeProviderUtc();
    }
    
    private OpenAiClient CreateOpenAiClient(
        IHttpClientFactory httpClientFactory, 
        IOptions<ChatGptCredentials> credentials)
    {
        var httpClient = httpClientFactory.CreateClient(nameof(ChatGPTFactory));
        httpClient.DefaultRequestHeaders.Authorization = credentials.Value.GetAuthHeader();
        httpClient.BaseAddress = new Uri(credentials.Value.ApiHost);
        return new OpenAiClient(httpClient);
    }

    public static ChatGPTFactory CreateInMemory(string apiKey, ChatCompletionsConfig? config = null)
    {
        if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
        return new ChatGPTFactory(apiKey, new InMemoryChatHistoryStorage(), new TimeProviderUtc(), config);
    }

    public async Task<ChatGPT> Create(
        string userId, 
        ChatCompletionsConfig? config = null, 
        bool ensureStorageCreated = true,
        CancellationToken cancellationToken = default)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (ensureStorageCreated && !_ensureStorageCreatedCalled)
        {
            await _chatHistoryStorage.EnsureStorageCreated(cancellationToken);
            _ensureStorageCreatedCalled = true;
        }
        return new ChatGPT(
            _client,
            _chatHistoryStorage,
            _clock,
            userId,
            ChatCompletionsConfig.Combine(_config, config)
        );
    }

    public async Task<ChatGPT> Create(
        ChatCompletionsConfig? config = null,
        bool ensureStorageCreated = true,
        CancellationToken cancellationToken = default)
    {
        if (ensureStorageCreated)
        {
            await _chatHistoryStorage.EnsureStorageCreated(cancellationToken);
        }
        return new ChatGPT(
            _client,
            _chatHistoryStorage,
            _clock,
            ChatCompletionsConfig.Combine(_config, config)
        );
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}