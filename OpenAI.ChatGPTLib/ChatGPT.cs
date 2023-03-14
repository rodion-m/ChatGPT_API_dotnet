using OpenAI.ChatCompletions.Chat.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatCompletions.Chat;

public class ChatGPT : IDisposable
{
    private readonly string _userId;
    private readonly IMessageStore _messageStore;
    private readonly ChatCompletionsConfig _config;
    private readonly OpenAiClient _client;
    private Chat? _currentChat;

    public ChatGPT(
        OpenAiClient client,
        string userId, 
        IMessageStore messageStore, 
        ChatCompletionsConfig config)
    {
        _client = client;
        _userId = userId ?? throw new ArgumentNullException(nameof(userId));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }
    
    /// <summary>
    /// If your don't have users use this ChatGPT constructor
    /// </summary>
    public ChatGPT(
        OpenAiClient client,
        IMessageStore messageStore, 
        ChatCompletionsConfig config)
    {
        _client = client;
        _userId = Guid.Empty.ToString();
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    // private async Task<Chat> ContinueOrStartNewChat(CancellationToken cancellationToken = default)
    // {
    //     if (_currentChat is not null) return _currentChat;
    //     var id = await _messageStore.GetLastChatIdOrNull(_userId, cancellationToken);
    //     return id is null
    //         ? await StartNewChat(cancellationToken: cancellationToken)
    //         : CreateChat(id.Value, false, null);
    // }
    
    public async Task<Chat> StartNewChat(
        string? name = null,
        ChatCompletionsConfig? config = null,
        DateTimeOffset? createdAt = null,
        CancellationToken cancellationToken = default)
    {
        var chatInfo = new ChatInfo(Guid.NewGuid(), _userId, name, createdAt ?? DateTimeOffset.Now, config ?? _config);
        //await _messageStore.SetCurrentChatId(_userId, chatId, cancellationToken);
        _currentChat = CreateChat(chatInfo, true);
        return _currentChat;
    }
    
    public async Task<Chat> SetChat(
        Guid chatId,
        CancellationToken cancellationToken = default)
    {
        var chatInfo = await _messageStore.GetChatInfo(_userId, chatId, cancellationToken);
        if (chatInfo is null)
        {
            throw new ArgumentException($"Chat with id {chatId} not found for user {_userId}");
        }
        _currentChat = CreateChat(chatInfo, false);
        await _messageStore.AddChatInfo(_userId, chatInfo, cancellationToken);
        //await _messageStore.SetCurrentChatId(_userId, chatId, cancellationToken);
        return _currentChat;
    }

    private Chat CreateChat(ChatInfo chatInfo, bool isNew)
    {
        if (chatInfo == null) throw new ArgumentNullException(nameof(chatInfo));
        return new Chat(_messageStore, _client, _userId, chatInfo, isNew);
    }
    
    public async Task<IReadOnlyList<ChatInfo>> GetChats(CancellationToken cancellationToken = default)
    {
        var chats = await _messageStore.GetChats(_userId, cancellationToken);
        return chats.ToList();
    }

    // public async IAsyncEnumerable<string> StreamNextMessageResponse(
    //     string message,
    //     Guid chatId = default,
    //     [EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    //     (chatId, var history) 
    //         = await GetOrCreateChat(chatId, cancellationToken);
    //     var messages = history.Append(new UserMessage(message));
    //     //todo calculate chat length and check against max tokens
    //     var sb = new StringBuilder();
    //     await foreach (var chunk in _client.StreamChatCompletions(
    //         messages, _defaultMaxTokens, cancellationToken: _cts.Token))
    //     {
    //         sb.Append(chunk);
    //         yield return chunk;
    //     }
    //     
    //     await _messageStore.SaveMessages(
    //         _userId, chatId, message, sb.ToString(), _cts.Token);
    // }
    //
    // private async Task<(Guid, IEnumerable<ChatCompletionMessage>)> GetOrCreateChat(
    //     Guid chatId, CancellationToken cancellationToken)
    // {
    //     // todo reduce complexity
    //     if (chatId == default)
    //     {
    //         var currentId = _lastChatId ?? await _messageStore.GetChatIdOrNull(_userId, cancellationToken);
    //         if (currentId is null)
    //         {
    //             chatId = await StartNewChat(cancellationToken);
    //             return (chatId, Enumerable.Empty<ChatCompletionMessage>());
    //         }
    //     }
    //     
    //     return (chatId, 
    //         await _messageStore.GetMessages(_userId, chatId, cancellationToken));
    // }

    public void Stop()
    {
        _currentChat?.Stop();
    }

    public void Dispose()
    {
        _currentChat?.Dispose();
    }
}

public interface IMessageStore
{
    Task SaveMessages(string userId, Guid chatId, IEnumerable<ChatCompletionMessage> messages, CancellationToken cancellationToken);
    Task<IEnumerable<ChatCompletionMessage>> GetMessages(string userId, Guid chatId, CancellationToken cancellationToken);
    
    Task<IEnumerable<ChatInfo>> GetChats(string userId, CancellationToken cancellationToken);

    Task SaveMessages(
        string userId, Guid chatId, 
        string userMessage, string assistantMessage, CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (userMessage == null) throw new ArgumentNullException(nameof(userMessage));
        if (assistantMessage == null) throw new ArgumentNullException(nameof(assistantMessage));
        var enumerable =  new ChatCompletionMessage[]
        {
            new UserMessage(userMessage),
            new AssistantMessage(assistantMessage)
        };
        return SaveMessages(userId, chatId, enumerable, cancellationToken);
    }
    
    Task<ChatInfo> GetChatInfo(string userId, Guid chatId, CancellationToken cancellationToken);
    Task AddChatInfo(string userId, ChatInfo chatInfo, CancellationToken cancellationToken);
    
    //Task SetCurrentChatId(string userId, Guid chatId, CancellationToken cancellationToken);
// #pragma warning disable CA1822
//     internal Task<Guid?> GetLastChatIdOrNull(string userId, CancellationToken cancellationToken)
// #pragma warning restore CA1822
//     {
//         throw new NotImplementedException("This method is reserved to the future");
//     }

}