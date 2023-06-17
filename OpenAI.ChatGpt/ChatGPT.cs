using OpenAI.ChatGpt.Interfaces;
using OpenAI.ChatGpt.Internal;
using OpenAI.ChatGpt.Models;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt;

/// <summary> Chat conversations provider. </summary>
[Fody.ConfigureAwait(false)]
// ReSharper disable once InconsistentNaming
public class ChatGPT : IDisposable
{
    private readonly string _userId;
    private readonly IChatHistoryStorage _storage;
    private readonly ITimeProvider _clock;
    private readonly ChatGPTConfig? _config;
    private readonly IOpenAiClient _client;
    private ChatService? _currentChat;
    
    private static readonly string NoUser = Guid.Empty.ToString();
    private readonly bool _isClientInjected;

    /// <summary>
    /// Use this constructor to create chat conversation provider for the specific user.
    /// </summary>
    public ChatGPT(
        IOpenAiClient client,
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock,
        string userId,
        ChatGPTConfig? config)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _userId = userId ?? throw new ArgumentNullException(nameof(userId));
        _storage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _config = config;
        _isClientInjected = true;
    }
    
    /// <summary>
    /// If you don't have users use this ChatGPT constructor.
    /// </summary>
    public ChatGPT(
        IOpenAiClient client,
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock,
        ChatGPTConfig? config)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _storage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _userId = NoUser;
        _config = config;
        _isClientInjected = true;
    }
    
    public ChatGPT(
        string apiKey,
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock,
        string? userId,
        ChatGPTConfig? config,
        string? host)
    {
        _client = host is null ? new OpenAiClient(apiKey) : new OpenAiClient(apiKey, host);
        _userId = userId ?? NoUser;
        _storage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _config = config;
        _isClientInjected = false;
    }

    /// <summary>
    /// If you don't have users and don't want to save messages into database use this method.
    /// </summary>
    public static Task<ChatService> CreateInMemoryChat(
        string apiKey,
        ChatGPTConfig? config = null,
        UserOrSystemMessage? initialDialog = null,
        ITimeProvider? clock = null,
        string? host = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        var chatGpt = new ChatGPT(
            apiKey, new InMemoryChatHistoryStorage(), clock ?? new TimeProviderUtc(), null, config, host);
        return chatGpt.StartNewTopic(initialDialog: initialDialog, cancellationToken: cancellationToken);
    }
    
    public void Dispose()
    {
        Stop();
        _currentChat?.Dispose();
        if (!_isClientInjected)
        {
            _client.Dispose();
        }
    }

    /// <summary> Continues the last topic or starts a new one.</summary>
    public async Task<ChatService> ContinueOrStartNewTopic(CancellationToken cancellationToken = default)
    {
        if (_currentChat is not null) return _currentChat;
        var topic = await _storage.GetMostRecentTopicOrNull(_userId, cancellationToken);
        return topic is null
            ? await StartNewTopic(cancellationToken: cancellationToken)
            : await SetTopic(topic, cancellationToken);
    }
    
    /// <summary> Starts a new topic. </summary>
    public async Task<ChatService> StartNewTopic(
        string? name = null,
        ChatGPTConfig? config = null,
        UserOrSystemMessage? initialDialog = null,
        bool clearOnDisposal = false,
        CancellationToken cancellationToken = default)
    {
        config = ChatGPTConfig.CombineOrDefault(_config, config);
        var topic = new Topic(_storage.NewTopicId(), _userId, name, _clock.GetCurrentTime(), config);
        await _storage.AddTopic(topic, cancellationToken);
        initialDialog ??= config.GetInitialDialogOrNull();
        if (initialDialog is not null)
        {
            var messages = ConvertToPersistentMessages(initialDialog, topic);
            await _storage.SaveMessages(_userId, topic.Id, messages, cancellationToken);
        }

        _currentChat = CreateChat(topic, initialDialog is null, clearOnDisposal: clearOnDisposal);
        return _currentChat;
    }

    private IEnumerable<PersistentChatMessage> ConvertToPersistentMessages(ChatCompletionMessage dialog, Topic topic)
    {
        return dialog.GetMessages()
            .Select(m => new PersistentChatMessage(
                _storage.NewMessageId(), _userId, topic.Id, _clock.GetCurrentTime(), m)
            );
    }

    public async Task<ChatService> SetTopic(Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await _storage.GetTopic(_userId, topicId, cancellationToken);
        if (topic is null)
        {
            throw new ArgumentException($"Chat with id {topicId} not found for user {_userId}");
        }
        return await SetTopic(topic, cancellationToken);
    }

    private Task<ChatService> SetTopic(Topic topic, CancellationToken cancellationToken = default)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        _currentChat = CreateChat(topic, false, false);
        return Task.FromResult(_currentChat);
    }

    private ChatService CreateChat(Topic topic, bool isNew, bool clearOnDisposal)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        return new ChatService(_storage, _clock, _client, _userId, topic, isNew, clearOnDisposal);
    }
    
    public async Task<IReadOnlyList<Topic>> GetTopics(CancellationToken cancellationToken = default)
    {
        var chats = await _storage.GetTopics(_userId, cancellationToken);
        return chats.ToList();
    }

    public void Stop()
    {
        _currentChat?.Stop();
    }
}