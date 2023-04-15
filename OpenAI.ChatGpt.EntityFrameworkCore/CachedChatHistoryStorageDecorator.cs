using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenAI.ChatGpt.Interfaces;
using OpenAI.ChatGpt.Models;

namespace OpenAI.ChatGpt.EntityFrameworkCore;

public class CachedChatHistoryStorageDecorator : IChatHistoryStorage
{
    private readonly IChatHistoryStorage _chatHistoryStorage;
    private readonly IMemoryCache _cache;
    private readonly ChatHistoryCacheConfig _cacheConfig;

    public CachedChatHistoryStorageDecorator(
        IChatHistoryStorage chatHistoryStorage, 
        IMemoryCache cache, 
        IOptions<ChatHistoryCacheConfig> cacheConfig)
    {
        ArgumentNullException.ThrowIfNull(cacheConfig);
        ArgumentNullException.ThrowIfNull(cacheConfig.Value);
        _chatHistoryStorage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _cacheConfig = cacheConfig.Value;
    }
    
    private string GetUserTopicsKey(string userId) => $"chatbot_topics_{userId}";
    private string GetMessagesKey(Guid topicId) => $"chatbot_messages_{topicId}";
    private string GetTopicKey(Guid topicId) => $"chatbot_topic_{topicId}";

    /// <inheritdoc/>
    public Task<IEnumerable<Topic>> GetTopics(string userId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        return _cache.GetOrCreateAsync(GetUserTopicsKey(userId), entry =>
        {
            entry.SlidingExpiration = _cacheConfig.TopicsSlidingExpiration;
            return _chatHistoryStorage.GetTopics(userId, cancellationToken);
        })!;
    }
    
    /// <inheritdoc/>
    public Task<Topic> GetTopic(string userId, Guid topicId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        return _cache.GetOrCreateAsync(GetTopicKey(topicId), entry =>
        {
            entry.SlidingExpiration = _cacheConfig.TopicsSlidingExpiration;
            if (GetTopcsFromCacheOrNull(userId) is { } topics)
                return Task.FromResult(topics.Single(t => t.Id == topicId));
            
            return _chatHistoryStorage.GetTopic(userId, topicId, cancellationToken);
        })!;
    }

    private IEnumerable<Topic>? GetTopcsFromCacheOrNull(string userId)
    {
        return _cache.Get<IEnumerable<Topic>>(GetUserTopicsKey(userId));
    }
    
    private IEnumerable<PersistentChatMessage>? GetMessagesFromCacheOrNull(Guid topicId)
    {
        return _cache.Get<IEnumerable<PersistentChatMessage>>(GetMessagesKey(topicId));
    }

    /// <inheritdoc/>
    public async Task AddTopic(Topic topic, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(topic);
        await _chatHistoryStorage.AddTopic(topic, cancellationToken);
        if (GetTopcsFromCacheOrNull(topic.UserId) is List<Topic> topics)
        {
            topics.Add(topic);
        }
        else
        {
            _cache.Remove(GetUserTopicsKey(topic.UserId));
        }
    }

    /// <inheritdoc/>
    public async Task SaveMessages(
        string userId,
        Guid topicId, 
        IEnumerable<PersistentChatMessage> messages,
        CancellationToken cancellationToken)
    {
        var messagesList = messages.ToList();
        await _chatHistoryStorage.SaveMessages(userId, topicId, messagesList, cancellationToken);
        if (GetMessagesFromCacheOrNull(topicId) is List<PersistentChatMessage> allMessages)
        {
            allMessages.AddRange(messagesList);
        }
        else
        {
            _cache.Remove(GetMessagesKey(topicId));
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<PersistentChatMessage>> GetMessages(
        string userId, Guid topicId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        return _cache.GetOrCreateAsync(GetMessagesKey(topicId), entry =>
        {
            entry.SlidingExpiration = _cacheConfig.MessagesSlidingExpiration;
            return _chatHistoryStorage.GetMessages(userId, topicId, cancellationToken);
        })!;
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
        await _chatHistoryStorage.EditMessage(userId, topicId, messageId, newMessage, cancellationToken);
        if (GetMessagesFromCacheOrNull(topicId) is { } allMessages)
        {
            var message = allMessages.Single(m => m.Id == messageId);
            message.Content = newMessage;
        }
        else
        {
            _cache.Remove(GetMessagesKey(topicId));
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteMessage(
        string userId, Guid topicId, Guid messageId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        var deleted = await _chatHistoryStorage.DeleteMessage(userId, topicId, messageId, cancellationToken);
        if (deleted)
        {
            if (GetMessagesFromCacheOrNull(topicId) is List<PersistentChatMessage> allMessages)
            {
                allMessages.RemoveAll(it => it.Id == messageId);
            }
            else
            {
                _cache.Remove(GetMessagesKey(topicId));
            }
        }

        return deleted;
    }

    /// <inheritdoc/>
    public async Task<Topic?> GetMostRecentTopicOrNull(string userId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        var topics = await GetTopics(userId, cancellationToken);
        return topics.MaxBy(t => t.CreatedAt);
    }

    /// <inheritdoc/>
    public async Task EditTopicName(
        string userId, Guid topicId, string newName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(newName);
        await _chatHistoryStorage.EditTopicName(userId, topicId, newName, cancellationToken);
        if (GetTopcsFromCacheOrNull(userId) is { } topics)
        {
            var topic = topics.Single(t => t.Id == topicId);
            topic.Name = newName;
        }
        else
        {
            _cache.Remove(GetUserTopicsKey(userId));
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteTopic(string userId, Guid topicId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        var deleted = await _chatHistoryStorage.DeleteTopic(userId, topicId, cancellationToken);
        if (deleted)
        {
            if (GetTopcsFromCacheOrNull(userId) is List<Topic> topics)
            {
                topics.RemoveAll(it => it.Id == topicId);
            }
            else
            {
                _cache.Remove(GetUserTopicsKey(userId));
            }
        }

        return deleted;
    }

    /// <inheritdoc/>
    public Task EnsureStorageCreated(CancellationToken cancellationToken) 
        => _chatHistoryStorage.EnsureStorageCreated(cancellationToken);
}