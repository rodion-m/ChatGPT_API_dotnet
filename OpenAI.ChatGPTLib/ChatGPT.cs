using OpenAI.ChatCompletions.Chat.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatCompletions.Chat;

public class ChatGPT : IDisposable
{
    private readonly string _userId;
    private readonly IMessageStore _messageStore;
    private readonly ChatCompletionsConfig? _config;
    private readonly OpenAiClient _client;
    private Chat? _currentChat;

    public ChatGPT(
        OpenAiClient client,
        string userId, 
        IMessageStore messageStore, 
        ChatCompletionsConfig? config)
    {
        _client = client;
        _userId = userId ?? throw new ArgumentNullException(nameof(userId));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _config = config;
    }
    
    /// <summary>
    /// If your don't have users use this ChatGPT constructor
    /// </summary>
    public ChatGPT(
        OpenAiClient client,
        IMessageStore messageStore, 
        ChatCompletionsConfig? config)
    {
        _client = client;
        _userId = Guid.Empty.ToString();
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _config = config;
    }

    
    /// <summary>
    /// If you don't have users and don't want to save messages into database use this method.
    /// </summary>
    public static Task<Chat> CreateInMemoryChat(
        string apiKey,
        ChatCompletionsConfig? config = null,
        ChatCompletionDialog? initialDialog = null)
    {
        if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
        var client = new OpenAiClient(apiKey);
        var chatGpt = new ChatGPT(client, new InMemoryMessageStore(), config);
        return chatGpt.StartNewTopic(initialDialog: initialDialog);
    }

    // private async Task<Chat> ContinueOrStartNewChat(CancellationToken cancellationToken = default)
    // {
    //     if (_currentChat is not null) return _currentChat;
    //     var id = await _messageStore.GetLastChatIdOrNull(_userId, cancellationToken);
    //     return id is null
    //         ? await StartNewChat(cancellationToken: cancellationToken)
    //         : CreateChat(id.Value, false, null);
    // }
    
    public async Task<Chat> StartNewTopic(
        string? name = null,
        ChatCompletionsConfig? config = null,
        ChatCompletionDialog? initialDialog = null,
        DateTimeOffset? createdAt = null,
        CancellationToken cancellationToken = default)
    {
        var topic = new Topic(_messageStore.NewTopicId(), _userId, name, createdAt ?? DateTimeOffset.Now, 
            ChatCompletionsConfig.CombineOrDefault(_config, config));
        if (initialDialog is not null)
        {
            await _messageStore.SaveMessages(_userId, topic.Id, initialDialog.GetMessages(), cancellationToken);
        }

        //await _messageStore.SetCurrentChatId(_userId, topicId, cancellationToken);
        _currentChat = CreateChat(topic, true);
        return _currentChat;
    }
    
    public async Task<Chat> SetTopic(
        Guid topicId,
        CancellationToken cancellationToken = default)
    {
        var topic = await _messageStore.GetTopic(_userId, topicId, cancellationToken);
        if (topic is null)
        {
            throw new ArgumentException($"Chat with id {topicId} not found for user {_userId}");
        }
        _currentChat = CreateChat(topic, false);
        await _messageStore.AddTopic(_userId, topic, cancellationToken);
        //await _messageStore.SetCurrentChatId(_userId, topicId, cancellationToken);
        return _currentChat;
    }

    private Chat CreateChat(Topic topic, bool isNew)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        return new Chat(_messageStore, _client, _userId, topic, isNew);
    }
    
    public async Task<IReadOnlyList<Topic>> GetChats(CancellationToken cancellationToken = default)
    {
        var chats = await _messageStore.GetTopics(_userId, cancellationToken);
        return chats.ToList();
    }

    public void Stop()
    {
        _currentChat?.Stop();
    }

    public void Dispose()
    {
        _currentChat?.Dispose();
    }
}