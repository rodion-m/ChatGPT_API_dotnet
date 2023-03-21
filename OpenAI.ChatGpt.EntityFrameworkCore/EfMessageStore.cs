using Microsoft.EntityFrameworkCore;
using OpenAI.ChatGpt.Models;

namespace OpenAI.ChatGpt.EntityFrameworkCore;

public class EfMessageStore : IMessageStore
{
    private readonly ChatGptDbContext _dbContext;

    public EfMessageStore(ChatGptDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IEnumerable<Topic>> GetTopics(string userId, CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        return await _dbContext.Topics.ToListAsync(cancellationToken: cancellationToken);
    }

    public Task<Topic> GetTopic(string userId, Guid topicId, CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        return _dbContext.Topics.FirstAsync(
            it => it.Id == topicId && it.UserId == userId,
            cancellationToken: cancellationToken);
    }

    public async Task AddTopic(Topic topic, CancellationToken cancellationToken)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        await _dbContext.Topics.AddAsync(topic, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

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

    public async Task<IEnumerable<PersistentChatMessage>> GetMessages(
        string userId, Guid topicId, CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        return await _dbContext.Messages
            .Where(it => it.TopicId == topicId && it.UserId == userId)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public Task<Topic?> GetLastTopicOrNull(string userId, CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        return _dbContext.Topics
            .Where(it => it.UserId == userId)
            .OrderByDescending(it => it.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public Task EnsureStorageCreated()
    {
        return _dbContext.Database.EnsureCreatedAsync();
    }
}