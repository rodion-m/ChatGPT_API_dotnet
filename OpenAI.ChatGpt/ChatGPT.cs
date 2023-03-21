using OpenAI.ChatGpt.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatGpt;

/// <summary> Chat conversations provider </summary>
public class ChatGPT : IDisposable
{
    private readonly string _userId;
    private readonly IMessageStore _messageStore;
    private readonly ChatCompletionsConfig? _config;
    private readonly OpenAiClient _client;
    private Chat? _currentChat;

    /// <summary>
    /// Use this constructor to create chat conversation provider for the specific user.
    /// </summary>
    public ChatGPT(
        OpenAiClient client,
        string userId, 
        IMessageStore messageStore, 
        ChatCompletionsConfig? config)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _userId = userId ?? throw new ArgumentNullException(nameof(userId));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _config = config;
    }
    
    /// <summary>
    /// If you don't have users use this ChatGPT constructor.
    /// </summary>
    public ChatGPT(
        OpenAiClient client,
        IMessageStore messageStore, 
        ChatCompletionsConfig? config)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _userId = Guid.Empty.ToString();
        _config = config;
    }

    /// <summary>
    /// If you don't have users and don't want to save messages into database use this method.
    /// </summary>
    public static Task<Chat> CreateInMemoryChat(
        string apiKey,
        ChatCompletionsConfig? config = null,
        UserOrSystemMessage? initialDialog = null)
    {
        if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
        var client = new OpenAiClient(apiKey);
        var chatGpt = new ChatGPT(client, new InMemoryMessageStore(), config);
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
        var topic = await _messageStore.GetLastTopicOrNull(_userId, cancellationToken);
        return topic is null
            ? await StartNewTopic(cancellationToken: cancellationToken)
            : await SetTopic(topic, cancellationToken);
    }
    
    /// <summary> Starts a new topic. </summary>
    public async Task<Chat> StartNewTopic(
        string? name = null,
        ChatCompletionsConfig? config = null,
        UserOrSystemMessage? initialDialog = null,
        DateTimeOffset? createdAt = null,
        CancellationToken cancellationToken = default)
    {
        createdAt ??= DateTimeOffset.Now;
        config = ChatCompletionsConfig.CombineOrDefault(_config, config);
        var topic = new Topic(_messageStore.NewTopicId(), _userId, name, createdAt.Value, config);
        await _messageStore.AddTopic(_userId, topic, cancellationToken);
        if (initialDialog is not null)
        {
            await _messageStore.SaveMessages(_userId, topic.Id, initialDialog.GetMessages(), cancellationToken);
        }

        _currentChat = CreateChat(topic, true);
        return _currentChat;
    }

    public async Task<Chat> SetTopic(Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await _messageStore.GetTopic(_userId, topicId, cancellationToken);
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
        return new Chat(_messageStore, _client, _userId, topic, isNew);
    }
    
    public async Task<IReadOnlyList<Topic>> GetTopics(CancellationToken cancellationToken = default)
    {
        var chats = await _messageStore.GetTopics(_userId, cancellationToken);
        return chats.ToList();
    }

    public void Stop()
    {
        _currentChat?.Stop();
    }
}