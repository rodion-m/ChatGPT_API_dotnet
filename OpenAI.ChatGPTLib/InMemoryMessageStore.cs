using System.Collections.Concurrent;
using OpenAI.ChatCompletions.Chat.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatCompletions.Chat;

public class InMemoryMessageStore : IMessageStore
{
    private readonly ConcurrentDictionary<string, Dictionary<Guid, ChatInfo>> _users = new();
    private readonly ConcurrentDictionary<string, Dictionary<Guid, List<ChatCompletionMessage>>> 
        _messages = new();

    public Task SaveMessages(
        string userId, 
        Guid chatId, 
        IEnumerable<ChatCompletionMessage> messages,
        CancellationToken cancellationToken)
    {
        if (!_messages.TryGetValue(userId, out var userMessages))
        {
            userMessages = new Dictionary<Guid, List<ChatCompletionMessage>>();
            _messages.TryAdd(userId, userMessages);
        }

        if (!userMessages.TryGetValue(chatId, out var chatMessages))
        {
            chatMessages = new List<ChatCompletionMessage>();
            userMessages.TryAdd(chatId, chatMessages);
        }

        chatMessages.AddRange(messages);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ChatCompletionMessage>> GetMessages(string userId, Guid chatId, CancellationToken cancellationToken)
    {
        if (!_messages.TryGetValue(userId, out var userMessages))
        {
            return Task.FromResult(Enumerable.Empty<ChatCompletionMessage>());
        }

        if (!userMessages.TryGetValue(chatId, out var chatMessages))
        {
            return Task.FromResult(Enumerable.Empty<ChatCompletionMessage>());
        }

        return Task.FromResult(chatMessages.AsEnumerable());
    }

    public Task<IEnumerable<ChatInfo>> GetChats(string userId, CancellationToken cancellationToken)
    {
        if (!_users.TryGetValue(userId, out var userChats))
        {
            return Task.FromResult(Enumerable.Empty<ChatInfo>());
        }

        return Task.FromResult(userChats.Values.AsEnumerable());
    }

    public Task<ChatInfo> GetChatInfo(string userId, Guid chatId, CancellationToken cancellationToken)
    {
        if (!_users.TryGetValue(userId, out var userChats))
        {
            throw new ArgumentException($"Chats for user {userId} not found");
        }

        if (!userChats.TryGetValue(chatId, out var chatInfo))
        {
            throw new ArgumentException($"Chat {chatId} for user {userId} not found");
        }

        return Task.FromResult(chatInfo);
    }

    public Task AddChatInfo(string userId, ChatInfo chatInfo, CancellationToken cancellationToken)
    {
        if (!_users.TryGetValue(userId, out var userChats))
        {
            userChats = new Dictionary<Guid, ChatInfo>();
            _users.TryAdd(userId, userChats);
        }

        userChats.Add(chatInfo.Id, chatInfo);
        return Task.CompletedTask;
    }
}