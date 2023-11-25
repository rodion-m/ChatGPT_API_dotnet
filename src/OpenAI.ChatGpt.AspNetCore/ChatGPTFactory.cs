using Microsoft.Extensions.Options;

namespace OpenAI.ChatGpt.AspNetCore;

/// <summary>
///  Factory for creating <see cref="ChatGPT" /> instances from DI.
/// </summary>
[Fody.ConfigureAwait(false)]
// ReSharper disable once InconsistentNaming
public class ChatGPTFactory : IDisposable
{
    private readonly IAiClient _client;
    private readonly ChatGPTConfig _config;
    private readonly IChatHistoryStorage _chatHistoryStorage;
    private readonly ITimeProvider _clock;
    private readonly bool _isHttpClientInjected;
    private volatile bool _ensureStorageCreatedCalled;

    public ChatGPTFactory(
        IAiClient client,
        IOptions<ChatGPTConfig> config,
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _chatHistoryStorage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _isHttpClientInjected = true;
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