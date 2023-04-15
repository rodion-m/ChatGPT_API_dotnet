using System.Collections.Concurrent;
using OpenAI.ChatGpt.Exceptions;
using OpenAI.ChatGpt.Interfaces;
using OpenAI.ChatGpt.Models;

namespace OpenAI.ChatGpt;

public class InMemoryChatHistoryStorage : IChatHistoryStorage
{
    private readonly ConcurrentDictionary<string, Dictionary<Guid, Topic>> _users = new();

    private readonly ConcurrentDictionary<string, Dictionary<Guid, List<PersistentChatMessage>>>
        _messages = new();

    /// <inheritdoc/>
    public Task SaveMessages(
        string userId,
        Guid topicId,
        IEnumerable<PersistentChatMessage> messages,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(messages);
        cancellationToken.ThrowIfCancellationRequested();
        if (!_messages.TryGetValue(userId, out var userMessages))
        {
            userMessages = new Dictionary<Guid, List<PersistentChatMessage>>();
            _messages.TryAdd(userId, userMessages);
        }

        if (!userMessages.TryGetValue(topicId, out var chatMessages))
        {
            chatMessages = new List<PersistentChatMessage>();
            userMessages.TryAdd(topicId, chatMessages);
        }

        chatMessages.AddRange(messages);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<PersistentChatMessage>> GetMessages(
        string userId, Guid topicId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        cancellationToken.ThrowIfCancellationRequested();
        if (!_messages.TryGetValue(userId, out var userMessages))
        {
            return Task.FromResult(Enumerable.Empty<PersistentChatMessage>());
        }

        if (!userMessages.TryGetValue(topicId, out var chatMessages))
        {
            return Task.FromResult(Enumerable.Empty<PersistentChatMessage>());
        }

        return Task.FromResult(chatMessages.AsEnumerable());
    }

    /// <inheritdoc/>
    public Task<Topic?> GetMostRecentTopicOrNull(string userId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        cancellationToken.ThrowIfCancellationRequested();
        if (!_users.TryGetValue(userId, out var userChats))
        {
            return Task.FromResult<Topic?>(null);
        }

        var lastTopic = userChats.Values.MaxBy(x => x.CreatedAt);
        return Task.FromResult(lastTopic);
    }

    /// <inheritdoc/>
    public async Task EditTopicName(
        string userId, Guid topicId, string newName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(newName);
        cancellationToken.ThrowIfCancellationRequested();
        var topic = await GetTopic(userId, topicId, cancellationToken);
        topic.Name = newName;
    }

    /// <inheritdoc/>
    public Task<bool> DeleteTopic(string userId, Guid topicId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        cancellationToken.ThrowIfCancellationRequested();
        if (!_users.TryGetValue(userId, out var userChats))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(userChats.Remove(topicId));
    }

    /// <inheritdoc/>
    public async Task EditMessage(
        string userId,
        Guid topicId,
        Guid messageId,
        string newMessage,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(newMessage);
        cancellationToken.ThrowIfCancellationRequested();
        var messages = await GetMessages(userId, topicId, cancellationToken);
        var message = messages.FirstOrDefault(x => x.Id == messageId);
        if (message == null)
        {
            throw new MessageNotFoundException(messageId);
        }

        message.Content = newMessage;
    }

    /// <inheritdoc/>
    public Task<bool> DeleteMessage(
        string userId, Guid topicId, Guid messageId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        cancellationToken.ThrowIfCancellationRequested();
        if (!_messages.TryGetValue(userId, out var userMessages))
        {
            return Task.FromResult(false);
        }

        if (userMessages.TryGetValue(topicId, out var chatMessages))
        {
            return Task.FromResult(chatMessages.RemoveAll(m => m.Id == messageId) == 1);
        }

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<Topic>> GetTopics(string userId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        cancellationToken.ThrowIfCancellationRequested();
        if (!_users.TryGetValue(userId, out var topics))
        {
            return Task.FromResult(Enumerable.Empty<Topic>());
        }

        return Task.FromResult(topics.Values.AsEnumerable());
    }

    /// <inheritdoc/>
    public Task<Topic> GetTopic(string userId, Guid topicId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        cancellationToken.ThrowIfCancellationRequested();
        if (!_users.TryGetValue(userId, out var userChats))
        {
            throw new TopicNotFoundException(topicId);
        }

        if (!userChats.TryGetValue(topicId, out var topic))
        {
            throw new TopicNotFoundException(topicId);
        }

        return Task.FromResult(topic);
    }

    /// <inheritdoc/>
    public Task AddTopic(Topic topic, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(topic);
        cancellationToken.ThrowIfCancellationRequested();
        if (!_users.TryGetValue(topic.UserId, out var userTopics))
        {
            userTopics = new Dictionary<Guid, Topic>();
            _users.TryAdd(topic.UserId, userTopics);
        }

        userTopics.Add(topic.Id, topic);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task EnsureStorageCreated(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}