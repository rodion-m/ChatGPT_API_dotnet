using Microsoft.EntityFrameworkCore;

namespace OpenAI.ChatGpt.EntityFrameworkCore;

[Fody.ConfigureAwait(false)]
public class EfChatHistoryStorage : IChatHistoryStorage
{
    private readonly ChatGptDbContext _dbContext;
    
    public EfChatHistoryStorage(ChatGptDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Topic>> GetTopics(
        string userId, CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        return await _dbContext.Topics.ToListAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Topic> GetTopic(
        string userId, Guid topicId, CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        var topic = await _dbContext.Topics.FirstOrDefaultAsync(
            it => it.Id == topicId && it.UserId == userId,
            cancellationToken: cancellationToken);
        return topic ?? throw new TopicNotFoundException(topicId);
    }

    /// <inheritdoc />
    public async Task AddTopic(Topic topic, CancellationToken cancellationToken)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        await _dbContext.Topics.AddAsync(topic, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveMessages(
        string userId,
        Guid topicId,
        IEnumerable<PersistentChatMessage> messages,
        CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        await _dbContext.AddRangeAsync(messages, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PersistentChatMessage>> GetMessages(
        string userId, Guid topicId, CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        return await _dbContext.Messages
            .Where(it => it.TopicId == topicId && it.UserId == userId)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<Topic?> GetMostRecentTopicOrNull(string userId, CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        return _dbContext.Topics
            .Where(it => it.UserId == userId)
            .OrderByDescending(it => it.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task EditTopicName(
        string userId, Guid topicId, string newName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(newName);
        var topic = await GetTopic(userId, topicId, cancellationToken);
        topic.Name = newName;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteTopic(
        string userId, Guid topicId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        var topic = await _dbContext.Topics.FirstOrDefaultAsync(
            it => it.Id == topicId && it.UserId == userId,
            cancellationToken: cancellationToken);
        if (topic == null) return false;
        _dbContext.Topics.Remove(topic);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task EditMessage(
        string userId,
        Guid topicId,
        Guid messageId,
        string newMessage,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(newMessage);
        var message = await _dbContext.Messages.FirstOrDefaultAsync(
            it => it.Id == messageId && it.TopicId == topicId && it.UserId == userId,
            cancellationToken: cancellationToken);
        if (message == null) return;
        message.Content = newMessage;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteMessage(
        string userId, Guid topicId, Guid messageId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        var message = await _dbContext.Messages.FirstOrDefaultAsync(
            it => it.Id == messageId && it.TopicId == topicId && it.UserId == userId,
            cancellationToken: cancellationToken);
        if (message == null) return false;
        _dbContext.Messages.Remove(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public Task EnsureStorageCreated(CancellationToken cancellationToken)
    {
        return _dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}