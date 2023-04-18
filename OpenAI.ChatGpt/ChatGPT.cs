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
    private readonly IChatHistoryStorage _chatHistoryStorage;
    private readonly ITimeProvider _clock;
    private readonly ChatCompletionsConfig? _config;
    private readonly OpenAiClient _client;
    private Chat? _currentChat;

    /// <summary>
    /// Use this constructor to create chat conversation provider for the specific user.
    /// </summary>
    public ChatGPT(
        OpenAiClient client,
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock,
        string userId,
        ChatCompletionsConfig? config)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _userId = userId ?? throw new ArgumentNullException(nameof(userId));
        _chatHistoryStorage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _config = config;
    }
    
    /// <summary>
    /// If you don't have users use this ChatGPT constructor.
    /// </summary>
    public ChatGPT(
        OpenAiClient client,
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock,
        ChatCompletionsConfig? config)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _chatHistoryStorage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _userId = Guid.Empty.ToString();
        _config = config;
    }

    /// <summary>
    /// If you don't have users and don't want to save messages into database use this method.
    /// </summary>
    public static Task<Chat> CreateInMemoryChat(
        string apiKey,
        ChatCompletionsConfig? config = null,
        UserOrSystemMessage? initialDialog = null,
        ITimeProvider? clock = null)
    {
        if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
        var client = new OpenAiClient(apiKey);
        var chatGpt = new ChatGPT(client, new InMemoryChatHistoryStorage(), clock ?? new TimeProviderUtc(), config);
        return chatGpt.StartNewTopic(initialDialog: initialDialog);
    }
    
    public void Dispose()
    {
        _currentChat?.Dispose();
    }

    /// <summary> Continues the last topic or starts a new one.</summary>
    public async Task<Chat> ContinueOrStartNewTopic(
        DateTimeOffset? createdAt = null,
        CancellationToken cancellationToken = default)
    {
        if (_currentChat is not null) return _currentChat;
        var topic = await _chatHistoryStorage.GetMostRecentTopicOrNull(_userId, cancellationToken);
        return topic is null
            ? await StartNewTopic(cancellationToken: cancellationToken)
            : await SetTopic(topic, cancellationToken);
    }
    
    /// <summary> Starts a new topic. </summary>
    public async Task<Chat> StartNewTopic(
        string? name = null,
        ChatCompletionsConfig? config = null,
        UserOrSystemMessage? initialDialog = null,
        CancellationToken cancellationToken = default)
    {
        config = ChatCompletionsConfig.CombineOrDefault(_config, config);
        var topic = new Topic(_chatHistoryStorage.NewTopicId(), _userId, name, _clock.GetCurrentTime(), config);
        await _chatHistoryStorage.AddTopic(topic, cancellationToken);
        initialDialog ??= config.GetInitialDialogOrNull();
        if (initialDialog is not null)
        {
            var messages = ConvertToPersistentMessages(initialDialog, topic);
            await _chatHistoryStorage.SaveMessages(_userId, topic.Id, messages, cancellationToken);
        }

        _currentChat = CreateChat(topic, initialDialog is null);
        return _currentChat;
    }

    private IEnumerable<PersistentChatMessage> ConvertToPersistentMessages(ChatCompletionMessage dialog, Topic topic)
    {
        return dialog.GetMessages()
            .Select(m => new PersistentChatMessage(
                _chatHistoryStorage.NewMessageId(), _userId, topic.Id, _clock.GetCurrentTime(), m)
            );
    }

    public async Task<Chat> SetTopic(Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await _chatHistoryStorage.GetTopic(_userId, topicId, cancellationToken);
        if (topic is null)
        {
            throw new ArgumentException($"Chat with id {topicId} not found for user {_userId}");
        }
        return await SetTopic(topic, cancellationToken);
    }

    private Task<Chat> SetTopic(Topic topic, CancellationToken cancellationToken = default)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        _currentChat = CreateChat(topic, false);
        return Task.FromResult(_currentChat);
    }

    private Chat CreateChat(Topic topic, bool isNew)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        return new Chat(_chatHistoryStorage, _clock, _client, _userId, topic, isNew);
    }
    
    public async Task<IReadOnlyList<Topic>> GetTopics(CancellationToken cancellationToken = default)
    {
        var chats = await _chatHistoryStorage.GetTopics(_userId, cancellationToken);
        return chats.ToList();
    }

    public void Stop()
    {
        _currentChat?.Stop();
    }
}