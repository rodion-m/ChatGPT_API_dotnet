using OpenAI.ChatGpt.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatGpt;

public interface IMessageStore
{
    Task<IEnumerable<Topic>> GetTopics(string userId, CancellationToken cancellationToken);
    Task<Topic> GetTopic(string userId, Guid topicId, CancellationToken cancellationToken);
    Task AddTopic(Topic topic, CancellationToken cancellationToken);

    Task SaveMessages(
        string userId,
        Guid topicId,
        IEnumerable<PersistentChatMessage> messages,
        CancellationToken cancellationToken
    );

    Task<IEnumerable<PersistentChatMessage>> GetMessages(
        string userId, 
        Guid topicId,
        CancellationToken cancellationToken
    );
    
    Task<Topic?> GetLastTopicOrNull(string userId, CancellationToken cancellationToken);

    Task SaveMessages(
        string userId,
        Guid topicId,
        UserOrSystemMessage message,
        string assistantMessage,
        CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (assistantMessage == null) throw new ArgumentNullException(nameof(assistantMessage));
        var now = Now();
        var enumerable = new PersistentChatMessage[]
        {
            new(NewMessageId(), userId, topicId, now, message),
            new(NewMessageId(), userId, topicId, now, ChatCompletionRoles.Assistant, assistantMessage),
        };
        return SaveMessages(userId, topicId, enumerable, cancellationToken);
    }

    Guid NewTopicId() => Guid.NewGuid();
    Guid NewMessageId() => Guid.NewGuid();
    DateTimeOffset Now() => DateTimeOffset.Now;
    Task EnsureStorageCreated();
}