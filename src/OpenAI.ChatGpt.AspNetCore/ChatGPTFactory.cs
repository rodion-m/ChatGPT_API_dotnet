using Microsoft.Extensions.Options;
using OpenAI.ChatGpt.AspNetCore.Models;

namespace OpenAI.ChatGpt.AspNetCore;

/// <summary>
///  Factory for creating <see cref="ChatGPT" /> instances from DI.
/// </summary>
/// <example>
/// builder.Services.AddHttpClient&lt;ChatGPTFactory&lt;("OpenAiClient")
///     .AddPolicyHandler(GetRetryPolicy())
///     .AddPolicyHandler(GetCircuitBreakerPolicy());
/// </example>
[Fody.ConfigureAwait(false)]
// ReSharper disable once InconsistentNaming
public class ChatGPTFactory : IDisposable
{
    private readonly IOpenAiClient _client;
    private readonly ChatGPTConfig _config;
    private readonly IChatHistoryStorage _chatHistoryStorage;
    private readonly ITimeProvider _clock;
    private readonly bool _isHttpClientInjected;
    private volatile bool _ensureStorageCreatedCalled;

    public ChatGPTFactory(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenAICredentials> credentials,
        IOptions<ChatGPTConfig> config,
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock)
    {
        if (httpClientFactory == null) throw new ArgumentNullException(nameof(httpClientFactory));
        if (credentials?.Value == null) throw new ArgumentNullException(nameof(credentials));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _chatHistoryStorage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _client = CreateOpenAiClient(httpClientFactory, credentials);
        _isHttpClientInjected = true;
    }

    internal ChatGPTFactory(
        IOptions<OpenAICredentials> credentials,
        IOptions<ChatGPTConfig> config,
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
        ChatGPTConfig? config = null,
        string? host = null)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        _client = host is null ? new OpenAiClient(apiKey) : new OpenAiClient(apiKey, host);
        _config = config ?? ChatGPTConfig.Default;
        _chatHistoryStorage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? new TimeProviderUtc();
    }
    
    private OpenAiClient CreateOpenAiClient(
        IHttpClientFactory httpClientFactory, 
        IOptions<OpenAICredentials> credentials)
    {
        var httpClient = httpClientFactory.CreateClient(OpenAiClient.HttpClientName);
        httpClient.DefaultRequestHeaders.Authorization = credentials.Value.GetAuthHeader();
        httpClient.BaseAddress = new Uri(credentials.Value.ApiHost);
        return new OpenAiClient(httpClient);
    }

    public static ChatGPTFactory CreateInMemory(
        string apiKey, 
        ChatGPTConfig? config = null,
        string? host = null)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        return new ChatGPTFactory(apiKey, new InMemoryChatHistoryStorage(), new TimeProviderUtc(), config, host);
    }

    public async Task<ChatGPT> Create(
        string userId, 
        ChatGPTConfig? config = null, 
        bool ensureStorageCreated = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userId);
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
            ChatGPTConfig.Combine(_config, config)
        );
    }

    public async Task<ChatGPT> Create(
        ChatGPTConfig? config = null,
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
            ChatGPTConfig.Combine(_config, config)
        );
    }

    public void Dispose()
    {
        if (!_isHttpClientInjected && _client is IDisposable disposableClient)
        {
            disposableClient.Dispose();
        }
    }
}