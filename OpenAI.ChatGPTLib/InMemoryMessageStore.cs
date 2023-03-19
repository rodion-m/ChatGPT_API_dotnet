using System.Collections.Concurrent;
using OpenAI.ChatCompletions.Chat.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatCompletions.Chat;

public class InMemoryMessageStore : IMessageStore
{
    private readonly ConcurrentDictionary<string, Dictionary<Guid, Topic>> _users = new();
    private readonly ConcurrentDictionary<string, Dictionary<Guid, List<ChatCompletionMessage>>> 
        _messages = new();

    public Task SaveMessages(
        string userId, 
        Guid topicId, 
        IEnumerable<ChatCompletionMessage> messages,
        CancellationToken cancellationToken)
    {
        if (!_messages.TryGetValue(userId, out var userMessages))
        {
            userMessages = new Dictionary<Guid, List<ChatCompletionMessage>>();
            _messages.TryAdd(userId, userMessages);
        }

        if (!userMessages.TryGetValue(topicId, out var chatMessages))
        {
            chatMessages = new List<ChatCompletionMessage>();
            userMessages.TryAdd(topicId, chatMessages);
        }

        chatMessages.AddRange(messages);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ChatCompletionMessage>> GetMessages(string userId, Guid topicId, CancellationToken cancellationToken)
    {
        if (!_messages.TryGetValue(userId, out var userMessages))
        {
            return Task.FromResult(Enumerable.Empty<ChatCompletionMessage>());
        }

        if (!userMessages.TryGetValue(topicId, out var chatMessages))
        {
            return Task.FromResult(Enumerable.Empty<ChatCompletionMessage>());
        }

        return Task.FromResult(chatMessages.AsEnumerable());
    }

    public Task<IEnumerable<Topic>> GetTopics(string userId, CancellationToken cancellationToken)
    {
        if (!_users.TryGetValue(userId, out var topics))
        {
            return Task.FromResult(Enumerable.Empty<Topic>());
        }

        return Task.FromResult(topics.Values.AsEnumerable());
    }

    public Task<Topic> GetTopic(string userId, Guid topicId, CancellationToken cancellationToken)
    {
        if (!_users.TryGetValue(userId, out var userChats))
        {
            throw new ArgumentException($"Chats for user {userId} not found");
        }

        if (!userChats.TryGetValue(topicId, out var topic))
        {
            throw new ArgumentException($"Chat {topicId} for user {userId} not found");
        }

        return Task.FromResult(topic);
    }

    public Task AddTopic(string userId, Topic topic, CancellationToken cancellationToken)
    {
        if (!_users.TryGetValue(userId, out var userChats))
        {
            userChats = new Dictionary<Guid, Topic>();
            _users.TryAdd(userId, userChats);
        }

        userChats.Add(topic.Id, topic);
        return Task.CompletedTask;
    }
}